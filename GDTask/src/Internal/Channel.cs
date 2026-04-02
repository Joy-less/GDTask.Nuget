using System;
using System.Collections.Generic;
using System.Threading;

namespace GodotTask.Internal;

static class Channel
{
    public static Channel<T> CreateSingleConsumerUnbounded<T>() => new SingleConsumerUnboundedChannel<T>();
}

abstract class Channel<TWrite, TRead>
{
    public ChannelReader<TRead> Reader { get; protected set; }
    public ChannelWriter<TWrite> Writer { get; protected set; }

    public static implicit operator ChannelReader<TRead>(Channel<TWrite, TRead> channel) => channel.Reader;
    public static implicit operator ChannelWriter<TWrite>(Channel<TWrite, TRead> channel) => channel.Writer;
}

abstract class Channel<T> : Channel<T, T>;

abstract class ChannelReader<T>
{
    public abstract GDTask Completion { get; }
    public abstract bool TryRead(out T item);
    public abstract GDTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default);

    public virtual GDTask<T> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (TryRead(out var item)) return GDTask.FromResult(item);

        return ReadAsyncCore(cancellationToken);
    }

    private async GDTask<T> ReadAsyncCore(CancellationToken cancellationToken = default)
    {
        if (await WaitToReadAsync(cancellationToken))
            if (TryRead(out var item))
                return item;

        throw new ChannelClosedException();
    }

    public abstract IGDTaskAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default);
}

abstract class ChannelWriter<T>
{
    public abstract bool TryWrite(T item);
    public abstract bool TryComplete(Exception error = null);

    public void Complete(Exception error = null)
    {
        if (!TryComplete(error)) throw new ChannelClosedException();
    }
}

class ChannelClosedException : InvalidOperationException
{
    public ChannelClosedException() :
        base("Channel is already closed.") { }

    public ChannelClosedException(string message) : base(message) { }

    public ChannelClosedException(Exception innerException) :
        base("Channel is already closed", innerException) { }

    public ChannelClosedException(string message, Exception innerException) : base(message, innerException) { }
}

class SingleConsumerUnboundedChannel<T> : Channel<T>
{
    private readonly Queue<T> _items;
    private readonly SingleConsumerUnboundedChannelReader _readerSource;
    private bool _closed;
    private GDTask _completedTask;
    private GDTaskCompletionSource _completedTaskSource;

    private Exception _completionError;

    public SingleConsumerUnboundedChannel()
    {
        _items = new();
        Writer = new SingleConsumerUnboundedChannelWriter(this);
        _readerSource = new(this);
        Reader = _readerSource;
    }

    private sealed class SingleConsumerUnboundedChannelWriter(SingleConsumerUnboundedChannel<T> parent) : ChannelWriter<T>
    {
        public override bool TryWrite(T item)
        {
            bool waiting;

            lock (parent._items)
            {
                if (parent._closed)
                    return false;

                parent._items.Enqueue(item);
                waiting = parent._readerSource.IsWaiting;
            }

            if (waiting) parent._readerSource.SignalContinuation();

            return true;
        }

        public override bool TryComplete(Exception error = null)
        {

            lock (parent._items)
            {
                if (parent._closed)
                    return false;
                parent._closed = true;
                var waiting = parent._readerSource.IsWaiting;

                if (parent._items.Count == 0)
                {
                    if (error == null)
                    {
                        if (parent._completedTaskSource != null) parent._completedTaskSource.TrySetResult();
                        else parent._completedTask = GDTask.CompletedTask;
                    }
                    else
                    {
                        if (parent._completedTaskSource != null) parent._completedTaskSource.TrySetException(error);
                        else parent._completedTask = GDTask.FromException(error);
                    }

                    if (waiting) parent._readerSource.SignalCompleted(error);
                }

                parent._completionError = error;
            }

            return true;
        }
    }

    private sealed class SingleConsumerUnboundedChannelReader : ChannelReader<T>, IGDTaskSource<bool>
    {
        private readonly Action<object> _cancellationCallbackDelegate = CancellationCallback;
        private readonly SingleConsumerUnboundedChannel<T> _parent;

        private CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationTokenRegistration;
        private GDTaskCompletionSourceCore<bool> _core;
        internal bool IsWaiting;

        public SingleConsumerUnboundedChannelReader(SingleConsumerUnboundedChannel<T> parent)
        {
            _parent = parent;

            TaskTracker.TrackActiveTask(this, 4);
        }

        public override GDTask Completion
        {
            get
            {
                if (_parent._completedTaskSource != null)
                    return _parent._completedTaskSource.Task;

                if (_parent._closed) return _parent._completedTask;

                _parent._completedTaskSource = new();
                return _parent._completedTaskSource.Task;
            }
        }

        bool IGDTaskSource<bool>.GetResult(short token) => _core.GetResult(token);

        void IGDTaskSource.GetResult(short token) => _core.GetResult(token);

        GDTaskStatus IGDTaskSource.GetStatus(short token) => _core.GetStatus(token);

