using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GodotTask.CompilerServices;

namespace GodotTask;

static class AwaiterActions
{
    internal static readonly Action<object> InvokeContinuationDelegate = Continuation;

    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Continuation(object state) => ((Action)state).Invoke();
}

/// <summary>
/// Lightweight Godot specific task-like object with a void return value.
/// </summary>
[AsyncMethodBuilder(typeof(AsyncGDTaskMethodBuilder)), StructLayout(LayoutKind.Auto)]
public readonly partial struct GDTask : IGDTask
{
    private readonly IGDTaskSource source;
    private readonly short token;

    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal GDTask(IGDTaskSource source, short token)
    {
        this.source = source;
        this.token = token;
    }

    /// <summary>
    /// Gets the <see cref="GDTaskStatus" /> of this task.
    /// </summary>
    public GDTaskStatus Status
    {
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (source == null) return GDTaskStatus.Succeeded;
            return source.GetStatus(token);
        }
    }

    /// <summary>
    /// Gets an awaiter used to await this <see cref="GDTask" />.
    /// </summary>
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Awaiter GetAwaiter() => new(this);

    IGDTaskAwaiter IGDTask.GetAwaiter() => GetAwaiter();

    /// <summary>
    /// Create a <see cref="ValueTask" /> that wraps around this task.
    /// </summary>
    public ValueTask AsValueTask() => new(source, token);

    /// <summary>
    /// returns (bool IsCanceled) instead of throws OperationCanceledException.
    /// </summary>
    public GDTask<bool> SuppressCancellationThrow()
    {
        var status = Status;
        if (status == GDTaskStatus.Succeeded) return CompletedTasks.False;
        if (status == GDTaskStatus.Canceled) return CompletedTasks.True;
        return new(new IsCanceledSource(source), token);
    }

    /// <summary>
    /// Returns a string representation of the internal status for this <see cref="GDTask" />.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (source == null) return "()";
        return $"({source.UnsafeGetStatus()})";
    }

    /// <summary>
    /// Creates a <see cref="GDTask" /> allows to await multiple times.
    /// </summary>
    public GDTask Preserve()
    {
        if (source == null) return this;

        return new(new MemoizeSource(source), token);
    }

    /// <summary>
    /// Creates a <see cref="GDTask{AsyncUnit}" /> that represents this <see cref="GDTask" />.
    /// </summary>
    /// <returns></returns>
    public GDTask<AsyncUnit> AsAsyncUnitGDTask()
    {
        if (source == null) return CompletedTasks.AsyncUnit;

        var status = source.GetStatus(token);

        if (status.IsCompletedSuccessfully())
        {
            source.GetResult(token);
            return CompletedTasks.AsyncUnit;
        }

        if (source is IGDTaskSource<AsyncUnit> asyncUnitSource) return new(asyncUnitSource, token);

        return new(new AsyncUnitSource(source), token);
    }

    private sealed class AsyncUnitSource(IGDTaskSource source) : IGDTaskSource<AsyncUnit>
    {
        public AsyncUnit GetResult(short token)
        {
            source.GetResult(token);
            return AsyncUnit.Default;
        }

        public GDTaskStatus GetStatus(short token) => source.GetStatus(token);

        public void OnCompleted(Action<object> continuation, object state, short token) => source.OnCompleted(continuation, state, token);

        public GDTaskStatus UnsafeGetStatus() => source.UnsafeGetStatus();

        void IGDTaskSource.GetResult(short token) => GetResult(token);
    }

    private sealed class IsCanceledSource(IGDTaskSource source) : IGDTaskSource<bool>
    {
        public bool GetResult(short token)
        {
            if (source.GetStatus(token) == GDTaskStatus.Canceled) return true;

            source.GetResult(token);
            return false;
        }

        void IGDTaskSource.GetResult(short token) => GetResult(token);

        public GDTaskStatus GetStatus(short token) => source.GetStatus(token);

        public GDTaskStatus UnsafeGetStatus() => source.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token) => source.OnCompleted(continuation, state, token);
    }

    private sealed class MemoizeSource(IGDTaskSource source) : IGDTaskSource
    {
        private ExceptionDispatchInfo _exception;
        private IGDTaskSource _source = source;
        private GDTaskStatus _status;

        public void GetResult(short token)
        {
            if (_source == null)
            {
                if (_exception != null) _exception.Throw();
            }
            else
                try
                {
                    _source.GetResult(token);
                    _status = GDTaskStatus.Succeeded;
                }
                catch (Exception ex)
                {
                    _exception = ExceptionDispatchInfo.Capture(ex);
                    if (ex is OperationCanceledException) _status = GDTaskStatus.Canceled;
                    else _status = GDTaskStatus.Faulted;
                    throw;
                }
                finally { _source = null; }
        }

        public GDTaskStatus GetStatus(short token)
        {
            if (_source == null) return _status;

            return _source.GetStatus(token);
        }

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            if (_source == null) continuation(state);
            else _source.OnCompleted(continuation, state, token);
        }

        public GDTaskStatus UnsafeGetStatus()
        {
            if (_source == null) return _status;

            return _source.UnsafeGetStatus();
        }
    }

    /// <summary>
    /// Provides an awaiter for awaiting a <see cref="GDTask" />.
    /// </summary>
    public readonly struct Awaiter : IGDTaskAwaiter
    {
        private readonly GDTask _task;

        /// <summary>
        /// Initializes the <see cref="Awaiter" />.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter(in GDTask task)
        {
            _task = task;
        }

        /// <summary>
        /// Gets whether this <see cref="GDTask" /> has completed.
        /// </summary>
        public bool IsCompleted
        {
            [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task.Status.IsCompleted();
        }

        /// <summary>
        /// Ends the awaiting on the completed <see cref="GDTask" />.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult()
        {
            if (_task.source == null) return;
            _task.source.GetResult(_task.token);
        }

        object IGDTaskAwaiter.GetResult()
        {
            GetResult();
            return null;
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="GDTask" /> associated with this <see cref="Awaiter" />.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            if (_task.source == null) continuation();
            else _task.source.OnCompleted(AwaiterActions.InvokeContinuationDelegate, continuation, _task.token);
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="GDTask" /> associated with this <see cref="Awaiter" />.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action continuation)
        {
            if (_task.source == null) continuation();
            else _task.source.OnCompleted(AwaiterActions.InvokeContinuationDelegate, continuation, _task.token);
        }

        /// <summary>
        /// If register manually continuation, you can use it instead of for compiler OnCompleted methods.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SourceOnCompleted(Action<object> continuation, object state)
        {
            if (_task.source == null) continuation(state);
            else _task.source.OnCompleted(continuation, state, _task.token);
        }
    }
}

