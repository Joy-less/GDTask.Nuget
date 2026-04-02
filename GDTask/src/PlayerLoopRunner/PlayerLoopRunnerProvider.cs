using System;
using System.Runtime.CompilerServices;
using Godot;
using GodotTask.Internal;

[assembly: InternalsVisibleTo("GDTask.Tests")]

namespace GodotTask;

#nullable enable
partial class PlayerLoopRunnerProvider : Node
{
    private static PlayerLoopRunnerProvider? Global;

    private readonly Variant[] _deferredArgs = new Variant[1];
    private readonly PlayerLoopProxy _deferredProxy;
    private readonly PlayerLoopProxy _isolatedPhysicsProcessProxy;
    private readonly PlayerLoopProxy _isolatedProcessProxy;
    private readonly PlayerLoopProxy _physicsProcessProxy;

    private readonly PlayerLoopProxy _processProxy;

    private PlayerLoopRunnerProvider()
    {
        _processProxy = new();
        _physicsProcessProxy = new();
        _isolatedProcessProxy = new();
        _isolatedPhysicsProcessProxy = new();
        _deferredProxy = new();
        var isolatedPlayerLoopRunner = new IsolatedGDTaskPlayerLoopRunner(_isolatedProcessProxy, _isolatedPhysicsProcessProxy);
        AddChild(isolatedPlayerLoopRunner);
        isolatedPlayerLoopRunner.Name = "IsolatedGDTaskPlayerLoopRunner";
    }

    internal static PlayerLoopRunnerProvider GlobalInstance
    {
        get
        {
            RuntimeChecker.ThrowIfEditor();
            if (Global != null) return Global;
            var newInstance = new PlayerLoopRunnerProvider();
            var root = ((SceneTree)Engine.GetMainLoop()).Root;
            root.CallDeferred(Node.MethodName.AddChild, newInstance, false, Variant.From(InternalMode.Front));
            newInstance.Name = "GDTaskPlayerLoopRunner";
            Global = newInstance;
            return Global;
        }
    }

    public static IPlayerLoop Process => GlobalInstance._processProxy;
    public static IPlayerLoop PhysicsProcess => GlobalInstance._physicsProcessProxy;
    public static IPlayerLoop IsolatedProcess => GlobalInstance._isolatedProcessProxy;
    public static IPlayerLoop IsolatedPhysicsProcess => GlobalInstance._isolatedPhysicsProcessProxy;
    public static IPlayerLoop Deferred => GlobalInstance._deferredProxy;

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
        _processProxy.NotifyPredelete();
        _physicsProcessProxy.NotifyPredelete();
        _deferredProxy.NotifyPredelete();
        if (Global != this) return;
        Global = null;
    }

    public override void _Process(double delta)
    {
        _processProxy.NotifyProcess(delta);
        _deferredArgs[0] = delta;
        CallDeferred(MethodName.DeferredProcess, _deferredArgs);
    }

    public override void _PhysicsProcess(double delta) => _physicsProcessProxy.NotifyProcess(delta);

    private void DeferredProcess(double delta) =>
        _deferredProxy.NotifyProcess(delta);
}

class PlayerLoopProxy : IPlayerLoop
{
    public event Action<double>? OnProcess;
    public event Action? OnPredelete;
    public void NotifyProcess(double delta) => OnProcess?.Invoke(delta);
    public void NotifyPredelete() => OnPredelete?.Invoke();
}