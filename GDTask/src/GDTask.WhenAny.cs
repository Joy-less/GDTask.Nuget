using System;
using System.Collections.Generic;
using System.Threading;
using GodotTask.Internal;

namespace GodotTask;

public partial struct GDTask
{
    /// <summary>
    /// Creates a task that will complete when any of the supplied tasks have completed.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the task.</typeparam>
    /// <returns>A task that represents the info for the first completed task.</returns>
    public static GDTask<(bool hasResultLeft, T result)> WhenAny<T>(GDTask<T> leftTask, GDTask rightTask) => new(new WhenAnyLrPromise<T>(leftTask, rightTask), 0);

    /// <inheritdoc cref="WhenAny{T}(GDTask{T},GDTask)" />
    public static GDTask<(int winArgumentIndex, T result)> WhenAny<T>(params GDTask<T>[] tasks) => new(new WhenAnyPromise<T>(tasks.AsSpan()), 0);

    /// <inheritdoc cref="WhenAny{T}(GDTask{T},GDTask)" />
    public static GDTask<(int winArgumentIndex, T result)> WhenAny<T>(params ReadOnlySpan<GDTask<T>> tasks) => new(new WhenAnyPromise<T>(tasks), 0);

    /// <inheritdoc cref="WhenAny{T}(GDTask{T},GDTask)" />
    public static GDTask<(int winArgumentIndex, T result)> WhenAny<T>(IEnumerable<GDTask<T>> tasks)
    {
        using var usage = EnumerableExtensions.ToSpan(tasks, out var span);
        return new GDTask<(int, T)>(new WhenAnyPromise<T>(span), 0);
    }

    /// <summary>
    /// Creates a task that will complete when any of the supplied tasks have completed.
    /// </summary>
    /// <returns>A task that evaluates the index of the first completed task.</returns>
    public static GDTask<int> WhenAny(params GDTask[] tasks) => new(new WhenAnyPromise(tasks.AsSpan()), 0);

    /// <inheritdoc cref="WhenAny(GDTask[])" />
    public static GDTask<int> WhenAny(params ReadOnlySpan<GDTask> tasks) => new(new WhenAnyPromise(tasks), 0);

    /// <inheritdoc cref="WhenAny(GDTask[])" />
    public static GDTask<int> WhenAny(IEnumerable<GDTask> tasks)
    {
        using var usage = EnumerableExtensions.ToSpan(tasks, out var span);
        return new(new WhenAnyPromise(span), 0);
    }

    private sealed class WhenAnyLrPromise<T> : IGDTaskSource<(bool, T)>
    {
        private int _completedCount;
        private GDTaskCompletionSourceCore<(bool, T)> _core;

        public WhenAnyLrPromise(GDTask<T> leftTask, GDTask rightTask)
        {
            TaskTracker.TrackActiveTask(this, 3);

            {
                GDTask<T>.Awaiter awaiter;

                try { awaiter = leftTask.GetAwaiter(); }
                catch (Exception ex)
                {
                    _core.TrySetException(ex);
                    goto RIGHT;
                }

                if (awaiter.IsCompleted) TryLeftInvokeContinuation(this, awaiter);
                else
                    awaiter.SourceOnCompleted(
                        state =>
                        {
                            using var t = (StateTuple<WhenAnyLrPromise<T>, GDTask<T>.Awaiter>)state;
                            TryLeftInvokeContinuation(t.Item1, t.Item2);
                        },
                        StateTuple.Create(this, awaiter)
                    );
            }
            RIGHT:
            {
                Awaiter awaiter;

                try { awaiter = rightTask.GetAwaiter(); }
                catch (Exception ex)
                {
                    _core.TrySetException(ex);
                    return;
                }

                if (awaiter.IsCompleted) TryRightInvokeContinuation(this, awaiter);
                else
                    awaiter.SourceOnCompleted(
                        state =>
                        {
                            using var t = (StateTuple<WhenAnyLrPromise<T>, Awaiter>)state;
                            TryRightInvokeContinuation(t.Item1, t.Item2);
                        },
                        StateTuple.Create(this, awaiter)
                    );
            }
        }

        public (bool, T) GetResult(short token)
        {
            TaskTracker.RemoveTracking(this);
            GC.SuppressFinalize(this);
            return _core.GetResult(token);
        }

        public GDTaskStatus GetStatus(short token) => _core.GetStatus(token);

        public void OnCompleted(Action<object> continuation, object state, short token) => _core.OnCompleted(continuation, state, token);

        public GDTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

        void IGDTaskSource.GetResult(short token) => GetResult(token);

