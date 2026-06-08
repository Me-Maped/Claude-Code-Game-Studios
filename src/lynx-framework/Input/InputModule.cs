using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;

namespace LynxFramework.Input;

/// <summary>
/// 输入管理模块。
/// 封装 Godot Input API，支持动作启用/禁用和键位重映射。
/// </summary>
public class InputModule : IModule
{
    private EventBus _eventBus;
    private LogService _logService;
    private readonly HashSet<string> _disabledActions = new();

    public int Priority => 80;

    public void OnInit()
    {
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
    }

    public void OnShutdown() => _disabledActions.Clear();

    /// <summary>启用指定输入动作。</summary>
    public void EnableAction(string name) => _disabledActions.Remove(name);

    /// <summary>禁用指定输入动作。</summary>
    public void DisableAction(string name) => _disabledActions.Add(name);

    /// <summary>检查指定动作是否启用。</summary>
    public bool IsActionEnabled(string name) => !_disabledActions.Contains(name);

    /// <summary>检查动作是否正在按下。</summary>
    public bool IsActionPressed(string name)
        => IsActionEnabled(name) && Godot.Input.IsActionPressed(name);

    /// <summary>检查动作是否刚刚按下。</summary>
    public bool IsActionJustPressed(string name)
        => IsActionEnabled(name) && Godot.Input.IsActionJustPressed(name);

    /// <summary>检查动作是否刚刚释放。</summary>
    public bool IsActionJustReleased(string name)
        => IsActionEnabled(name) && Godot.Input.IsActionJustReleased(name);

    /// <summary>获取方向向量。</summary>
    public Vector2 GetVector(string negativeX, string positiveX, string negativeY, string positiveY)
    {
        if (_disabledActions.Contains(negativeX) || _disabledActions.Contains(positiveX) ||
            _disabledActions.Contains(negativeY) || _disabledActions.Contains(positiveY))
            return Vector2.Zero;
        return Godot.Input.GetVector(negativeX, positiveX, negativeY, positiveY);
    }

    /// <summary>重映射输入动作。</summary>
    public void RemapAction(string action, InputEvent inputEvent)
    {
        Godot.InputMap.ActionEraseEvents(action);
        Godot.InputMap.ActionAddEvent(action, inputEvent);

        _eventBus?.Emit("input_remap", new InputRemapEventArg
        {
            Action = action,
            Event = inputEvent
        });
        _logService?.Info($"Input remapped: {action}");
    }

    /// <summary>禁用所有输入。</summary>
    public void DisableAll()
    {
        // 遍历所有已注册的 InputMap 动作并禁用
        foreach (var action in Godot.InputMap.GetActions())
            _disabledActions.Add(action.ToString());
    }

    /// <summary>启用所有输入。</summary>
    public void EnableAll() => _disabledActions.Clear();
}