        void IGDTaskSource.OnCompleted(Action<object> continuation, object state, short token) => _core.OnCompleted(continuation, state, token);

        GDTaskStatus IGDTaskSource.UnsafeGetStatus() => _core.UnsafeGetStatus();

        public override bool TryRead(out T item)
        {
            lock (_parent._items)
            {
                if (_parent._items.Count != 0)
                {
                    item = _parent._items.Dequeue();

                    // complete when all value was consumed.
                    if (_parent._closed && _parent._items.Count == 0)
                    {
                        if (_parent._completionError != null)
                        {
                            if (_parent._completedTaskSource != null) _parent._completedTaskSource.TrySetException(_parent._completionError);
                            else _parent._completedTask = GDTask.FromException(_parent._completionError);
                        }
                        else
                        {
                            if (_parent._completedTaskSource != null) _parent._completedTaskSource.TrySetResult();
                            else _parent._completedTask = GDTask.CompletedTask;
                        }
                    }
                }
                else
                {
                    item = default;
                    return false;
                }
            }

            return true;
        }

        public override GDTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) return GDTask.FromCanceled<bool>(cancellationToken);

            lock (_parent._items)
            {
                if (_parent._items.Count != 0) return CompletedTasks.True;

                if (_parent._closed)
                {
                    if (_parent._completionError == null) return CompletedTasks.False;

                    return GDTask.FromException<bool>(_parent._completionError);
                }

                _cancellationTokenRegistration.Dispose();

                _core.Reset();
                IsWaiting = true;

                _cancellationToken = cancellationToken;
                if (_cancellationToken.CanBeCanceled) _cancellationTokenRegistration = _cancellationToken.RegisterWithoutCaptureExecutionContext(_cancellationCallbackDelegate, this);

                return new(this, _core.Version);
            }
        }

        public void SignalContinuation() => _core.TrySetResult(true);

        private void SignalCancellation(CancellationToken cancellationToken)
        {
            TaskTracker.RemoveTracking(this);
            _core.TrySetCanceled(cancellationToken);
        }

        public void SignalCompleted(Exception error)
        {
            TaskTracker.RemoveTracking(this);

            if (error != null)
            {
                _core.TrySetException(error);
            }
            else
            {
                _core.TrySetResult(false);
            }
        }

        public override IGDTaskAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default) => new ReadAllAsyncEnumerable(this, cancellationToken);

        private static void CancellationCallback(object state)
        {
            var self = (SingleConsumerUnboundedChannelReader)state;
            self.SignalCancellation(self._cancellationToken);
        }

        private sealed class ReadAllAsyncEnumerable(SingleConsumerUnboundedChannelReader parent, CancellationToken cancellationToken) : IGDTaskAsyncEnumerable<T>, IGDTaskAsyncEnumerator<T>
        {
            private readonly Action<object> _cancellationCallback1Delegate = CancellationCallback1;
            private readonly Action<object> _cancellationCallback2Delegate = CancellationCallback2;
            private readonly CancellationToken _cancellationToken1 = cancellationToken;

            private readonly SingleConsumerUnboundedChannelReader _parent = parent;
            private bool _cacheValue;
            private CancellationToken _cancellationToken2;
            private CancellationTokenRegistration _cancellationTokenRegistration1;
            private CancellationTokenRegistration _cancellationTokenRegistration2;

            private T _current;
            private bool _running;

            public IGDTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                if (_running) throw new InvalidOperationException("Enumerator is already running, does not allow call GetAsyncEnumerator twice.");

                if (_cancellationToken1 != cancellationToken) _cancellationToken2 = cancellationToken;

                if (_cancellationToken1.CanBeCanceled) _cancellationTokenRegistration1 = _cancellationToken1.RegisterWithoutCaptureExecutionContext(_cancellationCallback1Delegate, this);

                if (_cancellationToken2.CanBeCanceled) _cancellationTokenRegistration2 = _cancellationToken2.RegisterWithoutCaptureExecutionContext(_cancellationCallback2Delegate, this);

                _running = true;
                return this;
            }

            public T Current
            {
                get
                {
                    if (_cacheValue) return _current;

                    if (_parent.TryRead(out _current))
                    {
                        _cacheValue = true;
                    }

                    return _current;
                }
            }

            public GDTask<bool> MoveNextAsync()
            {
                _cacheValue = false;
                return _parent.WaitToReadAsync(CancellationToken.None); // ok to use None, registered in ctor.
            }

            public GDTask DisposeAsync()
            {
                _cancellationTokenRegistration1.Dispose();
                _cancellationTokenRegistration2.Dispose();
                return default;
            }

            private static void CancellationCallback1(object state)
            {
                var self = (ReadAllAsyncEnumerable)state;
                self._parent.SignalCancellation(self._cancellationToken1);
            }

            private static void CancellationCallback2(object state)
            {
                var self = (ReadAllAsyncEnumerable)state;
                self._parent.SignalCancellation(self._cancellationToken2);
            }
        }
    }
}