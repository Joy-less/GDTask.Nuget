using System;
using System.Runtime.CompilerServices;
using System.Threading;
using GodotTask.Internal;

namespace GodotTask;

public partial struct GDTask
{
    /// <summary>
    /// Creates an awaitable that asynchronously yields back to the next <see cref="PlayerLoopTiming.Process" /> from the main
    /// thread when awaited, with specified <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when awaited, will asynchronously transition back into the next <see cref="PlayerLoopTiming.Process" />
    /// from the main thread at the time of the await. This awaitable behaves identically as
    /// <see cref="Yield(CancellationToken)" /> in case the call site is from the main thread.
    /// </returns>
    public static SwitchToMainThreadAwaitable SwitchToMainThread(CancellationToken cancellationToken = default) => new(GDTaskScheduler.GetPlayerLoop(PlayerLoopTiming.Process), cancellationToken);

    /// <summary>
    /// Creates an awaitable that asynchronously yields back to the next provided <see cref="PlayerLoopTiming" /> from the main
    /// thread when awaited, with specified <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when awaited, will asynchronously transition back into the next provided
    /// <see cref="PlayerLoopTiming" /> from the main thread at the time of the await. This awaitable behaves identically as
    /// <see cref="Yield(PlayerLoopTiming, CancellationToken)" /> in case the call site is from the main thread.
    /// </returns>
    public static SwitchToMainThreadAwaitable SwitchToMainThread(PlayerLoopTiming timing, CancellationToken cancellationToken = default) => new(GDTaskScheduler.GetPlayerLoop(timing), cancellationToken);

    /// <summary>
    /// Creates an awaitable that asynchronously yields back to the next provided <see cref="IPlayerLoop" /> from the main
    /// thread when awaited, with specified <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when awaited, will asynchronously transition back into the next provided <see cref="IPlayerLoop" />
    /// from the main thread at the time of the await.
    /// </returns>
    public static SwitchToMainThreadAwaitable SwitchToMainThread(IPlayerLoop playerLoop, CancellationToken cancellationToken = default)
    {
        Error.ThrowArgumentNullException(playerLoop, nameof(playerLoop));
        return new(playerLoop, cancellationToken);
    }

    /// <summary>
    /// Creates an asynchronously disposable that asynchronously yields back to the next
    /// <see cref="PlayerLoopTiming.Process" /> from the main thread after using scope is closed, with specified
    /// <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when disposed, will asynchronously transition back into the next
    /// <see cref="PlayerLoopTiming.Process" /> from the main thread at the time of the dispose. This behaves identically as
    /// <see cref="Yield(CancellationToken)" /> in case the call site is from the main thread.
    /// </returns>
    public static ReturnToMainThread ReturnToMainThread(CancellationToken cancellationToken = default) => new(GDTaskScheduler.GetPlayerLoop(PlayerLoopTiming.Process), cancellationToken);

    /// <summary>
    /// Creates an asynchronously disposable that asynchronously yields back to the next provided
    /// <see cref="PlayerLoopTiming" /> from the main thread after using scope is closed, with specified
    /// <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when disposed, will asynchronously transition back into the next provided
    /// <see cref="PlayerLoopTiming" /> from the main thread at the time of the dispose. This behaves identically as
    /// <see cref="Yield(PlayerLoopTiming, CancellationToken)" /> in case the call site is from the main thread.
    /// </returns>
    public static ReturnToMainThread ReturnToMainThread(PlayerLoopTiming timing, CancellationToken cancellationToken = default) => new(GDTaskScheduler.GetPlayerLoop(timing), cancellationToken);

    /// <summary>
    /// Creates an asynchronously disposable that asynchronously yields back to the next provided <see cref="IPlayerLoop" />
    /// from the main thread after using scope is closed, with specified <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when disposed, will asynchronously transition back into the next provided <see cref="IPlayerLoop" />
    /// from the main thread at the time of the dispose.
    /// </returns>
    public static ReturnToMainThread ReturnToMainThread(IPlayerLoop playerLoop, CancellationToken cancellationToken = default)
    {
        Error.ThrowArgumentNullException(playerLoop, nameof(playerLoop));
        return new(playerLoop, cancellationToken);
    }

    /// <summary>
    /// Queue the action execution to the next specified <see cref="PlayerLoopTiming" />.
    /// </summary>
    public static void Post(Action action, PlayerLoopTiming timing = PlayerLoopTiming.Process) => GDTaskScheduler.AddContinuation(timing, action);

    /// <summary>
    /// Queue the action execution to the next specified <see cref="IPlayerLoop" />.
    /// </summary>
    public static void Post(Action action, IPlayerLoop playerLoop)
    {
        Error.ThrowArgumentNullException(playerLoop, nameof(playerLoop));
        GDTaskScheduler.AddContinuation(playerLoop, action);
    }

