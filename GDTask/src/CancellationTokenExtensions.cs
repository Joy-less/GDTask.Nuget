using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GodotTask;

/// <summary>
/// Provides extensions methods for <see cref="GDTask" /> on <see cref="CancellationToken" /> related use cases.
/// </summary>
public static class CancellationTokenExtensions
{
    extension(GDTask task)
    {
        /// <summary>
        /// Creates a <see cref="CancellationToken" /> from the specified <see cref="GDTask" /> that cancels after it completes.
        /// </summary>
        public CancellationToken ToCancellationToken()
        {
            var cts = new CancellationTokenSource();
            ToCancellationTokenCore(task, cts).Forget();
            return cts.Token;
        }

        /// <summary>
        /// Creates a <see cref="CancellationToken" /> from the specified <see cref="GDTask" /> that cancels after it completes or
        /// is canceled by the linked <paramref name="linkToken" />.
        /// </summary>
        public CancellationToken ToCancellationToken(CancellationToken linkToken)
        {
            if (linkToken.IsCancellationRequested) return linkToken;

            if (!linkToken.CanBeCanceled) return task.ToCancellationToken();

            var cts = CancellationTokenSource.CreateLinkedTokenSource(linkToken);
            ToCancellationTokenCore(task, cts).Forget();

            return cts.Token;
        }
    }

    extension<T>(GDTask<T> task)
    {
        /// <inheritdoc cref="ToCancellationToken(GDTask)" />
        public CancellationToken ToCancellationToken() => task.AsGDTask().ToCancellationToken();

        /// <inheritdoc cref="ToCancellationToken(GDTask, CancellationToken)" />
        public CancellationToken ToCancellationToken(CancellationToken linkToken) => task.AsGDTask().ToCancellationToken(linkToken);
    }

    private static async GDTaskVoid ToCancellationTokenCore(GDTask task, CancellationTokenSource cts)
    {
        try { await task; }
        catch (Exception ex) { GDTaskExceptionHandler.PublishUnobservedTaskException(ex); }

        cts.Cancel();
        cts.Dispose();
    }

    extension(CancellationToken cancellationToken)
    {
        /// <summary>
        /// Creates a task and <see cref="CancellationTokenRegistration" /> that will complete when the specified
        /// <see cref="CancellationToken" /> is canceled.
        /// </summary>
        public (GDTask, CancellationTokenRegistration) ToGDTask()
        {
            if (cancellationToken.IsCancellationRequested) return (GDTask.FromCanceled(cancellationToken), default);

            var promise = new GDTaskCompletionSource();
            return (promise.Task, cancellationToken.RegisterWithoutCaptureExecutionContext(Callback, promise));
        }
        
        /// <summary>
        /// Creates a <see cref="CancellationTokenAwaitable" /> that will complete when the specified
        /// <see cref="CancellationToken" /> is canceled.
        /// </summary>
        /// <returns></returns>
        public CancellationTokenAwaitable WaitUntilCanceled() => new(cancellationToken);

        /// <summary>
        /// Register a <see cref="Action" /> to the supplied a <see cref="CancellationToken" /> and returns a
        /// <see cref="CancellationTokenRegistration" />.
        /// </summary>
        /// <returns>A <see cref="CancellationTokenRegistration" /> that unregister the <paramref name="callback" /> when disposed.</returns>
        public CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(Action callback)
        {
            var restoreFlow = false;

            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            try { return cancellationToken.Register(callback, false); }
            finally
            {
                if (restoreFlow) ExecutionContext.RestoreFlow();
            }
        }

        /// <inheritdoc cref="RegisterWithoutCaptureExecutionContext(System.Threading.CancellationToken,System.Action)" />
        public CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(Action<object> callback, object state)
        {
            var restoreFlow = false;

            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            try { return cancellationToken.Register(callback, state, false); }
            finally
            {
                if (restoreFlow) ExecutionContext.RestoreFlow();
            }
        }
    }

    private static void Callback(object state)
    {
        var promise = (GDTaskCompletionSource)state;
        promise.TrySetResult();
    }

    /// <summary>
    /// Register this <see cref="IDisposable" /> to a <see cref="CancellationToken" />  that gets disposed when the token is
    /// canceled.
    /// </summary>
    public static CancellationTokenRegistration AddTo(this IDisposable disposable, CancellationToken cancellationToken) => cancellationToken.RegisterWithoutCaptureExecutionContext(DisposeCallback, disposable);

    private static void DisposeCallback(object state)
    {
        var d = (IDisposable)state;
        d.Dispose();
    }
}

/// <summary>
/// A context that will complete when the associated <see cref="CancellationToken" /> is canceled.
/// </summary>
public readonly struct CancellationTokenAwaitable
{
    private readonly CancellationToken _cancellationToken;

    internal CancellationTokenAwaitable(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets an awaiter used to await this <see cref="CancellationTokenAwaitable" />.
    /// </summary>
    public Awaiter GetAwaiter() => new(_cancellationToken);

    /// <summary>
    /// Provides an object that waits for the completion of a <see cref="CancellationToken" />.
    /// </summary>
    public readonly struct Awaiter : ICriticalNotifyCompletion
    {
        private readonly CancellationToken _cancellationToken;

        internal Awaiter(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="CancellationToken" /> has canceled.
        /// </summary>
        public bool IsCompleted => !_cancellationToken.CanBeCanceled || _cancellationToken.IsCancellationRequested;

        /// <summary>
        /// Do nothing
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Sets the action to perform when the <see cref="Awaiter" /> <see cref="CancellationToken" /> has canceled.
        /// </summary>
        public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

        /// <inheritdoc cref="OnCompleted" />
        public void UnsafeOnCompleted(Action continuation) => _cancellationToken.RegisterWithoutCaptureExecutionContext(continuation);
    }
}