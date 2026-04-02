using System;
using System.Collections.Generic;
using System.Threading;
using GodotTask.Internal;

namespace GodotTask;

public partial struct GDTask
{
    /// <summary>
    /// Creates a task that will complete when all of the supplied tasks have completed.
    /// </summary>
    /// <param name="tasks">The tasks to wait on for completion.</param>
    /// <typeparam name="T">The type of the result returned by the task.</typeparam>
    /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
    public static GDTask<T[]> WhenAll<T>(params GDTask<T>[] tasks)
    {
        if (tasks.Length == 0) return FromResult(Array.Empty<T>());
        return new(new WhenAllPromise<T>(tasks), 0);
    }

    /// <inheritdoc cref="WhenAll{T}(GDTask{T}[])" />
    public static GDTask<T[]> WhenAll<T>(params ReadOnlySpan<GDTask<T>> tasks)
    {
        if (tasks.Length == 0) return FromResult(Array.Empty<T>());
        return new(new WhenAllPromise<T>(tasks), 0);
    }


    /// <inheritdoc cref="WhenAll{T}(GDTask{T}[])" />
    public static GDTask<T[]> WhenAll<T>(IEnumerable<GDTask<T>> tasks)
    {
        using var usage = EnumerableExtensions.ToSpan(tasks, out var span);
        var promise = new WhenAllPromise<T>(span); // consumed array in constructor.
        return new(promise, 0);
    }

    /// <summary>
    /// Creates a task that will complete when all of the supplied tasks have completed.
    /// </summary>
    /// <param name="tasks">The tasks to wait on for completion.</param>
    /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
    public static GDTask WhenAll(params GDTask[] tasks)
    {
        if (tasks.Length == 0) return CompletedTask;

        if (tasks.Length == 1) return tasks[0];

        return new(new WhenAllPromise(tasks), 0);
    }

    /// <inheritdoc cref="WhenAll(GDTask[])" />
    public static GDTask WhenAll(params ReadOnlySpan<GDTask> tasks)
    {
        if (tasks.Length == 0) return CompletedTask;

        if (tasks.Length == 1) return tasks[0];

        return new(new WhenAllPromise(tasks), 0);
    }

    /// <inheritdoc cref="WhenAll(GDTask[])" />
    public static GDTask WhenAll(IEnumerable<GDTask> tasks)
    {
        using var usage = EnumerableExtensions.ToSpan(tasks, out var span);
        if (span.Length == 0) return CompletedTask;

        if (span.Length == 1) return span[0];
        var promise = new WhenAllPromise(span); // consumed array in constructor.
        return new(promise, 0);
    }

    private sealed class WhenAllPromise<T> : IGDTaskSource<T[]>
    {
        private readonly T[] _result;
        private int _completeCount;
        private GDTaskCompletionSourceCore<T[]> _core; // don't reset(called after GetResult, will invoke TrySetException.)

        public WhenAllPromise(ReadOnlySpan<GDTask<T>> tasks)
        {
            TaskTracker.TrackActiveTask(this, 3);

            _completeCount = 0;

            if (tasks.Length == 0)
            {
                _result = Array.Empty<T>();
                _core.TrySetResult(_result);
                return;
            }

            _result = new T[tasks.Length];

            for (var i = 0; i < tasks.Length; i++)
            {
                GDTask<T>.Awaiter awaiter;

                try { awaiter = tasks[i].GetAwaiter(); }
                catch (Exception ex)
                {
                    _core.TrySetException(ex);
                    continue;
                }

                if (awaiter.IsCompleted) TryInvokeContinuation(this, awaiter, i);
                else
                    awaiter.SourceOnCompleted(
                        state =>
                        {
                            using var t = (StateTuple<WhenAllPromise<T>, GDTask<T>.Awaiter, int>)state;
                            TryInvokeContinuation(t.Item1, t.Item2, t.Item3);
                        },
                        StateTuple.Create(this, awaiter, i)
                    );
            }
        }

        public T[] GetResult(short token)
        {
            TaskTracker.RemoveTracking(this);
            GC.SuppressFinalize(this);
            return _core.GetResult(token);
        }

        void IGDTaskSource.GetResult(short token) => GetResult(token);

        public GDTaskStatus GetStatus(short token) => _core.GetStatus(token);

        public GDTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token) => _core.OnCompleted(continuation, state, token);

        private static void TryInvokeContinuation(WhenAllPromise<T> self, in GDTask<T>.Awaiter awaiter, int i)
        {
            try { self._result[i] = awaiter.GetResult(); }
            catch (Exception ex)
            {
                self._core.TrySetException(ex);
                return;
            }

            if (Interlocked.Increment(ref self._completeCount) == self._result.Length) self._core.TrySetResult(self._result);
        }
    }

    private sealed class WhenAllPromise : IGDTaskSource
    {
        private readonly int _tasksLength;
        private int _completeCount;
        private GDTaskCompletionSourceCore<AsyncUnit> _core; // don't reset(called after GetResult, will invoke TrySetException.)

        public WhenAllPromise(ReadOnlySpan<GDTask> tasks)
        {
            TaskTracker.TrackActiveTask(this, 3);

            _tasksLength = tasks.Length;
            _completeCount = 0;

            if (_tasksLength == 0)
            {
                _core.TrySetResult(AsyncUnit.Default);
                return;
            }

            for (var i = 0; i < _tasksLength; i++)
            {
                Awaiter awaiter;

                try { awaiter = tasks[i].GetAwaiter(); }
                catch (Exception ex)
                {
                    _core.TrySetException(ex);
                    continue;
                }

                if (awaiter.IsCompleted) TryInvokeContinuation(this, awaiter);
                else
                    awaiter.SourceOnCompleted(
                        state =>
                        {
                            using var t = (StateTuple<WhenAllPromise, Awaiter>)state;
                            TryInvokeContinuation(t.Item1, t.Item2);
                        },
                        StateTuple.Create(this, awaiter)
                    );
            }
        }

        public void GetResult(short token)
        {
            TaskTracker.RemoveTracking(this);
            GC.SuppressFinalize(this);
            _core.GetResult(token);
        }

        public GDTaskStatus GetStatus(short token) => _core.GetStatus(token);

        public GDTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token) => _core.OnCompleted(continuation, state, token);

        private static void TryInvokeContinuation(WhenAllPromise self, in Awaiter awaiter)
        {
            try { awaiter.GetResult(); }
            catch (Exception ex)
            {
                self._core.TrySetException(ex);
                return;
            }

            if (Interlocked.Increment(ref self._completeCount) == self._tasksLength) self._core.TrySetResult(AsyncUnit.Default);
        }
    }
}