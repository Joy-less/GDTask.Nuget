using System;
using System.Threading;
using Godot;
using GodotTask.Internal;

namespace GodotTask;

abstract class PlayerLoopTimer(bool periodic, IPlayerLoop playerLoop, CancellationToken cancellationToken, Action<object> timerCallback, object state)
    : IDisposable, IPlayerLoopItem
{
    protected readonly bool UsesEngineFrameBoundary = GDTaskScheduler.UsesEngineFrameBoundary(playerLoop);
    private bool _isDisposed;

    private bool _isRunning;
    private bool _tryStop;

    public void Dispose() => _isDisposed = true;

    bool IPlayerLoopItem.MoveNext(double deltaTime)
    {
        if (_isDisposed || _tryStop || cancellationToken.IsCancellationRequested)
        {
            _isRunning = false;
            return false;
        }

        if (!MoveNextCore(deltaTime))
        {
            timerCallback(state);

            if (periodic)
            {
                ResetCore(null);
                return true;
            }

            _isRunning = false;
            return false;
        }

        return true;
    }

    public static PlayerLoopTimer Create(TimeSpan interval, bool periodic, DelayType delayType, PlayerLoopTiming playerLoopTiming, CancellationToken cancellationToken, Action<object> timerCallback, object state) => Create(interval, periodic, delayType, GDTaskScheduler.GetPlayerLoop(playerLoopTiming), cancellationToken, timerCallback, state);

    public static PlayerLoopTimer Create(TimeSpan interval, bool periodic, DelayType delayType, IPlayerLoop playerLoop, CancellationToken cancellationToken, Action<object> timerCallback, object state)
    {
        // Force use Realtime.
        if (GDTaskScheduler.IsMainThread && Engine.IsEditorHint()) delayType = DelayType.Realtime;

        switch (delayType)
        {
            case DelayType.Realtime:
                return new RealtimePlayerLoopTimer(interval, periodic, playerLoop, cancellationToken, timerCallback, state);
            case DelayType.DeltaTime:
            default:
                return new DeltaTimePlayerLoopTimer(interval, periodic, playerLoop, cancellationToken, timerCallback, state);
        }
    }

    public static PlayerLoopTimer StartNew(TimeSpan interval, bool periodic, DelayType delayType, PlayerLoopTiming playerLoopTiming, CancellationToken cancellationToken, Action<object> timerCallback, object state) => StartNew(interval, periodic, delayType, GDTaskScheduler.GetPlayerLoop(playerLoopTiming), cancellationToken, timerCallback, state);

    public static PlayerLoopTimer StartNew(TimeSpan interval, bool periodic, DelayType delayType, IPlayerLoop playerLoop, CancellationToken cancellationToken, Action<object> timerCallback, object state)
    {
        var timer = Create(interval, periodic, delayType, playerLoop, cancellationToken, timerCallback, state);
        timer.Restart();
        return timer;
    }

    /// <summary>
    /// Restart(Reset and Start) timer.
    /// </summary>
    public void Restart()
    {
        if (_isDisposed) throw new ObjectDisposedException(null);

        ResetCore(null); // init state

        if (!_isRunning)
        {
            _isRunning = true;
            GDTaskScheduler.AddAction(playerLoop, this);
        }

        _tryStop = false;
    }

    /// <summary>
    /// Restart(Reset and Start) and change interval.
    /// </summary>
    public void Restart(TimeSpan interval)
    {
        if (_isDisposed) throw new ObjectDisposedException(null);

        ResetCore(interval); // init state

        if (!_isRunning)
        {
            _isRunning = true;
            GDTaskScheduler.AddAction(playerLoop, this);
        }

        _tryStop = false;
    }

    /// <summary>
    /// Stop timer.
    /// </summary>
    public void Stop() => _tryStop = true;

    protected abstract void ResetCore(TimeSpan? newInterval);

    protected abstract bool MoveNextCore(double deltaTime);
}

sealed class DeltaTimePlayerLoopTimer : PlayerLoopTimer
{
    private double _elapsed;
    private ulong _initialFrame;
    private double _interval;
    private bool _isMainThread;

    public DeltaTimePlayerLoopTimer(TimeSpan interval, bool periodic, IPlayerLoop playerLoop, CancellationToken cancellationToken, Action<object> timerCallback, object state)
        : base(periodic, playerLoop, cancellationToken, timerCallback, state)
    {
        ResetCore(interval);
    }

    protected override bool MoveNextCore(double deltaTime)
    {
        if (_elapsed == 0.0)
            // Match built-in player loop behavior by waiting for the next engine frame,
            // but do not suppress the first manual tick of a custom IPlayerLoop.
            if (_isMainThread && _initialFrame == Engine.GetProcessFrames())
                return true;

        _elapsed += deltaTime;
        if (_elapsed >= _interval) return false;

        return true;
    }

    protected override void ResetCore(TimeSpan? interval)
    {
        _elapsed = 0.0;
        _isMainThread = UsesEngineFrameBoundary && GDTaskScheduler.IsMainThread;
        if (_isMainThread)
            _initialFrame = Engine.GetProcessFrames();
        if (interval != null) _interval = (float)interval.Value.TotalSeconds;
    }
}

sealed class RealtimePlayerLoopTimer : PlayerLoopTimer
{
    private long _intervalTicks;
    private ValueStopwatch _stopwatch;

    public RealtimePlayerLoopTimer(TimeSpan interval, bool periodic, IPlayerLoop playerLoop, CancellationToken cancellationToken, Action<object> timerCallback, object state)
        : base(periodic, playerLoop, cancellationToken, timerCallback, state)
    {
        ResetCore(interval);
    }

    protected override bool MoveNextCore(double deltaTime)
    {
        if (_stopwatch.ElapsedTicks >= _intervalTicks) return false;

        return true;
    }

    protected override void ResetCore(TimeSpan? interval)
    {
        _stopwatch = ValueStopwatch.StartNew();
        if (interval != null) _intervalTicks = interval.Value.Ticks;
    }
}