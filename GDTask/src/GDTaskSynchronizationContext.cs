using Godot;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GodotTasks.Tasks
{
    /// <summary>
    /// Provides the functionality for propagating a synchronization context in <see cref="GDTask"/> synchronization models.
    /// </summary>
    public sealed class GDTaskSynchronizationContext : SynchronizationContext
    {
        private const int MaxArrayLength = 0X7FEFFFFF;
        private const int InitialSize = 16;

        private static SpinLock gate = new SpinLock(false);
        private static bool dequeuing = false;

        private static int actionListCount = 0;
        private static Callback[] actionList = new Callback[InitialSize];

        private static int waitingListCount = 0;
        private static Callback[] waitingList = new Callback[InitialSize];

        private static int opCount;

        internal GDTaskSynchronizationContext() { }
        
        /// <summary>
        /// Dispatches a synchronous message to a synchronization context.
        /// </summary>
        public override void Send(SendOrPostCallback d, object state)
        {
            d(state);
        }

        /// <summary>
        /// Dispatches an asynchronous message to a synchronization context.
        /// </summary>
        public override void Post(SendOrPostCallback d, object state)
        {
            bool lockTaken = false;
            try
            {
                gate.Enter(ref lockTaken);

                if (dequeuing)
                {
                    // Ensure Capacity
                    if (waitingList.Length == waitingListCount)
                    {
                        var newLength = waitingListCount * 2;
                        if ((uint)newLength > MaxArrayLength) newLength = MaxArrayLength;

                        var newArray = new Callback[newLength];
                        Array.Copy(waitingList, newArray, waitingListCount);
                        waitingList = newArray;
                    }
                    waitingList[waitingListCount] = new Callback(d, state);
                    waitingListCount++;
                }
                else
                {
                    // Ensure Capacity
                    if (actionList.Length == actionListCount)
                    {
                        var newLength = actionListCount * 2;
                        if ((uint)newLength > MaxArrayLength) newLength = MaxArrayLength;

                        var newArray = new Callback[newLength];
                        Array.Copy(actionList, newArray, actionListCount);
                        actionList = newArray;
                    }
                    actionList[actionListCount] = new Callback(d, state);
                    actionListCount++;
                }
            }
            finally
            {
                if (lockTaken) gate.Exit(false);
            }
        }

        /// <summary>
        /// Responds to the notification that an operation has started.
        /// </summary>
        public override void OperationStarted()
        {
            Interlocked.Increment(ref opCount);
        }

        /// <summary>
        /// Responds to the notification that an operation has completed.
        /// </summary>
        public override void OperationCompleted()
        {
            Interlocked.Decrement(ref opCount);
        }

        /// <summary>
        /// Returns the current <see cref="GodotSynchronizationContext"/>
        /// </summary>
        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        // delegate entrypoint.
        internal static void Run()
        {
            {
                bool lockTaken = false;
                try
                {
                    gate.Enter(ref lockTaken);
                    if (actionListCount == 0) return;
                    dequeuing = true;
                }
                finally
                {
                    if (lockTaken) gate.Exit(false);
                }
            }

            for (int i = 0; i < actionListCount; i++)
            {
                var action = actionList[i];
                actionList[i] = default;
                action.Invoke();
            }

            {
                bool lockTaken = false;
                try
                {
                    gate.Enter(ref lockTaken);
                    dequeuing = false;

                    var swapTempActionList = actionList;

                    actionListCount = waitingListCount;
                    actionList = waitingList;

                    waitingListCount = 0;
                    waitingList = swapTempActionList;
                }
                finally
                {
                    if (lockTaken) gate.Exit(false);
                }
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct Callback
        {
            private readonly SendOrPostCallback callback;
            private readonly object state;

            public Callback(SendOrPostCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public void Invoke()
            {
                try
                {
                    callback(state);
                }
                catch (Exception ex)
                {
                    GD.PrintErr(ex);
                }
            }
        }
    }
}