using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;

namespace LynxFramework.Input;

/// <summary>
/// 输入缓冲模块。
/// 在指定时间窗口内缓冲玩家输入，解决动作游戏的输入宽容度问题。
/// </summary>
public class InputBufferModule : IModule
{
    private InputModule _inputModule;
    private EventBus _eventBus;
    private LogService _logService;
    private UpdateDriver _updateDriver;
    private readonly List<BufferedAction> _buffer = new();
    private UpdateEntry _updateEntry;

    public int Priority => 90;

    public void OnInit()
    {
        _inputModule = FrameworkEntry.Instance.GetModule<InputModule>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
        _updateDriver = FrameworkEntry.Instance.GetModule<UpdateDriver>();
        _updateEntry = _updateDriver.Register(UpdatePhase.Process, 100, Update);
    }

    public void OnShutdown()
    {
        if (_updateEntry != null)
            _updateDriver.Unregister(_updateEntry);
    }

    /// <summary>缓冲指定动作。在 maxTimeSec 时间窗口内可被消费。</summary>
    public void BufferAction(string actionName, double maxTimeSec = 0.2)
    {
        _buffer.Add(new BufferedAction
        {
            ActionName = actionName,
            Timestamp = Time.GetTicksMsec() / 1000.0,
            MaxTimeSec = maxTimeSec,
            IsConsumed = false
        });
    }

    /// <summary>消费缓冲动作。成功消费返回 true。</summary>
    public bool ConsumeAction(string actionName)
    {
        var now = Time.GetTicksMsec() / 1000.0;
        for (int i = _buffer.Count - 1; i >= 0; i--)
        {
            var action = _buffer[i];
            if (action.ActionName == actionName && !action.IsConsumed)
            {
                if (now - action.Timestamp <= action.MaxTimeSec)
                {
                    // 标记为已消费（通过替换 struct）
                    _buffer[i] = new BufferedAction
                    {
                        ActionName = action.ActionName,
                        Timestamp = action.Timestamp,
                        MaxTimeSec = action.MaxTimeSec,
                        IsConsumed = true
                    };
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>检查指定动作是否在缓冲中（未过期且未消费）。</summary>
    public bool IsActionBuffered(string actionName)
    {
        var now = Time.GetTicksMsec() / 1000.0;
        foreach (var action in _buffer)
        {
            if (action.ActionName == actionName && !action.IsConsumed &&
                now - action.Timestamp <= action.MaxTimeSec)
                return true;
        }
        return false;
    }

    /// <summary>清除所有缓冲。</summary>
    public void Clear() => _buffer.Clear();

    private void Update(float delta)
    {
        var now = Time.GetTicksMsec() / 1000.0;
        _buffer.RemoveAll(a => a.IsConsumed || now - a.Timestamp > a.MaxTimeSec);
    }
}