/// <summary>
/// Lightweight Godot specified task-like object with a return value.
/// </summary>
/// <typeparam name="T">Return value of the task</typeparam>
[AsyncMethodBuilder(typeof(AsyncGDTaskMethodBuilder<>)), StructLayout(LayoutKind.Auto)]
public readonly struct GDTask<T> : IGDTask
{
    private readonly IGDTaskSource<T> source;
    private readonly T result;
    private readonly short token;

    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal GDTask(T result)
    {
        source = default;
        token = default;
        this.result = result;
    }

    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal GDTask(IGDTaskSource<T> source, short token)
    {
        this.source = source;
        this.token = token;
        result = default;
    }

    /// <summary>
    /// Gets the <see cref="GDTaskStatus" /> of this task.
    /// </summary>
    public GDTaskStatus Status
    {
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => source == null ? GDTaskStatus.Succeeded : source.GetStatus(token);
    }

    /// <summary>
    /// Gets an awaiter used to await this <see cref="GDTask{T}" />.
    /// </summary>
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Awaiter GetAwaiter() => new(this);

    IGDTaskAwaiter IGDTask.GetAwaiter() => GetAwaiter();

    /// <summary>
    /// Creates a <see cref="GDTask{T}" /> allows to await multiple times.
    /// </summary>
    public GDTask<T> Preserve()
    {
        if (source == null) return this;

        return new(new MemoizeSource(source), token);
    }

    /// <summary>
    /// Creates a <see cref="GDTask" /> that represents this <see cref="GDTask{T}" />.
    /// </summary>
    public GDTask AsGDTask()
    {
        if (source == null) return GDTask.CompletedTask;

        var status = source.GetStatus(token);

        if (status.IsCompletedSuccessfully())
        {
            source.GetResult(token);
            return GDTask.CompletedTask;
        }

        // Converting GDTask<T> -> GDTask is zero overhead.
        return new(source, token);
    }

    /// <summary>
    /// Implicit operator for covert from <see cref="GDTask{T}" /> to <see cref="GDTask" />.
    /// </summary>
    public static implicit operator GDTask(GDTask<T> self) => self.AsGDTask();

    /// <summary>
    /// Create a <see cref="ValueTask{T}" /> that wraps around this task.
    /// </summary>
    public ValueTask<T> AsValueTask() => new(source, token);

    /// <summary>
    /// returns (bool IsCanceled, T Result) instead of throws OperationCanceledException.
    /// </summary>
    public GDTask<(bool IsCanceled, T Result)> SuppressCancellationThrow()
    {
        if (source == null) return new((false, result));

        return new GDTask<(bool, T)>(new IsCanceledSource(source), token);
    }

    /// <summary>
    /// Returns a string representation of the internal status for this <see cref="GDTask{T}" />.
    /// </summary>
    public override string ToString() =>
        source == null
            ? result?.ToString()
            : "(" + source.UnsafeGetStatus() + ")";

    [method: DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    private sealed class IsCanceledSource(IGDTaskSource<T> source) : IGDTaskSource<(bool, T)>
    {
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, T) GetResult(short token)
        {
            if (source.GetStatus(token) == GDTaskStatus.Canceled) return (true, default);

            var result = source.GetResult(token);
            return (false, result);
        }

        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IGDTaskSource.GetResult(short token) => GetResult(token);

        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GDTaskStatus GetStatus(short token) => source.GetStatus(token);

        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GDTaskStatus UnsafeGetStatus() => source.UnsafeGetStatus();

        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action<object> continuation, object state, short token) => source.OnCompleted(continuation, state, token);
    }

    private sealed class MemoizeSource(IGDTaskSource<T> source) : IGDTaskSource<T>
    {
        private ExceptionDispatchInfo _exception;
        private T _result;
        private IGDTaskSource<T> _source = source;
        private GDTaskStatus _status;

        public T GetResult(short token)
        {
            if (_source == null)
            {
                if (_exception != null) _exception.Throw();
                return _result;
            }

            try
            {
                _result = _source.GetResult(token);
                _status = GDTaskStatus.Succeeded;
                return _result;
            }
            catch (Exception ex)
            {
                _exception = ExceptionDispatchInfo.Capture(ex);
                if (ex is OperationCanceledException) _status = GDTaskStatus.Canceled;
                else _status = GDTaskStatus.Faulted;
                throw;
            }
            finally { _source = null; }
        }

        void IGDTaskSource.GetResult(short token) => GetResult(token);

        public GDTaskStatus GetStatus(short token)
        {
            if (_source == null) return _status;

            return _source.GetStatus(token);
        }

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            if (_source == null) continuation(state);
            else _source.OnCompleted(continuation, state, token);
        }

        public GDTaskStatus UnsafeGetStatus()
        {
            if (_source == null) return _status;

            return _source.UnsafeGetStatus();
        }
    }

    /// <summary>
    /// Provides an awaiter for awaiting a <see cref="GDTask{T}" />.
    /// </summary>
    public readonly struct Awaiter : IGDTaskAwaiter
    {
        private readonly GDTask<T> _task;

        /// <summary>
        /// Initializes the <see cref="Awaiter" />.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter(in GDTask<T> task)
        {
            _task = task;
        }

        /// <summary>
        /// Gets whether this <see cref="GDTask{T}">Task</see> has completed.
        /// </summary>
        public bool IsCompleted
        {
            [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _task.Status.IsCompleted();
        }

        /// <summary>
        /// Ends the awaiting on the completed <see cref="GDTask{T}" />.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetResult()
        {
            var s = _task.source;
            if (s == null) return _task.result;

            return s.GetResult(_task.token);
        }

        object IGDTaskAwaiter.GetResult() => GetResult();

        /// <summary>
        /// Schedules the continuation onto the <see cref="GDTask{T}" /> associated with this <see cref="Awaiter" />.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            var s = _task.source;
            if (s == null) continuation();
            else s.OnCompleted(AwaiterActions.InvokeContinuationDelegate, continuation, _task.token);
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="GDTask{T}" /> associated with this <see cref="Awaiter" />.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action continuation)
        {
            var s = _task.source;
            if (s == null) continuation();
            else s.OnCompleted(AwaiterActions.InvokeContinuationDelegate, continuation, _task.token);
        }

        /// <summary>
        /// If register manually continuation, you can use it instead of for compiler OnCompleted methods.
        /// </summary>
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SourceOnCompleted(Action<object> continuation, object state)
        {
            var s = _task.source;
            if (s == null) continuation(state);
            else s.OnCompleted(continuation, state, _task.token);
        }
    }
}