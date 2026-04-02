using System;
using System.Diagnostics;

namespace GodotTask.Internal;

readonly struct ValueStopwatch
{
    private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

    private readonly long _startTimestamp;

    public static ValueStopwatch StartNew() => new(Stopwatch.GetTimestamp());

    private ValueStopwatch(long startTimestamp)
    {
        _startTimestamp = startTimestamp;
    }

    public TimeSpan Elapsed => TimeSpan.FromTicks(ElapsedTicks);

    public bool IsInvalid => _startTimestamp == 0;

    public long ElapsedTicks
    {
        get
        {
            if (_startTimestamp == 0) throw new InvalidOperationException("Detected invalid initialization - use 'StartNew()', not 'default'.");

            var delta = Stopwatch.GetTimestamp() - _startTimestamp;
            return (long)(delta * TimestampToTicks);
        }
    }
}