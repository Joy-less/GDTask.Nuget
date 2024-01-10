﻿using Fractural.Tasks.Internal;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using Godot;

namespace Fractural.Tasks
{
    /// <summary>
    /// Indicates the time provider used for Delaying
    /// </summary>
    public enum DelayType
    {
        /// <summary>Use scaled delta time provided from <see cref="Node._Process"/></summary>
        DeltaTime,
        /// <summary>Use time provided from <see cref="System.Diagnostics.Stopwatch.GetTimestamp()"/></summary>
        Realtime
    }

    public partial struct GDTask
    {
        /// <summary>
        /// Delay the execution until the next <see cref="PlayerLoopTiming.Process"/>.
        /// </summary>
        public static YieldAwaitable Yield()
        {
            // optimized for single continuation
            return new YieldAwaitable(PlayerLoopTiming.Process);
        }

        /// <summary>
        /// Delay the execution until the next provided <see cref="PlayerLoopTiming"/>.
        /// </summary>
        public static YieldAwaitable Yield(PlayerLoopTiming timing)
        {
            // optimized for single continuation
            return new YieldAwaitable(timing);
        }

        /// <summary>
        /// Delay the execution until the next <see cref="PlayerLoopTiming.Process"/>, with specified <see cref="CancellationToken"/>.
        /// </summary>
        public static GDTask Yield(CancellationToken cancellationToken)
        {
            return new GDTask(YieldPromise.Create(PlayerLoopTiming.Process, cancellationToken, out var token), token);
        }

        /// <summary>
        /// Delay the execution until the next provided <see cref="PlayerLoopTiming"/>, with specified <see cref="CancellationToken"/>.
        /// </summary>   
        public static GDTask Yield(PlayerLoopTiming timing, CancellationToken cancellationToken)
        {
            return new GDTask(YieldPromise.Create(timing, cancellationToken, out var token), token);
        }

        /// <summary>
        /// Delay the execution until the next frame of <see cref="PlayerLoopTiming.Process"/>.
        /// </summary>
        public static GDTask NextFrame()
        {
            return new GDTask(NextFramePromise.Create(PlayerLoopTiming.Process, CancellationToken.None, out var token), token);
        }

        /// <summary>
        /// Delay the execution until the next frame of the provided <see cref="PlayerLoopTiming"/>.
        /// </summary>
        public static GDTask NextFrame(PlayerLoopTiming timing)
        {
            return new GDTask(NextFramePromise.Create(timing, CancellationToken.None, out var token), token);
        }

        /// <summary>
        /// Delay the execution until the next frame of <see cref="PlayerLoopTiming.Process"/>, with specified <see cref="CancellationToken"/>.
        /// </summary>
        public static GDTask NextFrame(CancellationToken cancellationToken)
        {
            return new GDTask(NextFramePromise.Create(PlayerLoopTiming.Process, cancellationToken, out var token), token);
        }

        /// <summary>
        /// Delay the execution until the next frame of the provided <see cref="PlayerLoopTiming"/>, with specified <see cref="CancellationToken"/>.
        /// </summary>
        public static GDTask NextFrame(PlayerLoopTiming timing, CancellationToken cancellationToken)
        {
            return new GDTask(NextFramePromise.Create(timing, cancellationToken, out var token), token);
        }

        /// <inheritdoc cref="Yield()"/>
        public static YieldAwaitable WaitForEndOfFrame()
        {
            return GDTask.Yield(PlayerLoopTiming.Process);
        }

        /// <inheritdoc cref="Yield(CancellationToken)"/>
        public static GDTask WaitForEndOfFrame(CancellationToken cancellationToken)
        {
            return GDTask.Yield(PlayerLoopTiming.Process, cancellationToken);
        }

        /// <summary>
        /// Delay the execution until the next <see cref="PlayerLoopTiming.PhysicsProcess"/>.
        /// </summary>
        public static YieldAwaitable WaitForPhysicsProcess()
        {
            return GDTask.Yield(PlayerLoopTiming.PhysicsProcess);
        }

        /// <summary>
        /// Delay the execution until the next <see cref="PlayerLoopTiming.PhysicsProcess"/>, with specified <see cref="CancellationToken"/>.
        /// </summary>
        public static GDTask WaitForPhysicsProcess(CancellationToken cancellationToken)
        {
            return GDTask.Yield(PlayerLoopTiming.PhysicsProcess, cancellationToken);
        }

