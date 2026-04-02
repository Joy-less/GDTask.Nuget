using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Godot;
using GodotTask.Internal;

[assembly: InternalsVisibleTo("GDTask.Tests")]

namespace GodotTask;

#nullable enable
partial class PlayerLoopRunnerProvider : Node
{
    private static PlayerLoopRunnerProvider? Global;
    private static int InitializationRequested;

    private static readonly PlayerLoopProxy DeferredProxy = new();
    private static readonly PlayerLoopProxy IsolatedPhysicsProcessProxy = new();
    private static readonly PlayerLoopProxy IsolatedProcessProxy = new();
    private static readonly PlayerLoopProxy PhysicsProcessProxy = new();
    private static readonly PlayerLoopProxy ProcessProxy = new();

    private readonly Variant[] _deferredArgs = new Variant[1];

    private PlayerLoopRunnerProvider()
    {
        var isolatedPlayerLoopRunner = new IsolatedGDTaskPlayerLoopRunner(IsolatedProcessProxy, IsolatedPhysicsProcessProxy);
        AddChild(isolatedPlayerLoopRunner);
        isolatedPlayerLoopRunner.Name = "IsolatedGDTaskPlayerLoopRunner";
    }

    internal static PlayerLoopRunnerProvider GlobalInstance
    {
        get
        {
            EnsureInitialized();
            return Global ?? throw new InvalidOperationException("Player loop runner provider is pending main-thread initialization.");
        }
    }

    public static IPlayerLoop Process
    {
        get
        {
            EnsureInitialized();
            return ProcessProxy;
        }
    }

    public static IPlayerLoop PhysicsProcess
    {
        get
        {
            EnsureInitialized();
            return PhysicsProcessProxy;
        }
    }

    public static IPlayerLoop IsolatedProcess
    {
        get
        {
            EnsureInitialized();
            return IsolatedProcessProxy;
        }
    }

    public static IPlayerLoop IsolatedPhysicsProcess
    {
        get
        {
            EnsureInitialized();
            return IsolatedPhysicsProcessProxy;
        }
    }

    public static IPlayerLoop Deferred
    {
        get
        {
            EnsureInitialized();
            return DeferredProxy;
        }
    }

    private static void EnsureInitialized()
    {
        RuntimeChecker.ThrowIfEditor();
        if (Global != null) return;

        if (!GDTaskScheduler.IsMainThread)
        {
            RequestMainThreadInitialization();
            return;
        }

        CreateGlobalInstance();
    }

    private static void RequestMainThreadInitialization()
    {
        if (Interlocked.Exchange(ref InitializationRequested, 1) != 0) return;
        Dispatcher.SynchronizationContext.Post(
            static _ =>
            {
                if (Global != null) return;
                CreateGlobalInstance();
            },
            null
        );
    }

    private static void CreateGlobalInstance()
    {
        if (Global != null) return;

        var newInstance = new PlayerLoopRunnerProvider();
        var root = ((SceneTree)Engine.GetMainLoop()).Root;
        root.CallDeferred(Node.MethodName.AddChild, newInstance, false, Variant.From(InternalMode.Front));
        newInstance.Name = "GDTaskPlayerLoopRunner";
        Global = newInstance;
        Volatile.Write(ref InitializationRequested, 1);
    }

    public override void _Ready()
    {
        if (Global == null)
        {
            Global = this;
            return;
        }

        if (Global == this) return;
        QueueFree();
    }

    public override void _Notification(int what)
    {
        if (what != NotificationPredelete) return;
        if (Global != this) return;
        ProcessProxy.NotifyPredelete();
        PhysicsProcessProxy.NotifyPredelete();
        IsolatedProcessProxy.NotifyPredelete();
        IsolatedPhysicsProcessProxy.NotifyPredelete();
        DeferredProxy.NotifyPredelete();
        Global = null;
        Volatile.Write(ref InitializationRequested, 0);
    }

    public override void _Process(double delta)
    {
        ProcessProxy.NotifyProcess(delta);
        _deferredArgs[0] = delta;
        CallDeferred(MethodName.DeferredProcess, _deferredArgs);
    }

    public override void _PhysicsProcess(double delta) => PhysicsProcessProxy.NotifyProcess(delta);

    private void DeferredProcess(double delta) =>
        DeferredProxy.NotifyProcess(delta);
}

class PlayerLoopProxy : IPlayerLoop
{
    public event Action<double>? OnProcess;
    public event Action? OnPredelete;
    public void NotifyProcess(double delta) => OnProcess?.Invoke(delta);
    public void NotifyPredelete() => OnPredelete?.Invoke();
}