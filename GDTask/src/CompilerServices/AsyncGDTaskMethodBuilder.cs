#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace GodotTask.CompilerServices;

[StructLayout(LayoutKind.Auto)]
public struct AsyncGDTaskMethodBuilder
{
    private IStateMachineRunnerPromise _runnerPromise;
    private Exception _exception;

    // 1. Static Create method.
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncGDTaskMethodBuilder Create() => default;

    // 2. TaskLike Task property.
    public readonly GDTask Task
    {
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_runnerPromise != null) return _runnerPromise.Task;

            if (_exception != null) return GDTask.FromException(_exception);

            return GDTask.CompletedTask;
        }
    }

    // 3. SetException
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetException(Exception exception)
    {
        if (_runnerPromise == null) _exception = exception;
        else _runnerPromise.SetException(exception);
    }

    // 4. SetResult
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult()
    {
        if (_runnerPromise != null) _runnerPromise.SetResult();
    }

    // 5. AwaitOnCompleted
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (_runnerPromise == null) AsyncGDTask<TStateMachine>.SetStateMachine(ref stateMachine, ref _runnerPromise);

        awaiter.OnCompleted(_runnerPromise.MoveNext);
    }

    // 6. AwaitUnsafeOnCompleted
    [DebuggerHidden, SecuritySafeCritical, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (_runnerPromise == null) AsyncGDTask<TStateMachine>.SetStateMachine(ref stateMachine, ref _runnerPromise);

        awaiter.UnsafeOnCompleted(_runnerPromise.MoveNext);
    }

    // 7. Start
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        stateMachine.MoveNext();

    // 8. SetStateMachine
    [DebuggerHidden]
    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        // don't use boxed stateMachine.
    }
}

[StructLayout(LayoutKind.Auto)]
public struct AsyncGDTaskMethodBuilder<T>
{
    private IStateMachineRunnerPromise<T> runnerPromise;
    private Exception ex;
    private T result;

    // 1. Static Create method.
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncGDTaskMethodBuilder<T> Create() => default;

    // 2. TaskLike Task property.
    public readonly GDTask<T> Task
    {
        [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (runnerPromise != null) return runnerPromise.Task;

            if (ex != null) return GDTask.FromException<T>(ex);

            return GDTask.FromResult(result);
        }
    }

    // 3. SetException
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetException(Exception exception)
    {
        if (runnerPromise == null) ex = exception;
        else runnerPromise.SetException(exception);
    }

    // 4. SetResult
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult(T result)
    {
        if (runnerPromise == null) this.result = result;
        else runnerPromise.SetResult(result);
    }

    // 5. AwaitOnCompleted
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (runnerPromise == null) AsyncGDTask<TStateMachine, T>.SetStateMachine(ref stateMachine, ref runnerPromise);

        awaiter.OnCompleted(runnerPromise.MoveNext);
    }

    // 6. AwaitUnsafeOnCompleted
    [DebuggerHidden, SecuritySafeCritical, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (runnerPromise == null) AsyncGDTask<TStateMachine, T>.SetStateMachine(ref stateMachine, ref runnerPromise);

        awaiter.UnsafeOnCompleted(runnerPromise.MoveNext);
    }

    // 7. Start
    [DebuggerHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        stateMachine.MoveNext();

    // 8. SetStateMachine
    [DebuggerHidden]
    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        // don't use boxed stateMachine.
    }
}