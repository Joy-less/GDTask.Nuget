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
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        public static async GDTask<(T1, T2)> WhenAll<T1, T2>(GDTask<T1> task1, GDTask<T2> task2)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2 });
            return (observation1.Result, observation2.Result);
        }

        sealed class WhenAllPromise<T1, T2> : IGDTaskSource<(T1, T2)>
        {
            T1 t1 = default;
            T2 t2 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2>, GDTask<T2>.Awaiter>)state)
                            {
                                TryInvokeContinuationT2(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 2)
                {
                    self.core.TrySetResult((self.t1, self.t2));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 2)
                {
                    self.core.TrySetResult((self.t1, self.t2));
                }
            }


            public (T1, T2) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3)> WhenAll<T1, T2, T3>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3 });
            return (observation1.Result, observation2.Result, observation3.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3> : IGDTaskSource<(T1, T2, T3)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3>, GDTask<T3>.Awaiter>)state)
                            {
                                TryInvokeContinuationT3(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 3)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 3)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 3)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3));
                }
            }


            public (T1, T2, T3) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4> : IGDTaskSource<(T1, T2, T3, T4)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4>, GDTask<T4>.Awaiter>)state)
                            {
                                TryInvokeContinuationT4(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 4)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 4)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 4)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 4)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4));
                }
            }


            public (T1, T2, T3, T4) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5> : IGDTaskSource<(T1, T2, T3, T4, T5)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5>, GDTask<T5>.Awaiter>)state)
                            {
                                TryInvokeContinuationT5(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 5)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 5)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 5)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 5)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 5)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5));
                }
            }


            public (T1, T2, T3, T4, T5) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6> : IGDTaskSource<(T1, T2, T3, T4, T5, T6)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6>, GDTask<T6>.Awaiter>)state)
                            {
                                TryInvokeContinuationT6(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 6)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 6)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 6)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 6)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 6)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 6)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6));
                }
            }


            public (T1, T2, T3, T4, T5, T6) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7>, GDTask<T7>.Awaiter>)state)
                            {
                                TryInvokeContinuationT7(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 7)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 7)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 7)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 7)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 7)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 7)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 7)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully() && task8.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult(), task8.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            var observation8 = new WhenAllObservation<T8>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7), ObserveWhenAll(task8, observation8) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7, observation8 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result, observation8.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7, T8)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7, T8)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T7>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8>, GDTask<T8>.Awaiter>)state)
                            {
                                TryInvokeContinuationT8(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 8)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 8)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 8)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 8)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 8)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 8)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 8)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8));
                }
            }

            static void TryInvokeContinuationT8(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8> self, in GDTask<T8>.Awaiter awaiter)
            {
                try
                {
                    self.t8 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 8)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7, T8) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully() && task8.Status.IsCompletedSuccessfully() && task9.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult(), task8.GetAwaiter().GetResult(), task9.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            var observation8 = new WhenAllObservation<T8>();
            var observation9 = new WhenAllObservation<T9>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7), ObserveWhenAll(task8, observation8), ObserveWhenAll(task9, observation9) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7, observation8, observation9 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result, observation8.Result, observation9.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T7>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T8>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9>, GDTask<T9>.Awaiter>)state)
                            {
                                TryInvokeContinuationT9(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }

            static void TryInvokeContinuationT8(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T8>.Awaiter awaiter)
            {
                try
                {
                    self.t8 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }

            static void TryInvokeContinuationT9(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, in GDTask<T9>.Awaiter awaiter)
            {
                try
                {
                    self.t9 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 9)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7, T8, T9) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully() && task8.Status.IsCompletedSuccessfully() && task9.Status.IsCompletedSuccessfully() && task10.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult(), task8.GetAwaiter().GetResult(), task9.GetAwaiter().GetResult(), task10.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            var observation8 = new WhenAllObservation<T8>();
            var observation9 = new WhenAllObservation<T9>();
            var observation10 = new WhenAllObservation<T10>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7), ObserveWhenAll(task8, observation8), ObserveWhenAll(task9, observation9), ObserveWhenAll(task10, observation10) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7, observation8, observation9, observation10 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result, observation8.Result, observation9.Result, observation10.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T7>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T8>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T9>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, GDTask<T10>.Awaiter>)state)
                            {
                                TryInvokeContinuationT10(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT8(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T8>.Awaiter awaiter)
            {
                try
                {
                    self.t8 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT9(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T9>.Awaiter awaiter)
            {
                try
                {
                    self.t9 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }

            static void TryInvokeContinuationT10(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, in GDTask<T10>.Awaiter awaiter)
            {
                try
                {
                    self.t10 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 10)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully() && task8.Status.IsCompletedSuccessfully() && task9.Status.IsCompletedSuccessfully() && task10.Status.IsCompletedSuccessfully() && task11.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult(), task8.GetAwaiter().GetResult(), task9.GetAwaiter().GetResult(), task10.GetAwaiter().GetResult(), task11.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            var observation8 = new WhenAllObservation<T8>();
            var observation9 = new WhenAllObservation<T9>();
            var observation10 = new WhenAllObservation<T10>();
            var observation11 = new WhenAllObservation<T11>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7), ObserveWhenAll(task8, observation8), ObserveWhenAll(task9, observation9), ObserveWhenAll(task10, observation10), ObserveWhenAll(task11, observation11) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7, observation8, observation9, observation10, observation11 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result, observation8.Result, observation9.Result, observation10.Result, observation11.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T7>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T8>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T9>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T10>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, GDTask<T11>.Awaiter>)state)
                            {
                                TryInvokeContinuationT11(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT8(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T8>.Awaiter awaiter)
            {
                try
                {
                    self.t8 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT9(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T9>.Awaiter awaiter)
            {
                try
                {
                    self.t9 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT10(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T10>.Awaiter awaiter)
            {
                try
                {
                    self.t10 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }

            static void TryInvokeContinuationT11(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self, in GDTask<T11>.Awaiter awaiter)
            {
                try
                {
                    self.t11 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 11)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully() && task8.Status.IsCompletedSuccessfully() && task9.Status.IsCompletedSuccessfully() && task10.Status.IsCompletedSuccessfully() && task11.Status.IsCompletedSuccessfully() && task12.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult(), task8.GetAwaiter().GetResult(), task9.GetAwaiter().GetResult(), task10.GetAwaiter().GetResult(), task11.GetAwaiter().GetResult(), task12.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            var observation8 = new WhenAllObservation<T8>();
            var observation9 = new WhenAllObservation<T9>();
            var observation10 = new WhenAllObservation<T10>();
            var observation11 = new WhenAllObservation<T11>();
            var observation12 = new WhenAllObservation<T12>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7), ObserveWhenAll(task8, observation8), ObserveWhenAll(task9, observation9), ObserveWhenAll(task10, observation10), ObserveWhenAll(task11, observation11), ObserveWhenAll(task12, observation12) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7, observation8, observation9, observation10, observation11, observation12 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result, observation8.Result, observation9.Result, observation10.Result, observation11.Result, observation12.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            T12 t12 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T7>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T8>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T9>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T10>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T11>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, GDTask<T12>.Awaiter>)state)
                            {
                                TryInvokeContinuationT12(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT8(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T8>.Awaiter awaiter)
            {
                try
                {
                    self.t8 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT9(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T9>.Awaiter awaiter)
            {
                try
                {
                    self.t9 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT10(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T10>.Awaiter awaiter)
            {
                try
                {
                    self.t10 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT11(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T11>.Awaiter awaiter)
            {
                try
                {
                    self.t11 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }

            static void TryInvokeContinuationT12(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, in GDTask<T12>.Awaiter awaiter)
            {
                try
                {
                    self.t12 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 12)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully() && task8.Status.IsCompletedSuccessfully() && task9.Status.IsCompletedSuccessfully() && task10.Status.IsCompletedSuccessfully() && task11.Status.IsCompletedSuccessfully() && task12.Status.IsCompletedSuccessfully() && task13.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult(), task8.GetAwaiter().GetResult(), task9.GetAwaiter().GetResult(), task10.GetAwaiter().GetResult(), task11.GetAwaiter().GetResult(), task12.GetAwaiter().GetResult(), task13.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            var observation8 = new WhenAllObservation<T8>();
            var observation9 = new WhenAllObservation<T9>();
            var observation10 = new WhenAllObservation<T10>();
            var observation11 = new WhenAllObservation<T11>();
            var observation12 = new WhenAllObservation<T12>();
            var observation13 = new WhenAllObservation<T13>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7), ObserveWhenAll(task8, observation8), ObserveWhenAll(task9, observation9), ObserveWhenAll(task10, observation10), ObserveWhenAll(task11, observation11), ObserveWhenAll(task12, observation12), ObserveWhenAll(task13, observation13) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7, observation8, observation9, observation10, observation11, observation12, observation13 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result, observation8.Result, observation9.Result, observation10.Result, observation11.Result, observation12.Result, observation13.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            T12 t12 = default;
            T13 t13 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T7>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T8>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T9>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T10>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T11>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T12>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, GDTask<T13>.Awaiter>)state)
                            {
                                TryInvokeContinuationT13(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT8(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T8>.Awaiter awaiter)
            {
                try
                {
                    self.t8 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT9(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T9>.Awaiter awaiter)
            {
                try
                {
                    self.t9 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT10(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T10>.Awaiter awaiter)
            {
                try
                {
                    self.t10 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT11(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T11>.Awaiter awaiter)
            {
                try
                {
                    self.t11 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT12(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T12>.Awaiter awaiter)
            {
                try
                {
                    self.t12 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }

            static void TryInvokeContinuationT13(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, in GDTask<T13>.Awaiter awaiter)
            {
                try
                {
                    self.t13 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 13)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13, GDTask<T14> task14)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully() && task8.Status.IsCompletedSuccessfully() && task9.Status.IsCompletedSuccessfully() && task10.Status.IsCompletedSuccessfully() && task11.Status.IsCompletedSuccessfully() && task12.Status.IsCompletedSuccessfully() && task13.Status.IsCompletedSuccessfully() && task14.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult(), task8.GetAwaiter().GetResult(), task9.GetAwaiter().GetResult(), task10.GetAwaiter().GetResult(), task11.GetAwaiter().GetResult(), task12.GetAwaiter().GetResult(), task13.GetAwaiter().GetResult(), task14.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            var observation8 = new WhenAllObservation<T8>();
            var observation9 = new WhenAllObservation<T9>();
            var observation10 = new WhenAllObservation<T10>();
            var observation11 = new WhenAllObservation<T11>();
            var observation12 = new WhenAllObservation<T12>();
            var observation13 = new WhenAllObservation<T13>();
            var observation14 = new WhenAllObservation<T14>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7), ObserveWhenAll(task8, observation8), ObserveWhenAll(task9, observation9), ObserveWhenAll(task10, observation10), ObserveWhenAll(task11, observation11), ObserveWhenAll(task12, observation12), ObserveWhenAll(task13, observation13), ObserveWhenAll(task14, observation14) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7, observation8, observation9, observation10, observation11, observation12, observation13, observation14 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result, observation8.Result, observation9.Result, observation10.Result, observation11.Result, observation12.Result, observation13.Result, observation14.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            T12 t12 = default;
            T13 t13 = default;
            T14 t14 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13, GDTask<T14> task14)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T7>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T8>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T9>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T10>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T11>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T12>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T13>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, GDTask<T14>.Awaiter>)state)
                            {
                                TryInvokeContinuationT14(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT8(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T8>.Awaiter awaiter)
            {
                try
                {
                    self.t8 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT9(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T9>.Awaiter awaiter)
            {
                try
                {
                    self.t9 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT10(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T10>.Awaiter awaiter)
            {
                try
                {
                    self.t10 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT11(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T11>.Awaiter awaiter)
            {
                try
                {
                    self.t11 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT12(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T12>.Awaiter awaiter)
            {
                try
                {
                    self.t12 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT13(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T13>.Awaiter awaiter)
            {
                try
                {
                    self.t13 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }

            static void TryInvokeContinuationT14(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, in GDTask<T14>.Awaiter awaiter)
            {
                try
                {
                    self.t14 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 14)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }

        /// <inheritdoc cref="WhenAll{T1,T2}"/>
        public static async GDTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13, GDTask<T14> task14, GDTask<T15> task15)
        {
            if (task1.Status.IsCompletedSuccessfully() && task2.Status.IsCompletedSuccessfully() && task3.Status.IsCompletedSuccessfully() && task4.Status.IsCompletedSuccessfully() && task5.Status.IsCompletedSuccessfully() && task6.Status.IsCompletedSuccessfully() && task7.Status.IsCompletedSuccessfully() && task8.Status.IsCompletedSuccessfully() && task9.Status.IsCompletedSuccessfully() && task10.Status.IsCompletedSuccessfully() && task11.Status.IsCompletedSuccessfully() && task12.Status.IsCompletedSuccessfully() && task13.Status.IsCompletedSuccessfully() && task14.Status.IsCompletedSuccessfully() && task15.Status.IsCompletedSuccessfully())
            {
                return (task1.GetAwaiter().GetResult(), task2.GetAwaiter().GetResult(), task3.GetAwaiter().GetResult(), task4.GetAwaiter().GetResult(), task5.GetAwaiter().GetResult(), task6.GetAwaiter().GetResult(), task7.GetAwaiter().GetResult(), task8.GetAwaiter().GetResult(), task9.GetAwaiter().GetResult(), task10.GetAwaiter().GetResult(), task11.GetAwaiter().GetResult(), task12.GetAwaiter().GetResult(), task13.GetAwaiter().GetResult(), task14.GetAwaiter().GetResult(), task15.GetAwaiter().GetResult());
            }

            var observation1 = new WhenAllObservation<T1>();
            var observation2 = new WhenAllObservation<T2>();
            var observation3 = new WhenAllObservation<T3>();
            var observation4 = new WhenAllObservation<T4>();
            var observation5 = new WhenAllObservation<T5>();
            var observation6 = new WhenAllObservation<T6>();
            var observation7 = new WhenAllObservation<T7>();
            var observation8 = new WhenAllObservation<T8>();
            var observation9 = new WhenAllObservation<T9>();
            var observation10 = new WhenAllObservation<T10>();
            var observation11 = new WhenAllObservation<T11>();
            var observation12 = new WhenAllObservation<T12>();
            var observation13 = new WhenAllObservation<T13>();
            var observation14 = new WhenAllObservation<T14>();
            var observation15 = new WhenAllObservation<T15>();
            await WhenAll(new GDTask[] { ObserveWhenAll(task1, observation1), ObserveWhenAll(task2, observation2), ObserveWhenAll(task3, observation3), ObserveWhenAll(task4, observation4), ObserveWhenAll(task5, observation5), ObserveWhenAll(task6, observation6), ObserveWhenAll(task7, observation7), ObserveWhenAll(task8, observation8), ObserveWhenAll(task9, observation9), ObserveWhenAll(task10, observation10), ObserveWhenAll(task11, observation11), ObserveWhenAll(task12, observation12), ObserveWhenAll(task13, observation13), ObserveWhenAll(task14, observation14), ObserveWhenAll(task15, observation15) });
            CompleteObservedWhenAll(new IWhenAllObservation[] { observation1, observation2, observation3, observation4, observation5, observation6, observation7, observation8, observation9, observation10, observation11, observation12, observation13, observation14, observation15 });
            return (observation1.Result, observation2.Result, observation3.Result, observation4.Result, observation5.Result, observation6.Result, observation7.Result, observation8.Result, observation9.Result, observation10.Result, observation11.Result, observation12.Result, observation13.Result, observation14.Result, observation15.Result);
        }

        sealed class WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IGDTaskSource<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)>
        {
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            T12 t12 = default;
            T13 t13 = default;
            T14 t14 = default;
            T15 t15 = default;
            int completedCount;
            GDTaskCompletionSourceCore<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> core;

            public WhenAllPromise(GDTask<T1> task1, GDTask<T2> task2, GDTask<T3> task3, GDTask<T4> task4, GDTask<T5> task5, GDTask<T6> task6, GDTask<T7> task7, GDTask<T8> task8, GDTask<T9> task9, GDTask<T10> task10, GDTask<T11> task11, GDTask<T12> task12, GDTask<T13> task13, GDTask<T14> task14, GDTask<T15> task15)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T1>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T2>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T3>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T4>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T5>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T6>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T7>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T8>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T9>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T10>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T11>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T12>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T13>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T14>.Awaiter>)state)
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
                            using (var t = (StateTuple<WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, GDTask<T15>.Awaiter>)state)
                            {
                                TryInvokeContinuationT15(t.Item1, t.Item2);
                            }
                        }, StateTuple.Create(this, awaiter));
                    }
                }
            }

            static void TryInvokeContinuationT1(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T1>.Awaiter awaiter)
            {
                try
                {
                    self.t1 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT2(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T2>.Awaiter awaiter)
            {
                try
                {
                    self.t2 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT3(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T3>.Awaiter awaiter)
            {
                try
                {
                    self.t3 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT4(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T4>.Awaiter awaiter)
            {
                try
                {
                    self.t4 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT5(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T5>.Awaiter awaiter)
            {
                try
                {
                    self.t5 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT6(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T6>.Awaiter awaiter)
            {
                try
                {
                    self.t6 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT7(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T7>.Awaiter awaiter)
            {
                try
                {
                    self.t7 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT8(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T8>.Awaiter awaiter)
            {
                try
                {
                    self.t8 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT9(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T9>.Awaiter awaiter)
            {
                try
                {
                    self.t9 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT10(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T10>.Awaiter awaiter)
            {
                try
                {
                    self.t10 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT11(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T11>.Awaiter awaiter)
            {
                try
                {
                    self.t11 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT12(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T12>.Awaiter awaiter)
            {
                try
                {
                    self.t12 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT13(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T13>.Awaiter awaiter)
            {
                try
                {
                    self.t13 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT14(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T14>.Awaiter awaiter)
            {
                try
                {
                    self.t14 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }

            static void TryInvokeContinuationT15(WhenAllPromise<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, in GDTask<T15>.Awaiter awaiter)
            {
                try
                {
                    self.t15 = awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    self.core.TrySetException(ex);
                    return;
                }

                if (Interlocked.Increment(ref self.completedCount) == 15)
                {
                    self.core.TrySetResult((self.t1, self.t2, self.t3, self.t4, self.t5, self.t6, self.t7, self.t8, self.t9, self.t10, self.t11, self.t12, self.t13, self.t14, self.t15));
                }
            }


            public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15) GetResult(short token)
            {
                TaskTracker.RemoveTracking(this);
                GC.SuppressFinalize(this);
                return core.GetResult(token);
            }

            void IGDTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public GDTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public GDTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }
        }
    }
}