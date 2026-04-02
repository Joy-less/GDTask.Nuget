using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GodotTask.Internal;

namespace GodotTask;

/// <summary>
/// A conditional component that tracks and logs active tasks.
/// </summary>
public static partial class TaskTracker
{
    private static int TrackingId;
    internal static readonly ObservableProperty EnableTrackingObservable = new(false);
    internal static readonly ObservableProperty EnableStackTraceObservable = new(true);

    private static readonly ConditionalWeakTable<IGDTaskSource, TrackingData> Tracking = [];
    /// <summary>
    /// Enable tracking for active tasks.
    /// </summary>
    public static bool EnableTracking
    {
        get => EnableTrackingObservable.Value;
        set => EnableTrackingObservable.Value = value;
    }

    /// <summary>
    /// Record StackTrace for tracked tasks.
    /// </summary>
    public static bool EnableStackTrace
    {
        get => EnableStackTraceObservable.Value;
        set => EnableStackTraceObservable.Value = value;
    }

    /// <summary>
    /// Shows the task tracker window if not already.
    /// </summary>
    /// <remarks>
    /// This also sets <see cref="EnableTracking" /> to true.
    /// </remarks>
    public static void ShowTrackerWindow()
    {
        EnableTracking = true;
        TaskTrackerWindow.Launch();
    }

    internal static void TrackActiveTask(IGDTaskSource task, int skipFrame)
    {
        if (!EnableTrackingObservable.Value) return;
        var stackTrace = EnableStackTraceObservable.Value ? new StackTrace(skipFrame, true).ToString()[6..] : "";
        var typeName = EnableStackTraceObservable.Value ? TypePrinter.ConstructTypeName(task.GetType()) : task.GetType().Name;
        var trackingData = new TrackingData(typeName, Interlocked.Increment(ref TrackingId), DateTime.UtcNow, stackTrace, task.UnsafeGetStatus);
        Tracking.AddOrUpdate(task, trackingData);
        TaskTrackerWindow.TryAddItem(trackingData);
    }

    internal static void RemoveTracking(IGDTaskSource task)
    {
        if (!EnableTrackingObservable.Value) return;
        if (!Tracking.TryGetValue(task, out var trackingData)) return;
        Tracking.Remove(task);
        TaskTrackerWindow.TryRemoveItem(trackingData);
    }

    internal static IEnumerable<TrackingData> GetAllExistingTrackingData()
    {
        foreach (var (_, trackingData) in Tracking) yield return trackingData;
    }
}