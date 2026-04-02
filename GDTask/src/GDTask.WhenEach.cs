using System;
using System.Collections.Generic;
using System.Threading;
using GodotTask.Internal;

namespace GodotTask;

public partial struct GDTask
{
    /// <summary>
    /// Creates an <see cref="IGDTaskAsyncEnumerable{T}" /> that will yield the supplied tasks as those tasks complete.
    /// </summary>
    /// <param name="tasks">The task to iterate through when completed.</param>
    /// <returns>An <see cref="IGDTaskAsyncEnumerable{T}" /> for iterating through the supplied tasks.</returns>
    /// <remarks>
    /// The supplied tasks will become available to be output via the enumerable once they've completed. The exact order
    /// in which the tasks will become available is not defined.
    /// </remarks>
    public static IGDTaskAsyncEnumerable<GDTask<T>> WhenEach<T>(params GDTask<T>[] tasks) => new WhenEachEnumerable<T>(tasks);

    /// <inheritdoc cref="WhenEach{T}(GDTask{T}[])" />
    public static IGDTaskAsyncEnumerable<GDTask<T>> WhenEach<T>(IEnumerable<GDTask<T>> tasks) => new WhenEachEnumerable<T>(tasks);

    /// <inheritdoc cref="WhenEach{T}(GDTask{T}[])" />
    public static IGDTaskAsyncEnumerable<GDTask> WhenEach(params GDTask[] tasks) => new WhenEachEnumerable(tasks);

    /// <inheritdoc cref="WhenEach(GDTask[])" />
    public static IGDTaskAsyncEnumerable<GDTask> WhenEach(IEnumerable<GDTask> tasks) => new WhenEachEnumerable(tasks);
}

sealed class WhenEachEnumerable<T>(IEnumerable<GDTask<T>> source) : IGDTaskAsyncEnumerable<GDTask<T>>
{
    public IGDTaskAsyncEnumerator<GDTask<T>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new Enumerator(source, cancellationToken);

    private sealed class Enumerator(IEnumerable<GDTask<T>> source, CancellationToken cancellationToken) : IGDTaskAsyncEnumerator<GDTask<T>>
    {
        private Channel<GDTask<T>> _channel;
        private IGDTaskAsyncEnumerator<GDTask<T>> _channelEnumerator;
        private int _completeCount;
        private WhenEachState _state;

        public GDTask<T> Current => _channelEnumerator.Current;

        public GDTask<bool> MoveNextAsync()
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_state == WhenEachState.NotRunning)
            {
                _channel = Channel.CreateSingleConsumerUnbounded<GDTask<T>>();
                _channelEnumerator = _channel.Reader.ReadAllAsync().GetAsyncEnumerator(cancellationToken);
                _state = WhenEachState.Running;

                using var usage = EnumerableExtensions.ToSpan(source, out var span);
                if (span.Length == 0)
                {
                    _state = WhenEachState.Completed;
                    _channel.Writer.TryComplete();
                }

                foreach (var task in span) RunWhenEachTask(this, task, span.Length).Forget();
            }

            return _channelEnumerator.MoveNextAsync();
        }

        public async GDTask DisposeAsync()
        {
            if (_channelEnumerator != null) await _channelEnumerator.DisposeAsync();

            if (_state != WhenEachState.Completed)
            {
                _state = WhenEachState.Completed;
                _channel?.Writer.TryComplete(new OperationCanceledException());
            }
        }

        private static async GDTaskVoid RunWhenEachTask(Enumerator self, GDTask<T> task, int length)
        {
            try
            {
                var result = await task;
                self._channel.Writer.TryWrite(GDTask.FromResult(result));
            }
            catch (Exception ex) { self._channel.Writer.TryWrite(GDTask.FromException<T>(ex)); }

            if (Interlocked.Increment(ref self._completeCount) == length)
            {
                self._state = WhenEachState.Completed;
                self._channel.Writer.TryComplete();
            }
        }

        private enum WhenEachState : byte
        {
            NotRunning,
            Running,
            Completed,
        }
    }
}

sealed class WhenEachEnumerable(IEnumerable<GDTask> source) : IGDTaskAsyncEnumerable<GDTask>
{
    public IGDTaskAsyncEnumerator<GDTask> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new Enumerator(source, cancellationToken);

    private sealed class Enumerator(IEnumerable<GDTask> source, CancellationToken cancellationToken) : IGDTaskAsyncEnumerator<GDTask>
    {
        private Channel<GDTask> _channel;
        private IGDTaskAsyncEnumerator<GDTask> _channelEnumerator;
        private int _completeCount;
        private WhenEachState _state;

        public GDTask Current => _channelEnumerator.Current;

        public GDTask<bool> MoveNextAsync()
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_state == WhenEachState.NotRunning)
            {
                _channel = Channel.CreateSingleConsumerUnbounded<GDTask>();
                _channelEnumerator = _channel.Reader.ReadAllAsync().GetAsyncEnumerator(cancellationToken);
                _state = WhenEachState.Running;

                using var usage = EnumerableExtensions.ToSpan(source, out var span);
                if (span.Length == 0)
                {
                    _state = WhenEachState.Completed;
                    _channel.Writer.TryComplete();
                }

                foreach (var task in span) RunWhenEachTask(this, task, span.Length).Forget();
            }

            return _channelEnumerator.MoveNextAsync();
        }

        public async GDTask DisposeAsync()
        {
            if (_channelEnumerator != null) await _channelEnumerator.DisposeAsync();

            if (_state != WhenEachState.Completed)
            {
                _state = WhenEachState.Completed;
                _channel?.Writer.TryComplete(new OperationCanceledException());
            }
        }

        private static async GDTaskVoid RunWhenEachTask(Enumerator self, GDTask task, int length)
        {
            try
            {
                await task;
                self._channel.Writer.TryWrite(GDTask.CompletedTask);
            }
            catch (Exception ex) { self._channel.Writer.TryWrite(GDTask.FromException(ex)); }

            if (Interlocked.Increment(ref self._completeCount) == length)
            {
                self._state = WhenEachState.Completed;
                self._channel.Writer.TryComplete();
            }
        }

        private enum WhenEachState : byte
        {
            NotRunning,
            Running,
            Completed,
        }
    }
}