        /// <summary>
        /// Delay the execution after frame(s) of the provided <see cref="PlayerLoopTiming"/>, with specified <see cref="CancellationToken"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delayFrameCount"/> is less than 0.</exception>
        public static GDTask DelayFrame(int delayFrameCount, PlayerLoopTiming delayTiming = PlayerLoopTiming.Process, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (delayFrameCount < 0)
            {
                throw new ArgumentOutOfRangeException("Delay does not allow minus delayFrameCount. delayFrameCount:" + delayFrameCount);
            }

            return new GDTask(DelayFramePromise.Create(delayFrameCount, delayTiming, cancellationToken, out var token), token);
        }

        /// <summary>
        /// Delay the execution after <paramref name="millisecondsDelay"/> on provided <see cref="PlayerLoopTiming"/> with <see cref="DelayType.DeltaTime"/> provider, with specified <see cref="CancellationToken"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsDelay"/> is less than 0.</exception>
        public static GDTask Delay(int millisecondsDelay, PlayerLoopTiming delayTiming = PlayerLoopTiming.Process, CancellationToken cancellationToken = default(CancellationToken))
        {
            var delayTimeSpan = TimeSpan.FromMilliseconds(millisecondsDelay);
            return Delay(delayTimeSpan, delayTiming, cancellationToken);
        }

        /// <summary>
        /// Delay the execution after <paramref name="delayTimeSpan"/> on provided <see cref="PlayerLoopTiming"/> with <see cref="DelayType.DeltaTime"/> provider, with specified <see cref="CancellationToken"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delayTimeSpan"/> is less than 0.</exception>
        public static GDTask Delay(TimeSpan delayTimeSpan, PlayerLoopTiming delayTiming = PlayerLoopTiming.Process, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Delay(delayTimeSpan, DelayType.DeltaTime, delayTiming, cancellationToken);
        }

        /// <summary>
        /// Delay the execution after <paramref name="millisecondsDelay"/> on provided <see cref="PlayerLoopTiming"/> with <see cref="DelayType.DeltaTime"/> provider, with specified <see cref="CancellationToken"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsDelay"/> is less than 0.</exception>
        public static GDTask Delay(int millisecondsDelay, DelayType delayType, PlayerLoopTiming delayTiming = PlayerLoopTiming.Process, CancellationToken cancellationToken = default(CancellationToken))
        {
            var delayTimeSpan = TimeSpan.FromMilliseconds(millisecondsDelay);
            return Delay(delayTimeSpan, delayType, delayTiming, cancellationToken);
        }

        /// <summary>
        /// Delay the execution after <paramref name="delayTimeSpan"/> on provided <see cref="PlayerLoopTiming"/> with <see cref="DelayType.DeltaTime"/> provider, with specified <see cref="CancellationToken"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delayTimeSpan"/> is less than 0.</exception>
        public static GDTask Delay(TimeSpan delayTimeSpan, DelayType delayType, PlayerLoopTiming delayTiming = PlayerLoopTiming.Process, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (delayTimeSpan < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("Delay does not allow minus delayTimeSpan. delayTimeSpan:" + delayTimeSpan);
            }

#if DEBUG
            // force use Realtime.
            if (GDTaskPlayerLoopAutoload.IsMainThread && Engine.IsEditorHint())
            {
                if (delayType != DelayType.Realtime)
                {
                    GD.Print("When running by the editor's main thread, delayType must be DelayType.Realtime!");
                }
                delayType = DelayType.Realtime;
            }
#endif

            switch (delayType)
            {
                case DelayType.Realtime:
                    {
                        return new GDTask(DelayRealtimePromise.Create(delayTimeSpan, delayTiming, cancellationToken, out var token), token);
                    }
                case DelayType.DeltaTime:
                default:
                    {
                        return new GDTask(DelayPromise.Create(delayTimeSpan, delayTiming, cancellationToken, out var token), token);
                    }
            }
        }

        sealed class YieldPromise : IGDTaskSource, IPlayerLoopItem, ITaskPoolNode<YieldPromise>
        {
            static TaskPool<YieldPromise> pool;
            YieldPromise nextNode;
            public ref YieldPromise NextNode => ref nextNode;

