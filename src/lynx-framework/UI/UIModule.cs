using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Pool;
using LynxFramework.Resource;

namespace LynxFramework.UI;

/// <summary>
/// UI 管理模块。
/// 栈式管理 UI 面板生命周期，支持转场动画和输入拦截。
/// </summary>
public class UIModule : IModule
{
    private ResourceService _resourceService;
    private PoolManager _poolManager;
    private EventBus _eventBus;
    private LogService _logService;
    private CanvasLayer _uiLayer;
    private readonly List<UIStackEntry> _stack = new();

    public int Priority => 130;

    public void OnInit()
    {
        _resourceService = FrameworkEntry.Instance.GetModule<ResourceService>();
        _poolManager = FrameworkEntry.Instance.GetModule<PoolManager>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();

        _uiLayer = new CanvasLayer { Name = "UILayer", Layer = 100 };
        FrameworkEntry.Instance.AddChild(_uiLayer);
    }

    public void OnShutdown() => CloseAllUI();

    /// <summary>设置 UI 层（可自定义 CanvasLayer）。</summary>
    public void SetUILayer(CanvasLayer layer) => _uiLayer = layer;

    /// <summary>打开 UI 面板。</summary>
    public void OpenUI(string uiName, UIOpenParam data = null, IUITransition transition = null)
    {
        data ??= UIOpenParam.Empty;

        // 如果已在栈中，将其移到顶部
        var existingIndex = _stack.FindIndex(e => e.Name == uiName);
        if (existingIndex >= 0)
        {
            var existing = _stack[existingIndex];
            _stack.RemoveAt(existingIndex);
            _stack.Add(existing);
            existing.PanelInterface?.OnResume();
            return;
        }

        // 实例化 UI 面板
        var scenePath = $"res://ui/{uiName}.tscn";
        var scene = _resourceService.LoadCached<PackedScene>(scenePath);
        if (scene == null)
        {
            _logService?.Warning($"UI scene not found: {scenePath}");
            return;
        }

        var instance = scene.Instantiate<Control>();
        _uiLayer.AddChild(instance);

        var entry = new UIStackEntry
        {
            Name = uiName,
            Panel = instance,
            PanelInterface = instance as IUIPanel,
            Transition = transition
        };

        // 通知当前栈顶面板被覆盖
        if (_stack.Count > 0)
        {
            var top = _stack[^1];
            top.PanelInterface?.OnCover();
            top.PanelInterface?.OnPause();
        }

        _stack.Add(entry);

        // 拦截底层 UI 输入
        for (int i = 0; i < _stack.Count - 1; i++)
            _stack[i].Panel.MouseFilter = Control.MouseFilterEnum.Ignore;

        // 播放过渡动画
        if (transition != null)
            transition.PlayOpen(instance, () => { });
        else
            instance.Visible = true;

        entry.PanelInterface?.OnOpen(data);
        _eventBus?.Emit("ui_opened", new UIEventArg { UIName = uiName });
        _logService?.Info($"UI opened: {uiName}");
    }

    /// <summary>关闭指定 UI 面板。</summary>
    public void CloseUI(string uiName)
    {
        var index = _stack.FindIndex(e => e.Name == uiName);
        if (index < 0) return;

        var entry = _stack[index];
        entry.PanelInterface?.OnClose();

        if (entry.Transition != null)
        {
            entry.Transition.PlayClose(entry.Panel, () => RemoveEntry(entry, index));
        }
        else
        {
            RemoveEntry(entry, index);
        }

        // 通知新的栈顶面板恢复
        if (_stack.Count > 0)
        {
            var newTop = _stack[^1];
            newTop.PanelInterface?.OnUncover();
            newTop.PanelInterface?.OnResume();

            // 恢复栈顶输入
            newTop.Panel.MouseFilter = Control.MouseFilterEnum.Stop;
        }

        _eventBus?.Emit("ui_closed", new UIEventArg { UIName = uiName });
        _logService?.Info($"UI closed: {uiName}");
    }

    /// <summary>关闭栈顶 UI 面板。</summary>
    public void CloseTopUI()
    {
        if (_stack.Count > 0)
            CloseUI(_stack[^1].Name);
    }

    /// <summary>关闭所有 UI 面板。</summary>
    public void CloseAllUI()
    {
        while (_stack.Count > 0)
            CloseUI(_stack[^1].Name);
    }

    /// <summary>获取已打开的 UI 面板接口。</summary>
    public IUIPanel GetUI(string uiName)
    {
        var entry = _stack.Find(e => e.Name == uiName);
        return entry?.PanelInterface;
    }

    /// <summary>检查指定 UI 是否已打开。</summary>
    public bool IsUIOpen(string uiName) => _stack.Exists(e => e.Name == uiName);

    /// <summary>获取 UI 栈深度。</summary>
    public int StackDepth => _stack.Count;

    private void RemoveEntry(UIStackEntry entry, int index)
    {
        _stack.RemoveAt(index);
        entry.Panel.QueueFree();
    }
}
