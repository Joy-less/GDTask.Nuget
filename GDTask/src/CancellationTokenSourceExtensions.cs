using System;
using System.Threading;
using GodotTask.Internal;

namespace GodotTask;

/// <summary>
/// Provides extensions methods for <see cref="CancellationTokenSource" />.
/// </summary>
public static class CancellationTokenSourceExtensions
{
    private static void CancelCancellationTokenSourceState(object state)
    {
        var cts = (CancellationTokenSource)state;
        cts.Cancel();
    }

    extension(CancellationTokenSource cts)
    {
        /// <inheritdoc cref="CancelAfterSlim(CancellationTokenSource, int, DelayType, PlayerLoopTiming)" />
        public IDisposable CancelAfterSlim(int millisecondsDelay, DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming delayTiming = PlayerLoopTiming.Process) => 
            cts.CancelAfterSlim(TimeSpan.FromMilliseconds(millisecondsDelay), delayType, delayTiming);

        /// <summary>
        /// Cancel this <see cref="CancellationTokenSource" /> after a given <paramref name="millisecondsDelay" />.
        /// </summary>
        /// <returns>A <see cref="PlayerLoopTimer" /> that, when disposed, aborts the timing session</returns>
        public IDisposable CancelAfterSlim(int millisecondsDelay, DelayType delayType, IPlayerLoop delayLoop) => 
            cts.CancelAfterSlim(TimeSpan.FromMilliseconds(millisecondsDelay), delayType, delayLoop);

        /// <summary>
        /// Cancel this <see cref="CancellationTokenSource" /> after a given <paramref name="delayTimeSpan" />.
        /// </summary>
        /// <returns>A <see cref="PlayerLoopTimer" /> that, when disposed, aborts the timing session</returns>
        public IDisposable CancelAfterSlim(TimeSpan delayTimeSpan, DelayType delayType = DelayType.DeltaTime, PlayerLoopTiming delayTiming = PlayerLoopTiming.Process) => 
            PlayerLoopTimer.StartNew(delayTimeSpan, false, delayType, delayTiming, cts.Token, CancelCancellationTokenSourceState, cts);

        /// <summary>
        /// Cancel this <see cref="CancellationTokenSource" /> after a given <paramref name="delayTimeSpan" />.
        /// </summary>
        /// <returns>A <see cref="PlayerLoopTimer" /> that, when disposed, aborts the timing session</returns>
        public IDisposable CancelAfterSlim(TimeSpan delayTimeSpan, DelayType delayType, IPlayerLoop delayLoop)
        {
            Error.ThrowArgumentNullException(delayLoop, nameof(delayLoop));
            return PlayerLoopTimer.StartNew(delayTimeSpan, false, delayType, delayLoop, cts.Token, CancelCancellationTokenSourceState, cts);
        }
    }
}