            static YieldPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(YieldPromise), () => pool.Size);
            }

            CancellationToken cancellationToken;
            GDTaskCompletionSourceCore<object> core;

            YieldPromise()
            {
            }

            public static IGDTaskSource Create(PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetGDTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new YieldPromise();
                }


                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                GDTaskPlayerLoopAutoload.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
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

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        sealed class NextFramePromise : IGDTaskSource, IPlayerLoopItem, ITaskPoolNode<NextFramePromise>
        {
            static TaskPool<NextFramePromise> pool;
            NextFramePromise nextNode;
            public ref NextFramePromise NextNode => ref nextNode;

            static NextFramePromise()
            {
                TaskPool.RegisterSizeGetter(typeof(NextFramePromise), () => pool.Size);
            }

            bool isMainThread;
            ulong frameCount;
            CancellationToken cancellationToken;
            GDTaskCompletionSourceCore<AsyncUnit> core;

            NextFramePromise()
            {
            }

            public static IGDTaskSource Create(PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetGDTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new NextFramePromise();
                }

                result.isMainThread = GDTaskPlayerLoopAutoload.IsMainThread;
                if (result.isMainThread)
                    result.frameCount = Engine.GetProcessFrames();
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                GDTaskPlayerLoopAutoload.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
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

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (isMainThread && frameCount == Engine.GetProcessFrames())
                {
                    return true;
                }

                core.TrySetResult(AsyncUnit.Default);
                return false;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        sealed class DelayFramePromise : IGDTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayFramePromise>
        {
            static TaskPool<DelayFramePromise> pool;
            DelayFramePromise nextNode;
            public ref DelayFramePromise NextNode => ref nextNode;

            static DelayFramePromise()
            {
                TaskPool.RegisterSizeGetter(typeof(DelayFramePromise), () => pool.Size);
            }

            bool isMainThread;
            ulong initialFrame;
            int delayFrameCount;
            CancellationToken cancellationToken;

            int currentFrameCount;
            GDTaskCompletionSourceCore<AsyncUnit> core;

            DelayFramePromise()
            {
            }

            public static IGDTaskSource Create(int delayFrameCount, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetGDTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new DelayFramePromise();
                }

                result.delayFrameCount = delayFrameCount;
                result.cancellationToken = cancellationToken;
                result.isMainThread = GDTaskPlayerLoopAutoload.IsMainThread;
                if (result.isMainThread)
                    result.initialFrame = Engine.GetProcessFrames();

                TaskTracker.TrackActiveTask(result, 3);

                GDTaskPlayerLoopAutoload.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
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

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (currentFrameCount == 0)
                {
                    if (delayFrameCount == 0) // same as Yield
                    {
                        core.TrySetResult(AsyncUnit.Default);
                        return false;
                    }

                    // skip in initial frame.
                    if (isMainThread && initialFrame == Engine.GetProcessFrames())
                    {
#if DEBUG
                        // force use Realtime.
                        if (GDTaskPlayerLoopAutoload.IsMainThread && Engine.IsEditorHint())
                        {
                            //goto ++currentFrameCount
                        }
                        else
                        {
                            return true;
                        }
#else
                        return true;
#endif
                    }
                }

                if (++currentFrameCount >= delayFrameCount)
                {
                    core.TrySetResult(AsyncUnit.Default);
                    return false;
                }

                return true;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                currentFrameCount = default;
                delayFrameCount = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        sealed class DelayPromise : IGDTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayPromise>
        {
            static TaskPool<DelayPromise> pool;
            DelayPromise nextNode;
            public ref DelayPromise NextNode => ref nextNode;

            static DelayPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(DelayPromise), () => pool.Size);
            }

            bool isMainThread;
            ulong initialFrame;
            double delayTimeSpan;
            double elapsed;
            CancellationToken cancellationToken;
            PlayerLoopTiming timing;
            GDTaskCompletionSourceCore<object> core;

            DelayPromise()
            {
            }

            public static IGDTaskSource Create(TimeSpan delayTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetGDTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new DelayPromise();
                }

                result.elapsed = 0.0f;
                result.delayTimeSpan = (float)delayTimeSpan.TotalSeconds;
                result.cancellationToken = cancellationToken;
                result.isMainThread = GDTaskPlayerLoopAutoload.IsMainThread;
                result.timing = timing;
                if (result.isMainThread)
                    result.initialFrame = Engine.GetProcessFrames();

                TaskTracker.TrackActiveTask(result, 3);

                GDTaskPlayerLoopAutoload.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
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

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (elapsed == 0.0f)
                {
                    if (isMainThread && initialFrame == Engine.GetProcessFrames())
                    {
                        return true;
                    }
                }

                elapsed += timing == PlayerLoopTiming.Process ? 
                    GDTaskPlayerLoopAutoload.Global.DeltaTime : 
                    GDTaskPlayerLoopAutoload.Global.PhysicsDeltaTime;
                
                if (elapsed >= delayTimeSpan)
                {
                    core.TrySetResult(null);
                    return false;
                }

                return true;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                delayTimeSpan = default;
                elapsed = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        sealed class DelayRealtimePromise : IGDTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayRealtimePromise>
        {
            static TaskPool<DelayRealtimePromise> pool;
            DelayRealtimePromise nextNode;
            public ref DelayRealtimePromise NextNode => ref nextNode;

            static DelayRealtimePromise()
            {
                TaskPool.RegisterSizeGetter(typeof(DelayRealtimePromise), () => pool.Size);
            }

            long delayTimeSpanTicks;
            ValueStopwatch stopwatch;
            CancellationToken cancellationToken;

            GDTaskCompletionSourceCore<AsyncUnit> core;

            DelayRealtimePromise()
            {
            }

            public static IGDTaskSource Create(TimeSpan delayTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetGDTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new DelayRealtimePromise();
                }

                result.stopwatch = ValueStopwatch.StartNew();
                result.delayTimeSpanTicks = delayTimeSpan.Ticks;
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                GDTaskPlayerLoopAutoload.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
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

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (stopwatch.IsInvalid)
                {
                    core.TrySetResult(AsyncUnit.Default);
                    return false;
                }

                if (stopwatch.ElapsedTicks >= delayTimeSpanTicks)
                {
                    core.TrySetResult(AsyncUnit.Default);
                    return false;
                }

                return true;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                stopwatch = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }
    }