        private static void TryLeftInvokeContinuation(WhenAnyLrPromise<T> self, in GDTask<T>.Awaiter awaiter)
        {
            T result;

            try { result = awaiter.GetResult(); }
            catch (Exception ex)
            {
                self._core.TrySetException(ex);
                return;
            }

            if (Interlocked.Increment(ref self._completedCount) == 1) self._core.TrySetResult((true, result));
        }

        private static void TryRightInvokeContinuation(WhenAnyLrPromise<T> self, in Awaiter awaiter)
        {
            try { awaiter.GetResult(); }
            catch (Exception ex)
            {
                self._core.TrySetException(ex);
                return;
            }

            if (Interlocked.Increment(ref self._completedCount) == 1) self._core.TrySetResult((false, default));
        }
    }


    private sealed class WhenAnyPromise<T> : IGDTaskSource<(int, T)>
    {
        private int _completedCount;
        private GDTaskCompletionSourceCore<(int, T)> _core;

        public WhenAnyPromise(ReadOnlySpan<GDTask<T>> tasks)
        {
            if (tasks.Length == 0) throw new ArgumentException("The tasks argument contains no tasks.");

            TaskTracker.TrackActiveTask(this, 3);

            for (var i = 0; i < tasks.Length; i++)
            {
                GDTask<T>.Awaiter awaiter;

                try { awaiter = tasks[i].GetAwaiter(); }
                catch (Exception ex)
                {
                    _core.TrySetException(ex);
                    continue; // consume others.
                }

                if (awaiter.IsCompleted) TryInvokeContinuation(this, awaiter, i);
                else
                    awaiter.SourceOnCompleted(
                        state =>
                        {
                            using var t = (StateTuple<WhenAnyPromise<T>, GDTask<T>.Awaiter, int>)state;
                            TryInvokeContinuation(t.Item1, t.Item2, t.Item3);
                        },
                        StateTuple.Create(this, awaiter, i)
                    );
            }
        }

        public (int, T) GetResult(short token)
        {
            TaskTracker.RemoveTracking(this);
            GC.SuppressFinalize(this);
            return _core.GetResult(token);
        }

        public GDTaskStatus GetStatus(short token) => _core.GetStatus(token);

        public void OnCompleted(Action<object> continuation, object state, short token) => _core.OnCompleted(continuation, state, token);

        public GDTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

        void IGDTaskSource.GetResult(short token) => GetResult(token);

        private static void TryInvokeContinuation(WhenAnyPromise<T> self, in GDTask<T>.Awaiter awaiter, int i)
        {
            T result;

            try { result = awaiter.GetResult(); }
            catch (Exception ex)
            {
                self._core.TrySetException(ex);
                return;
            }

            if (Interlocked.Increment(ref self._completedCount) == 1) self._core.TrySetResult((i, result));
        }
    }

    private sealed class WhenAnyPromise : IGDTaskSource<int>
    {
        private int _completedCount;
        private GDTaskCompletionSourceCore<int> _core;

        public WhenAnyPromise(ReadOnlySpan<GDTask> tasks)
        {
            if (tasks.Length == 0) throw new ArgumentException("The tasks argument contains no tasks.");

            TaskTracker.TrackActiveTask(this, 3);

            for (var i = 0; i < tasks.Length; i++)
            {
                Awaiter awaiter;

                try { awaiter = tasks[i].GetAwaiter(); }
                catch (Exception ex)
                {
                    _core.TrySetException(ex);
                    continue; // consume others.
                }

                if (awaiter.IsCompleted) TryInvokeContinuation(this, awaiter, i);
                else
                    awaiter.SourceOnCompleted(
                        state =>
                        {
                            using var t = (StateTuple<WhenAnyPromise, Awaiter, int>)state;
                            TryInvokeContinuation(t.Item1, t.Item2, t.Item3);
                        },
                        StateTuple.Create(this, awaiter, i)
                    );
            }
        }

        public int GetResult(short token)
        {
            TaskTracker.RemoveTracking(this);
            GC.SuppressFinalize(this);
            return _core.GetResult(token);
        }

        public GDTaskStatus GetStatus(short token) => _core.GetStatus(token);

        public void OnCompleted(Action<object> continuation, object state, short token) => _core.OnCompleted(continuation, state, token);

        public GDTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

        void IGDTaskSource.GetResult(short token) => GetResult(token);

        private static void TryInvokeContinuation(WhenAnyPromise self, in Awaiter awaiter, int i)
        {
            try { awaiter.GetResult(); }
            catch (Exception ex)
            {
                self._core.TrySetException(ex);
                return;
            }

            if (Interlocked.Increment(ref self._completedCount) == 1) self._core.TrySetResult(i);
        }
    }
}