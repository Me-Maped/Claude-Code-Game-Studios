using System;
using System.Collections.Generic;

namespace LynxFramework.Core;

/// <summary>
/// 更新阶段枚举。
/// </summary>
public enum UpdatePhase
{
    Process,
    PhysicsProcess
}

/// <summary>
/// 更新条目句柄。调用方可通过 IsActive 暂停或通过 Unregister 完全移除。
/// </summary>
public sealed class UpdateEntry
{
    public UpdatePhase Phase;
    public int Priority;
    public Action<float> Callback;
    public bool IsActive = true;
}

/// <summary>
/// 统一主循环调度器。
/// 支持 PROCESS / PHYSICS_PROCESS 两种阶段与优先级排序。
/// 排序仅在注册变更时触发（脏标记），避免每帧排序开销。
/// </summary>
public class UpdateDriver : IModule
{
    private readonly List<UpdateEntry> _processEntries = new();
    private readonly List<UpdateEntry> _physicsEntries = new();
    private bool _dirty = true;
    private bool _isRunning;

    public int Priority => 40;
    public int FrameCount { get; private set; }

    public void OnInit() { }
    public void OnShutdown() => Stop();

    /// <summary>注册更新回调，返回句柄用于后续管理。</summary>
    public UpdateEntry Register(UpdatePhase phase, int priority, Action<float> callback)
    {
        var entry = new UpdateEntry { Phase = phase, Priority = priority, Callback = callback };
        var list = phase == UpdatePhase.Process ? _processEntries : _physicsEntries;
        list.Add(entry);
        _dirty = true;
        return entry;
    }

    /// <summary>注销更新回调。</summary>
    public void Unregister(UpdateEntry entry)
    {
        var list = entry.Phase == UpdatePhase.Process ? _processEntries : _physicsEntries;
        list.Remove(entry);
    }

    /// <summary>驱动 Process 阶段更新。由 FrameworkEntry 调用。</summary>
    public void DriveProcess(float delta)
    {
        if (!_isRunning) return;
        EnsureSorted();
        FrameCount++;
        for (int i = 0; i < _processEntries.Count; i++)
        {
            if (_processEntries[i].IsActive)
                _processEntries[i].Callback(delta);
        }
    }

    /// <summary>驱动 PhysicsProcess 阶段更新。由 FrameworkEntry 调用。</summary>
    public void DrivePhysicsProcess(float delta)
    {
        if (!_isRunning) return;
        EnsureSorted();
        for (int i = 0; i < _physicsEntries.Count; i++)
        {
            if (_physicsEntries[i].IsActive)
                _physicsEntries[i].Callback(delta);
        }
    }

    /// <summary>启动更新驱动。</summary>
    public void Start() => _isRunning = true;

    /// <summary>停止更新驱动。</summary>
    public void Stop() => _isRunning = false;

    private void EnsureSorted()
    {
        if (!_dirty) return;
        _processEntries.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        _physicsEntries.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        _dirty = false;
    }
}