    /// <summary>
    /// Creates an awaitable that asynchronously yields to <see cref="ThreadPool" /> when awaited.
    /// </summary>
    /// <returns>
    /// A context that, when awaited, will asynchronously transition to <see cref="ThreadPool" /> at the time of the await.
    /// </returns>
    public static SwitchToThreadPoolAwaitable SwitchToThreadPool() => new();

    /// <summary>
    /// Creates an awaitable that asynchronously yields to the provided <see cref="SynchronizationContext" /> when awaited,
    /// with specified <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when awaited, will asynchronously transition to the provided <see cref="SynchronizationContext" /> at
    /// the time of the await.
    /// </returns>
    public static SwitchToSynchronizationContextAwaitable SwitchToSynchronizationContext(SynchronizationContext synchronizationContext, CancellationToken cancellationToken = default)
    {
        Error.ThrowArgumentNullException(synchronizationContext, nameof(synchronizationContext));
        return new(synchronizationContext, cancellationToken);
    }

    /// <summary>
    /// Creates an asynchronously disposable that asynchronously yields back to the provided
    /// <see cref="SynchronizationContext" /> after using scope is closed, with specified <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when disposed, will asynchronously transition back into the provided
    /// <see cref="SynchronizationContext" /> at the time of the dispose.
    /// </returns>
    public static ReturnToSynchronizationContext ReturnToSynchronizationContext(SynchronizationContext synchronizationContext, CancellationToken cancellationToken = default) => new(synchronizationContext, false, cancellationToken);

    /// <summary>
    /// Creates an asynchronously disposable that asynchronously yields back to the
    /// <see cref="SynchronizationContext.Current" /> after using scope is closed, with specified
    /// <see cref="CancellationToken" />.
    /// </summary>
    /// <returns>
    /// A context that, when disposed, will asynchronously transition back into the provided
    /// <see cref="SynchronizationContext.Current" /> at the time of the dispose.
    /// </returns>
    public static ReturnToSynchronizationContext ReturnToCurrentSynchronizationContext(bool dontPostWhenSameContext = true, CancellationToken cancellationToken = default) => new(SynchronizationContext.Current, dontPostWhenSameContext, cancellationToken);
}

/// <summary>
/// An awaitable that, when awaited, will asynchronously yields back to the next <see cref="IPlayerLoop" />.
/// </summary>
public readonly struct SwitchToMainThreadAwaitable
{
    internal readonly IPlayerLoop PlayerLoop;
    internal readonly CancellationToken CancellationToken;

    internal SwitchToMainThreadAwaitable(IPlayerLoop playerLoop, CancellationToken cancellationToken)
    {
        PlayerLoop = playerLoop;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets an awaiter used to await this <see cref="SwitchToMainThreadAwaitable" />.
    /// </summary>
    public Awaiter GetAwaiter() => new(PlayerLoop, CancellationToken);

    /// <summary>
    /// Provides an awaiter for awaiting a <see cref="SwitchToMainThreadAwaitable" />.
    /// </summary>
    public readonly struct Awaiter : ICriticalNotifyCompletion
    {
        private readonly IPlayerLoop _playerLoop;
        private readonly CancellationToken _cancellationToken;

        internal Awaiter(IPlayerLoop playerLoop, CancellationToken cancellationToken)
        {
            _playerLoop = playerLoop;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets whether this <see cref="SwitchToMainThreadAwaitable">Task</see> has completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                var currentThreadId = Environment.CurrentManagedThreadId;
                if (GDTaskScheduler.MainThreadId == currentThreadId) return true; // run immediate.

                return false; // register continuation.
            }
        }

        /// <summary>
        /// Ends the awaiting on the completed <see cref="SwitchToMainThreadAwaitable" />.
        /// </summary>
        public void GetResult() => _cancellationToken.ThrowIfCancellationRequested();

        /// <summary>
        /// Schedules the continuation onto the <see cref="SwitchToMainThreadAwaitable" /> associated with this
        /// <see cref="Awaiter" />.
        /// </summary>
        public void OnCompleted(Action continuation) => GDTaskScheduler.AddContinuation(_playerLoop, continuation);

        /// <summary>
        /// Schedules the continuation onto the <see cref="SwitchToMainThreadAwaitable" /> associated with this
        /// <see cref="Awaiter" />.
        /// </summary>
        public void UnsafeOnCompleted(Action continuation) => GDTaskScheduler.AddContinuation(_playerLoop, continuation);
    }
}

