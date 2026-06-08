using System;
using System.Collections.Generic;
using LynxFramework.Core;
using LynxFramework.Log;

namespace LynxFramework.Procedure;

/// <summary>
/// 流程管理模块。有限状态机，管理游戏流程切换。
/// 通过 RegisterProcedure 注册流程工厂，SetCurrentProcedure 切换流程。
/// </summary>
public class ProcedureModule : IModule
{
    private EventBus _eventBus;
    private LogService _logService;
    private UpdateDriver _updateDriver;
    private IProcedure _current;
    private string _currentProcedureName;
    private readonly Dictionary<string, Func<IProcedure>> _procedures = new();
    private UpdateEntry _updateEntry;

    public int Priority => 170;

    public void OnInit()
    {
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
        _updateDriver = FrameworkEntry.Instance.GetModule<UpdateDriver>();
        _updateEntry = _updateDriver.Register(UpdatePhase.Process, 0, OnUpdate);
    }

    public void OnShutdown()
    {
        _current?.OnLeave();
        _current = null;
        if (_updateEntry != null)
            _updateDriver.Unregister(_updateEntry);
    }

    /// <summary>注册流程工厂。</summary>
    public void RegisterProcedure(string name, Func<IProcedure> factory)
        => _procedures[name] = factory;

    /// <summary>切换到指定流程。</summary>
    public void SetCurrentProcedure(string procName)
    {
        if (!_procedures.TryGetValue(procName, out var factory))
        {
            _logService?.Warning($"Procedure not registered: {procName}");
            return;
        }

        _current?.OnLeave();
        _current = factory();
        _currentProcedureName = procName;
        _current.OnEnter();

        _eventBus?.Emit("procedure_changed", new ProcedureEventArg
        {
            ProcedureName = procName
        });
        _logService?.Info($"Procedure changed: {procName}");
    }

    /// <summary>获取当前流程实例。</summary>
    public IProcedure GetCurrentProcedure() => _current;

    /// <summary>获取当前流程名称。</summary>
    public string GetCurrentProcedureName() => _currentProcedureName;

    /// <summary>检查指定流程是否已注册。</summary>
    public bool HasProcedure(string name) => _procedures.ContainsKey(name);

    private void OnUpdate(float delta) => _current?.OnUpdate(delta);
}
