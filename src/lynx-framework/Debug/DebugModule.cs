using System;
using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Pool;

namespace LynxFramework.Debug;

/// <summary>
/// 调试模块。
/// 提供调试 UI、命令注册、统计信息展示。
/// </summary>
public class DebugModule : IModule
{
    private EventBus _eventBus;
    private LogService _logService;
    private PoolManager _poolManager;
    private readonly Dictionary<string, Action<string[]>> _commands = new();
    private readonly Dictionary<string, Func<float>> _stats = new();
    private Control _debugPanel;
    private bool _isVisible;

    public int Priority => 190;

    public void OnInit()
    {
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
        _poolManager = FrameworkEntry.Instance.GetModule<PoolManager>();

        RegisterBuiltinCommands();
        RegisterBuiltinStats();
    }

    public void OnShutdown()
    {
        _debugPanel?.QueueFree();
        _commands.Clear();
        _stats.Clear();
    }

    /// <summary>显示调试 UI。</summary>
    public void ShowDebugUI()
    {
        if (_debugPanel == null) CreateDebugPanel();
        _debugPanel.Visible = true;
        _isVisible = true;
    }

    /// <summary>隐藏调试 UI。</summary>
    public void HideDebugUI()
    {
        if (_debugPanel != null) _debugPanel.Visible = false;
        _isVisible = false;
    }

    /// <summary>切换调试 UI 显示。</summary>
    public void ToggleDebugUI()
    {
        if (_isVisible) HideDebugUI(); else ShowDebugUI();
    }

    /// <summary>注册自定义命令。</summary>
    public void RegisterCommand(string name, Action<string[]> callback) => _commands[name] = callback;

    /// <summary>执行命令字符串。</summary>
    public void ExecuteCommand(string cmdString)
    {
        var parts = cmdString.Split(' ');
        var cmdName = parts[0];
        var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (_commands.TryGetValue(cmdName, out var callback))
        {
            callback(args);
        }
        else
        {
            _logService?.Warning($"Unknown command: {cmdName}");
        }
    }

    /// <summary>注册统计信息提供者。</summary>
    public void RegisterStat(string statName, Func<float> provider) => _stats[statName] = provider;

    /// <summary>注销统计信息。</summary>
    public void UnregisterStat(string statName) => _stats.Remove(statName);

    /// <summary>获取所有统计信息。</summary>
    public Dictionary<string, float> GetStats()
    {
        var result = new Dictionary<string, float>();
        foreach (var kv in _stats)
        {
            try { result[kv.Key] = kv.Value(); }
            catch { result[kv.Key] = 0f; }
        }
        return result;
    }

    /// <summary>检查调试 UI 是否可见。</summary>
    public bool IsVisible => _isVisible;

    private void RegisterBuiltinCommands()
    {
        RegisterCommand("help", CmdHelp);
        RegisterCommand("pool_stats", CmdPoolStats);
        RegisterCommand("fps", CmdFPS);
    }

    private void RegisterBuiltinStats()
    {
        RegisterStat("fps", StatFPS);
        RegisterStat("frame_time", StatFrameTime);
        RegisterStat("object_count", StatObjectCount);
    }

    private void CmdHelp(string[] args)
    {
        _logService?.Info("Available commands:");
        foreach (var cmd in _commands.Keys)
            _logService?.Info($"  {cmd}");
    }

    private void CmdPoolStats(string[] args)
    {
        var stats = _poolManager?.GetPoolStats();
        if (stats == null || stats.Count == 0)
        {
            _logService?.Info("No node pools active.");
            return;
        }
        foreach (var kv in stats)
            _logService?.Info($"  {kv.Key}: active={kv.Value.ActiveCount}, available={kv.Value.AvailableCount}, hitRate={kv.Value.HitRate:P}");
    }

    private void CmdFPS(string[] args) => _logService?.Info($"FPS: {Engine.GetFramesPerSecond()}");

    private float StatFPS() => (float)Engine.GetFramesPerSecond();
    private float StatFrameTime() => 1000f / Mathf.Max(1f, (float)Engine.GetFramesPerSecond());
    private float StatObjectCount() => (float)Performance.GetMonitor(Performance.Monitor.ObjectCount);

    private void CreateDebugPanel()
    {
        _debugPanel = new Control { Name = "DebugPanel" };
        var canvasLayer = new CanvasLayer { Name = "DebugLayer", Layer = 999 };
        canvasLayer.AddChild(_debugPanel);
        FrameworkEntry.Instance.AddChild(canvasLayer);
        _logService?.Info("Debug panel created. Press ToggleDebugUI() to show.");
    }
}