/// <summary>
/// An context that, when disposed, will asynchronously yields back to the next specified <see cref="IPlayerLoop" /> on the
/// main thread.
/// </summary>
public readonly struct ReturnToMainThread
{
    private readonly IPlayerLoop _playerLoop;
    private readonly CancellationToken _cancellationToken;

    internal ReturnToMainThread(IPlayerLoop playerLoop, CancellationToken cancellationToken)
    {
        _playerLoop = playerLoop;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Dispose this context and asynchronously yields back to the next specified <see cref="PlayerLoopTiming" /> on the main
    /// thread.
    /// </summary>
    public Awaiter DisposeAsync() => new(_playerLoop, _cancellationToken); // run immediate.

    /// <summary>
    /// Provides an awaiter for awaiting a <see cref="ReturnToMainThread" />.
    /// </summary>
    public readonly struct Awaiter : ICriticalNotifyCompletion
    {
        private readonly IPlayerLoop _playerLoop;
        private readonly CancellationToken _cancellationToken;

        internal Awaiter(IPlayerLoop playerLoop, CancellationToken cancellationToken)
        {
            _playerLoop = playerLoop;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Return self
        /// </summary>
        public Awaiter GetAwaiter() => this;

        /// <summary>
        /// Gets whether the current <see cref="GDTaskScheduler.MainThreadId" /> is
        /// <see cref="Environment.CurrentManagedThreadId" />.
        /// </summary>
        public bool IsCompleted => GDTaskScheduler.MainThreadId == Environment.CurrentManagedThreadId;

        /// <summary>
        /// Ends the awaiting on the completed <see cref="ReturnToMainThread" />.
        /// </summary>
        public void GetResult() => _cancellationToken.ThrowIfCancellationRequested();

        /// <summary>
        /// Schedules the continuation onto the <see cref="ReturnToMainThread" /> associated with this <see cref="Awaiter" />.
        /// </summary>
        public void OnCompleted(Action continuation) => GDTaskScheduler.AddContinuation(_playerLoop, continuation);

        /// <summary>
        /// Schedules the continuation onto the <see cref="ReturnToMainThread" /> associated with this <see cref="Awaiter" />.
        /// </summary>
        public void UnsafeOnCompleted(Action continuation) => GDTaskScheduler.AddContinuation(_playerLoop, continuation);
    }
}

/// <summary>
/// An context that, when disposed, will asynchronously yields to the thread pool.
/// </summary>
public readonly struct SwitchToThreadPoolAwaitable
{
    /// <summary>
    /// Initializes the <see cref="SwitchToThreadPoolAwaitable" />.
    /// </summary>
    public SwitchToThreadPoolAwaitable() { }

    /// <summary>
    /// Gets an awaiter used to await this <see cref="SwitchToThreadPoolAwaitable" />.
    /// </summary>
    public Awaiter GetAwaiter() => new();

    /// <summary>
    /// Provides an awaiter for awaiting a <see cref="SwitchToThreadPoolAwaitable" />.
    /// </summary>
    public readonly struct Awaiter : ICriticalNotifyCompletion
    {
        private static readonly WaitCallback SwitchToCallback = Callback;

        /// <summary>
        /// Initializes the <see cref="Awaiter" />.
        /// </summary>
        public Awaiter() { }

        /// <summary>
        /// Gets whether this <see cref="SwitchToThreadPoolAwaitable" /> has completed, always returns false.
        /// </summary>
        public bool IsCompleted => false;

        /// <summary>
        /// Do nothing
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Schedules the continuation onto the <see cref="SwitchToThreadPoolAwaitable" /> associated with this
        /// <see cref="Awaiter" />.
        /// </summary>
        public void OnCompleted(Action continuation) => ThreadPool.QueueUserWorkItem(SwitchToCallback, continuation);

        /// <summary>
        /// Schedules the continuation onto the <see cref="SwitchToThreadPoolAwaitable" /> associated with this
        /// <see cref="Awaiter" />.
        /// </summary>
        public void UnsafeOnCompleted(Action continuation) => ThreadPool.UnsafeQueueUserWorkItem(SwitchToCallback, continuation);

        private static void Callback(object state)
        {
            var continuation = (Action)state;
            continuation();
        }
    }
}

/// <summary>
/// An awaitable that asynchronously yields to the provided <see cref="System.Threading.SynchronizationContext" /> when
/// awaited.
/// </summary>
public readonly struct SwitchToSynchronizationContextAwaitable
{
    private readonly SynchronizationContext _synchronizationContext;
    private readonly CancellationToken _cancellationToken;

    internal SwitchToSynchronizationContextAwaitable(SynchronizationContext synchronizationContext, CancellationToken cancellationToken)
    {
        _synchronizationContext = synchronizationContext;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets an awaiter used to await this <see cref="SwitchToSynchronizationContextAwaitable" />.
    /// </summary>
    public Awaiter GetAwaiter() => new(_synchronizationContext, _cancellationToken);

    /// <summary>
    /// Provides an awaiter for awaiting a <see cref="SwitchToSynchronizationContextAwaitable" />.
    /// </summary>
    public readonly struct Awaiter : ICriticalNotifyCompletion
    {
        private static readonly SendOrPostCallback SwitchToCallback = Callback;

        private readonly SynchronizationContext _synchronizationContext;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes the <see cref="Awaiter" />.
        /// </summary>
        public Awaiter(SynchronizationContext synchronizationContext, CancellationToken cancellationToken)
        {
            _synchronizationContext = synchronizationContext;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets whether this <see cref="SwitchToSynchronizationContextAwaitable" /> has completed, always returns false.
        /// </summary>
        public bool IsCompleted => false;

        /// <summary>
        /// Ends the awaiting on the completed <see cref="SwitchToSynchronizationContextAwaitable" />.
        /// </summary>
        public void GetResult() => _cancellationToken.ThrowIfCancellationRequested();

        /// <summary>
        /// Schedules the continuation onto the <see cref="SwitchToSynchronizationContextAwaitable" /> associated with this
        /// <see cref="Awaiter" />.
        /// </summary>
        public void OnCompleted(Action continuation) => _synchronizationContext.Post(SwitchToCallback, continuation);

        /// <summary>
        /// Schedules the continuation onto the <see cref="SwitchToSynchronizationContextAwaitable" /> associated with this
        /// <see cref="Awaiter" />.
        /// </summary>
        public void UnsafeOnCompleted(Action continuation) => _synchronizationContext.Post(SwitchToCallback, continuation);

        private static void Callback(object state)
        {
            var continuation = (Action)state;
            continuation();
        }
    }
}

/// <summary>
/// An context that, when disposed, will asynchronously yields back to the previous <see cref="SynchronizationContext" />.
/// </summary>
public readonly struct ReturnToSynchronizationContext
{
    private readonly SynchronizationContext _syncContext;
    private readonly bool _dontPostWhenSameContext;
    public readonly CancellationToken CancellationToken;

    internal ReturnToSynchronizationContext(SynchronizationContext syncContext, bool dontPostWhenSameContext, CancellationToken cancellationToken)
    {
        _syncContext = syncContext;
        _dontPostWhenSameContext = dontPostWhenSameContext;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Dispose this context and asynchronously yields back to the previous <see cref="SynchronizationContext" />.
    /// </summary>
    public Awaiter DisposeAsync() => new(_syncContext, _dontPostWhenSameContext, CancellationToken);

    /// <summary>
    /// Provides an awaiter for awaiting a <see cref="ReturnToSynchronizationContext" />.
    /// </summary>
    public readonly struct Awaiter : ICriticalNotifyCompletion
    {
        private static readonly SendOrPostCallback SwitchToCallback = Callback;

        private readonly SynchronizationContext _synchronizationContext;
        private readonly bool _dontPostWhenSameContext;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes the <see cref="Awaiter" />.
        /// </summary>
        public Awaiter(SynchronizationContext synchronizationContext, bool dontPostWhenSameContext, CancellationToken cancellationToken)
        {
            _synchronizationContext = synchronizationContext;
            _dontPostWhenSameContext = dontPostWhenSameContext;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Return self
        /// </summary>
        public Awaiter GetAwaiter() => this;

        /// <summary>
        /// Gets whether the <see cref="SynchronizationContext.Current" /> synchronizationContext is the captured one.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                if (!_dontPostWhenSameContext) return false;

                var current = SynchronizationContext.Current;
                if (current == _synchronizationContext) return true;

                return false;
            }
        }

        /// <summary>
        /// Ends the awaiting on the completed <see cref="ReturnToSynchronizationContext" />.
        /// </summary>
        public void GetResult() => _cancellationToken.ThrowIfCancellationRequested();

        /// <summary>
        /// Schedules the continuation onto the <see cref="ReturnToSynchronizationContext" /> associated with this
        /// <see cref="Awaiter" />.
        /// </summary>
        public void OnCompleted(Action continuation) => _synchronizationContext.Post(SwitchToCallback, continuation);

        /// <summary>
        /// Schedules the continuation onto the <see cref="ReturnToSynchronizationContext" /> associated with this
        /// <see cref="Awaiter" />.
        /// </summary>
        public void UnsafeOnCompleted(Action continuation) => _synchronizationContext.Post(SwitchToCallback, continuation);

        private static void Callback(object state)
        {
            var continuation = (Action)state;
            continuation();
        }
    }
}