using System;
using System.Collections.Generic;
using System.Linq;

namespace LynxFramework.Core;

/// <summary>
/// 模块管理器：负责模块注册、查询、生命周期管理。
/// </summary>
public class ModuleManager
{
    private readonly Dictionary<Type, IModule> _modules = new();
    private List<IModule> _sortedModules;

    /// <summary>注册模块。同一类型不可重复注册。</summary>
    public void RegisterModule(IModule module)
    {
        var type = module.GetType();
        if (_modules.ContainsKey(type))
            throw new InvalidOperationException($"Module {type.Name} already registered.");

        _modules[type] = module;
        _sortedModules = null; // 标记需要重新排序
    }

    /// <summary>注销模块。</summary>
    public void UnregisterModule<T>() where T : IModule
    {
        _modules.Remove(typeof(T));
        _sortedModules = null;
    }

    /// <summary>获取指定类型的模块实例。</summary>
    public T GetModule<T>() where T : IModule
    {
        return _modules.TryGetValue(typeof(T), out var module) ? (T)module : default;
    }

    /// <summary>检查指定类型的模块是否已注册。</summary>
    public bool HasModule<T>() where T : IModule => _modules.ContainsKey(typeof(T));

    /// <summary>按优先级升序初始化所有模块。</summary>
    public void InitAll()
    {
        EnsureSorted();
        foreach (var module in _sortedModules)
            module.OnInit();
    }

    /// <summary>按优先级降序关闭所有模块。</summary>
    public void ShutdownAll()
    {
        EnsureSorted();
        for (int i = _sortedModules.Count - 1; i >= 0; i--)
            _sortedModules[i].OnShutdown();
    }

    /// <summary>获取所有已排序的模块列表。</summary>
    public IReadOnlyList<IModule> GetAllModules()
    {
        EnsureSorted();
        return _sortedModules;
    }

    private void EnsureSorted()
    {
        _sortedModules ??= _modules.Values.OrderBy(m => m.Priority).ToList();
    }
}