    /// <summary>
    /// An awaitable that when awaited, asynchronously yields back to the next specified <see cref="PlayerLoopTiming"/>.
    /// </summary>
    public readonly struct YieldAwaitable
    {
        readonly PlayerLoopTiming timing;

        internal YieldAwaitable(PlayerLoopTiming timing)
        {
            this.timing = timing;
        }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="YieldAwaitable"/>.
        /// </summary>
        public Awaiter GetAwaiter()
        {
            return new Awaiter(timing);
        }

        /// <summary>
        /// Creates a <see cref="GDTask"/> that represents this <see cref="YieldAwaitable"/>.
        /// </summary>
        public GDTask ToGDTask()
        {
            return GDTask.Yield(timing, CancellationToken.None);
        }

        /// <summary>
        /// Provides an awaiter for awaiting a <see cref="YieldAwaitable"/>.
        /// </summary>
        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            readonly PlayerLoopTiming timing;

            /// <summary>
            /// Initializes the <see cref="Awaiter"/>.
            /// </summary>
            public Awaiter(PlayerLoopTiming timing)
            {
                this.timing = timing;
            }

            /// <summary>
            /// Gets whether this <see cref="YieldAwaitable">Task</see> has completed, always returns false.
            /// </summary>
            public bool IsCompleted => false;

            /// <summary>
            /// Do nothing
            /// </summary>
            public void GetResult() { }

            /// <summary>
            /// Schedules the continuation onto the <see cref="YieldAwaitable"/> associated with this <see cref="Awaiter"/>.
            /// </summary>
            public void OnCompleted(Action continuation)
            {
                GDTaskPlayerLoopAutoload.AddContinuation(timing, continuation);
            }

            /// <summary>
            /// Schedules the continuation onto the <see cref="YieldAwaitable"/> associated with this <see cref="Awaiter"/>.
            /// </summary>
            public void UnsafeOnCompleted(Action continuation)
            {
                GDTaskPlayerLoopAutoload.AddContinuation(timing, continuation);
            }
        }
    }
}
