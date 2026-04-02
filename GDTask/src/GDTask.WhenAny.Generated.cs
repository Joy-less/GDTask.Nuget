using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using GodotTask.Internal;

namespace GodotTask
{
    public partial struct GDTask
    {
        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// The returned combinator task completes successfully with the first completed input representation,
        /// even if that winning input is faulted or canceled.
        /// </summary>
        /// <returns>A task that represents the info for completed tasks.</returns>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2)> WhenAny<T1, T2>(GDTask<T1> task1, GDTask<T2> task2)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2)
            {
                var (winArgumentIndex, result1, result2) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>>(task1, task2), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2));
            }
        }

        sealed class WhenAnyPromise<T1, T2> : IGDTaskSource<(int, T1 result1, T2 result2)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result));
                }
            }


            public (int, T1 result1, T2 result2) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3)> WhenAny<T1, T2, T3>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3)
            {
                var (winArgumentIndex, result1, result2, result3) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>>(task1, task2, task3), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4)> WhenAny<T1, T2, T3, T4>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4)
            {
                var (winArgumentIndex, result1, result2, result3, result4) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>>(task1, task2, task3, task4), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5)> WhenAny<T1, T2, T3, T4, T5>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>>(task1, task2, task3, task4, task5), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6)> WhenAny<T1, T2, T3, T4, T5, T6>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>>(task1, task2, task3, task4, task5, task6), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7)> WhenAny<T1, T2, T3, T4, T5, T6, T7>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>>(task1, task2, task3, task4, task5, task6, task7), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8)> WhenAny<T1, T2, T3, T4, T5, T6, T7, T8>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7), ObserveWhenAnyCompletion(task8));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7, GDTask<WhenAnyObserved<T8>> task8)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7, result8) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7, WhenAnyObserved<T8> result8)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>, WhenAnyObserved<T8>>(task1, task2, task3, task4, task5, task6, task7, task8), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7), GetWhenAnyObservedResult(result8));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task8.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT8(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT8(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T8>.Awaiter awaiter)
            {
                T8 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((7, default, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9)> WhenAny<T1, T2, T3, T4, T5, T6, T7, T8, T9>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7), ObserveWhenAnyCompletion(task8), ObserveWhenAnyCompletion(task9));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7, GDTask<WhenAnyObserved<T8>> task8, GDTask<WhenAnyObserved<T9>> task9)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7, result8, result9) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7, WhenAnyObserved<T8> result8, WhenAnyObserved<T9> result9)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>, WhenAnyObserved<T8>, WhenAnyObserved<T9>>(task1, task2, task3, task4, task5, task6, task7, task8, task9), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7), GetWhenAnyObservedResult(result8), GetWhenAnyObservedResult(result9));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task8.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT8(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task9.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT9(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T9>.Awaiter>)state)
                            {
                                TryInvokeContinuationT9(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT8(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T8>.Awaiter awaiter)
            {
                T8 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((7, default, default, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT9(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T9>.Awaiter awaiter)
            {
                T9 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((8, default, default, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10)> WhenAny<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7), ObserveWhenAnyCompletion(task8), ObserveWhenAnyCompletion(task9), ObserveWhenAnyCompletion(task10));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7, GDTask<WhenAnyObserved<T8>> task8, GDTask<WhenAnyObserved<T9>> task9, GDTask<WhenAnyObserved<T10>> task10)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7, result8, result9, result10) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7, WhenAnyObserved<T8> result8, WhenAnyObserved<T9> result9, WhenAnyObserved<T10> result10)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>, WhenAnyObserved<T8>, WhenAnyObserved<T9>, WhenAnyObserved<T10>>(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7), GetWhenAnyObservedResult(result8), GetWhenAnyObservedResult(result9), GetWhenAnyObservedResult(result10));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task8.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT8(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task9.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT9(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T9>.Awaiter>)state)
                            {
                                TryInvokeContinuationT9(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task10.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT10(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T10>.Awaiter>)state)
                            {
                                TryInvokeContinuationT10(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT8(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T8>.Awaiter awaiter)
            {
                T8 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((7, default, default, default, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT9(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T9>.Awaiter awaiter)
            {
                T9 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((8, default, default, default, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT10(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T10>.Awaiter awaiter)
            {
                T10 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((9, default, default, default, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11)> WhenAny<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7), ObserveWhenAnyCompletion(task8), ObserveWhenAnyCompletion(task9), ObserveWhenAnyCompletion(task10), ObserveWhenAnyCompletion(task11));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7, GDTask<WhenAnyObserved<T8>> task8, GDTask<WhenAnyObserved<T9>> task9, GDTask<WhenAnyObserved<T10>> task10, GDTask<WhenAnyObserved<T11>> task11)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7, result8, result9, result10, result11) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7, WhenAnyObserved<T8> result8, WhenAnyObserved<T9> result9, WhenAnyObserved<T10> result10, WhenAnyObserved<T11> result11)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>, WhenAnyObserved<T8>, WhenAnyObserved<T9>, WhenAnyObserved<T10>, WhenAnyObserved<T11>>(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7), GetWhenAnyObservedResult(result8), GetWhenAnyObservedResult(result9), GetWhenAnyObservedResult(result10), GetWhenAnyObservedResult(result11));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task8.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT8(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task9.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT9(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T9>.Awaiter>)state)
                            {
                                TryInvokeContinuationT9(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task10.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT10(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T10>.Awaiter>)state)
                            {
                                TryInvokeContinuationT10(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task11.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT11(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T11>.Awaiter>)state)
                            {
                                TryInvokeContinuationT11(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT8(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T8>.Awaiter awaiter)
            {
                T8 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((7, default, default, default, default, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT9(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T9>.Awaiter awaiter)
            {
                T9 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((8, default, default, default, default, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT10(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T10>.Awaiter awaiter)
            {
                T10 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((9, default, default, default, default, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT11(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T11>.Awaiter awaiter)
            {
                T11 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((10, default, default, default, default, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12)> WhenAny<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7), ObserveWhenAnyCompletion(task8), ObserveWhenAnyCompletion(task9), ObserveWhenAnyCompletion(task10), ObserveWhenAnyCompletion(task11), ObserveWhenAnyCompletion(task12));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7, GDTask<WhenAnyObserved<T8>> task8, GDTask<WhenAnyObserved<T9>> task9, GDTask<WhenAnyObserved<T10>> task10, GDTask<WhenAnyObserved<T11>> task11, GDTask<WhenAnyObserved<T12>> task12)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7, result8, result9, result10, result11, result12) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7, WhenAnyObserved<T8> result8, WhenAnyObserved<T9> result9, WhenAnyObserved<T10> result10, WhenAnyObserved<T11> result11, WhenAnyObserved<T12> result12)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>, WhenAnyObserved<T8>, WhenAnyObserved<T9>, WhenAnyObserved<T10>, WhenAnyObserved<T11>, WhenAnyObserved<T12>>(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7), GetWhenAnyObservedResult(result8), GetWhenAnyObservedResult(result9), GetWhenAnyObservedResult(result10), GetWhenAnyObservedResult(result11), GetWhenAnyObservedResult(result12));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task8.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT8(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task9.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT9(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T9>.Awaiter>)state)
                            {
                                TryInvokeContinuationT9(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task10.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT10(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T10>.Awaiter>)state)
                            {
                                TryInvokeContinuationT10(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task11.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT11(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T11>.Awaiter>)state)
                            {
                                TryInvokeContinuationT11(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task12.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT12(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T12>.Awaiter>)state)
                            {
                                TryInvokeContinuationT12(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT8(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T8>.Awaiter awaiter)
            {
                T8 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((7, default, default, default, default, default, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT9(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T9>.Awaiter awaiter)
            {
                T9 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((8, default, default, default, default, default, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT10(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T10>.Awaiter awaiter)
            {
                T10 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((9, default, default, default, default, default, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT11(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T11>.Awaiter awaiter)
            {
                T11 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((10, default, default, default, default, default, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT12(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T12>.Awaiter awaiter)
            {
                T12 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((11, default, default, default, default, default, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13)> WhenAny<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7), ObserveWhenAnyCompletion(task8), ObserveWhenAnyCompletion(task9), ObserveWhenAnyCompletion(task10), ObserveWhenAnyCompletion(task11), ObserveWhenAnyCompletion(task12), ObserveWhenAnyCompletion(task13));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7, GDTask<WhenAnyObserved<T8>> task8, GDTask<WhenAnyObserved<T9>> task9, GDTask<WhenAnyObserved<T10>> task10, GDTask<WhenAnyObserved<T11>> task11, GDTask<WhenAnyObserved<T12>> task12, GDTask<WhenAnyObserved<T13>> task13)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7, result8, result9, result10, result11, result12, result13) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7, WhenAnyObserved<T8> result8, WhenAnyObserved<T9> result9, WhenAnyObserved<T10> result10, WhenAnyObserved<T11> result11, WhenAnyObserved<T12> result12, WhenAnyObserved<T13> result13)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>, WhenAnyObserved<T8>, WhenAnyObserved<T9>, WhenAnyObserved<T10>, WhenAnyObserved<T11>, WhenAnyObserved<T12>, WhenAnyObserved<T13>>(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7), GetWhenAnyObservedResult(result8), GetWhenAnyObservedResult(result9), GetWhenAnyObservedResult(result10), GetWhenAnyObservedResult(result11), GetWhenAnyObservedResult(result12), GetWhenAnyObservedResult(result13));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task8.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT8(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task9.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT9(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T9>.Awaiter>)state)
                            {
                                TryInvokeContinuationT9(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task10.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT10(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T10>.Awaiter>)state)
                            {
                                TryInvokeContinuationT10(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task11.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT11(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T11>.Awaiter>)state)
                            {
                                TryInvokeContinuationT11(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task12.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT12(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T12>.Awaiter>)state)
                            {
                                TryInvokeContinuationT12(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task13.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT13(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T13>.Awaiter>)state)
                            {
                                TryInvokeContinuationT13(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT8(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T8>.Awaiter awaiter)
            {
                T8 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((7, default, default, default, default, default, default, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT9(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T9>.Awaiter awaiter)
            {
                T9 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((8, default, default, default, default, default, default, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT10(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T10>.Awaiter awaiter)
            {
                T10 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((9, default, default, default, default, default, default, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT11(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T11>.Awaiter awaiter)
            {
                T11 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((10, default, default, default, default, default, default, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT12(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T12>.Awaiter awaiter)
            {
                T12 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((11, default, default, default, default, default, default, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT13(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T13>.Awaiter awaiter)
            {
                T13 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((12, default, default, default, default, default, default, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14)> WhenAny<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13, GDTask<T14> task14)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7), ObserveWhenAnyCompletion(task8), ObserveWhenAnyCompletion(task9), ObserveWhenAnyCompletion(task10), ObserveWhenAnyCompletion(task11), ObserveWhenAnyCompletion(task12), ObserveWhenAnyCompletion(task13), ObserveWhenAnyCompletion(task14));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7, GDTask<WhenAnyObserved<T8>> task8, GDTask<WhenAnyObserved<T9>> task9, GDTask<WhenAnyObserved<T10>> task10, GDTask<WhenAnyObserved<T11>> task11, GDTask<WhenAnyObserved<T12>> task12, GDTask<WhenAnyObserved<T13>> task13, GDTask<WhenAnyObserved<T14>> task14)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7, result8, result9, result10, result11, result12, result13, result14) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7, WhenAnyObserved<T8> result8, WhenAnyObserved<T9> result9, WhenAnyObserved<T10> result10, WhenAnyObserved<T11> result11, WhenAnyObserved<T12> result12, WhenAnyObserved<T13> result13, WhenAnyObserved<T14> result14)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>, WhenAnyObserved<T8>, WhenAnyObserved<T9>, WhenAnyObserved<T10>, WhenAnyObserved<T11>, WhenAnyObserved<T12>, WhenAnyObserved<T13>, WhenAnyObserved<T14>>(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7), GetWhenAnyObservedResult(result8), GetWhenAnyObservedResult(result9), GetWhenAnyObservedResult(result10), GetWhenAnyObservedResult(result11), GetWhenAnyObservedResult(result12), GetWhenAnyObservedResult(result13), GetWhenAnyObservedResult(result14));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13, GDTask<T14> task14)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task8.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT8(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task9.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT9(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T9>.Awaiter>)state)
                            {
                                TryInvokeContinuationT9(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task10.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT10(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T10>.Awaiter>)state)
                            {
                                TryInvokeContinuationT10(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task11.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT11(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T11>.Awaiter>)state)
                            {
                                TryInvokeContinuationT11(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task12.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT12(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T12>.Awaiter>)state)
                            {
                                TryInvokeContinuationT12(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task13.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT13(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T13>.Awaiter>)state)
                            {
                                TryInvokeContinuationT13(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task14.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT14(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T14>.Awaiter>)state)
                            {
                                TryInvokeContinuationT14(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT8(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T8>.Awaiter awaiter)
            {
                T8 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((7, default, default, default, default, default, default, default, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT9(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T9>.Awaiter awaiter)
            {
                T9 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((8, default, default, default, default, default, default, default, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT10(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T10>.Awaiter awaiter)
            {
                T10 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((9, default, default, default, default, default, default, default, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT11(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T11>.Awaiter awaiter)
            {
                T11 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((10, default, default, default, default, default, default, default, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT12(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T12>.Awaiter awaiter)
            {
                T12 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((11, default, default, default, default, default, default, default, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT13(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T13>.Awaiter awaiter)
            {
                T13 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((12, default, default, default, default, default, default, default, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT14(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T14>.Awaiter awaiter)
            {
                T14 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((13, default, default, default, default, default, default, default, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

        /// <inheritdoc cref="WhenAny{T1,T2}"/>
        public static GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14, T15 result15)> WhenAny<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13, GDTask<T14> task14, GDTask<T15> task15)
        {
            return Core(ObserveWhenAnyCompletion(task1), ObserveWhenAnyCompletion(task2), ObserveWhenAnyCompletion(task3), ObserveWhenAnyCompletion(task4), ObserveWhenAnyCompletion(task5), ObserveWhenAnyCompletion(task6), ObserveWhenAnyCompletion(task7), ObserveWhenAnyCompletion(task8), ObserveWhenAnyCompletion(task9), ObserveWhenAnyCompletion(task10), ObserveWhenAnyCompletion(task11), ObserveWhenAnyCompletion(task12), ObserveWhenAnyCompletion(task13), ObserveWhenAnyCompletion(task14), ObserveWhenAnyCompletion(task15));

            static async GDTask<(int winArgumentIndex, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14, T15 result15)> Core(GDTask<WhenAnyObserved<T1>> task1, GDTask<WhenAnyObserved<T2>> task2, GDTask<WhenAnyObserved<T3>> task3, GDTask<WhenAnyObserved<T4>> task4, GDTask<WhenAnyObserved<T5>> task5, GDTask<WhenAnyObserved<T6>> task6, GDTask<WhenAnyObserved<T7>> task7, GDTask<WhenAnyObserved<T8>> task8, GDTask<WhenAnyObserved<T9>> task9, GDTask<WhenAnyObserved<T10>> task10, GDTask<WhenAnyObserved<T11>> task11, GDTask<WhenAnyObserved<T12>> task12, GDTask<WhenAnyObserved<T13>> task13, GDTask<WhenAnyObserved<T14>> task14, GDTask<WhenAnyObserved<T15>> task15)
            {
                var (winArgumentIndex, result1, result2, result3, result4, result5, result6, result7, result8, result9, result10, result11, result12, result13, result14, result15) =
                    await new GDTask<(int winArgumentIndex, WhenAnyObserved<T1> result1, WhenAnyObserved<T2> result2, WhenAnyObserved<T3> result3, WhenAnyObserved<T4> result4, WhenAnyObserved<T5> result5, WhenAnyObserved<T6> result6, WhenAnyObserved<T7> result7, WhenAnyObserved<T8> result8, WhenAnyObserved<T9> result9, WhenAnyObserved<T10> result10, WhenAnyObserved<T11> result11, WhenAnyObserved<T12> result12, WhenAnyObserved<T13> result13, WhenAnyObserved<T14> result14, WhenAnyObserved<T15> result15)>(new WhenAnyPromise<WhenAnyObserved<T1>, WhenAnyObserved<T2>, WhenAnyObserved<T3>, WhenAnyObserved<T4>, WhenAnyObserved<T5>, WhenAnyObserved<T6>, WhenAnyObserved<T7>, WhenAnyObserved<T8>, WhenAnyObserved<T9>, WhenAnyObserved<T10>, WhenAnyObserved<T11>, WhenAnyObserved<T12>, WhenAnyObserved<T13>, WhenAnyObserved<T14>, WhenAnyObserved<T15>>(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14, task15), 0);
                return (winArgumentIndex, GetWhenAnyObservedResult(result1), GetWhenAnyObservedResult(result2), GetWhenAnyObservedResult(result3), GetWhenAnyObservedResult(result4), GetWhenAnyObservedResult(result5), GetWhenAnyObservedResult(result6), GetWhenAnyObservedResult(result7), GetWhenAnyObservedResult(result8), GetWhenAnyObservedResult(result9), GetWhenAnyObservedResult(result10), GetWhenAnyObservedResult(result11), GetWhenAnyObservedResult(result12), GetWhenAnyObservedResult(result13), GetWhenAnyObservedResult(result14), GetWhenAnyObservedResult(result15));
            }
        }

        sealed class WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IGDTaskSource<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14, T15 result15)>
        {
            int completedCount;
            GDTaskCompletionSourceCore<(int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14, T15 result15)> core;

            public WhenAnyPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13, GDTask<T14> task14, GDTask<T15> task15)
            {
                TaskTracker.TrackActiveTask(this, 3);

                this.completedCount = 0;
                {
                    var awaiter = task1.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT1(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T1>.Awaiter>)state)
                            {
                                TryInvokeContinuationT1(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task2.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT2(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task3.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT3(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task4.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT4(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task5.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT5(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task6.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT6(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task7.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT7(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task8.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT8(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task9.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT9(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T9>.Awaiter>)state)
                            {
                                TryInvokeContinuationT9(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task10.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT10(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T10>.Awaiter>)state)
                            {
                                TryInvokeContinuationT10(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task11.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT11(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T11>.Awaiter>)state)
                            {
                                TryInvokeContinuationT11(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task12.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT12(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T12>.Awaiter>)state)
                            {
                                TryInvokeContinuationT12(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task13.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT13(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T13>.Awaiter>)state)
                            {
                                TryInvokeContinuationT13(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task14.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT14(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T14>.Awaiter>)state)
                            {
                                TryInvokeContinuationT14(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
                {
                    var awaiter = task15.GetAwaiter();

                    if (awaiter.IsCompleted)
                    {
                        TryInvokeContinuationT15(this, awaiter);
                    }
                    else
                    {
                        awaiter.SourceOnCompleted(state =>
                        {
                            using (var t = (StateTuple<WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T15>.Awaiter>)state)
                            {
                                TryInvokeContinuationT15(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T1>.Awaiter awaiter)
            {
                T1 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((0, result, default, default, default, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT2(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T2>.Awaiter awaiter)
            {
                T2 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((1, default, result, default, default, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT3(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T3>.Awaiter awaiter)
            {
                T3 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((2, default, default, result, default, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT4(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T4>.Awaiter awaiter)
            {
                T4 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((3, default, default, default, result, default, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT5(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T5>.Awaiter awaiter)
            {
                T5 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((4, default, default, default, default, result, default, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT6(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T6>.Awaiter awaiter)
            {
                T6 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((5, default, default, default, default, default, result, default, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT7(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T7>.Awaiter awaiter)
            {
                T7 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((6, default, default, default, default, default, default, result, default, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT8(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T8>.Awaiter awaiter)
            {
                T8 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((7, default, default, default, default, default, default, default, result, default, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT9(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T9>.Awaiter awaiter)
            {
                T9 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((8, default, default, default, default, default, default, default, default, result, default, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT10(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T10>.Awaiter awaiter)
            {
                T10 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((9, default, default, default, default, default, default, default, default, default, result, default, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT11(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T11>.Awaiter awaiter)
            {
                T11 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((10, default, default, default, default, default, default, default, default, default, default, result, default, default, default, default));
                }
            }

            static void TryInvokeContinuationT12(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T12>.Awaiter awaiter)
            {
                T12 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((11, default, default, default, default, default, default, default, default, default, default, default, result, default, default, default));
                }
            }

            static void TryInvokeContinuationT13(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T13>.Awaiter awaiter)
            {
                T13 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((12, default, default, default, default, default, default, default, default, default, default, default, default, result, default, default));
                }
            }

            static void TryInvokeContinuationT14(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T14>.Awaiter awaiter)
            {
                T14 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((13, default, default, default, default, default, default, default, default, default, default, default, default, default, result, default));
                }
            }

            static void TryInvokeContinuationT15(WhenAnyPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T15>.Awaiter awaiter)
            {
                T15 result;
                try
                {
                    result = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 1)
                {
                    self.core.TrySetResult((14, default, default, default, default, default, default, default, default, default, default, default, default, default, default, result));
                }
            }


            public (int, T1 result1, T2 result2, T3 result3, T4 result4, T5 result5, T6 result6, T7 result7, T8 result8, T9 result9, T10 result10, T11 result11, T12 result12, T13 result13, T14 result14, T15 result15) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }
        }

    }
}