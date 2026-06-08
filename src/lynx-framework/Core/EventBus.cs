using System;
using System.Collections.Generic;

namespace LynxFramework.Core;

/// <summary>
/// 高性能全局事件总线。
/// 类型安全载荷通过泛型约束 Action&lt;T&gt; where T : EventArg 实现。
/// Emit 过程中禁止直接修改回调列表，通过延迟队列处理。
/// </summary>
public class EventBus : IModule
{
    private readonly Dictionary<string, List<Delegate>> _handlers = new();
    private readonly Dictionary<string, List<Delegate>> _pendingAdd = new();
    private readonly HashSet<(string eventId, Delegate handler)> _pendingRemove = new();
    private bool _isEmitting;

    public int Priority => 0;

    public void OnInit() { }
    public void OnShutdown() => Clear();

    // --- 订阅 ---

    /// <summary>订阅无参事件。</summary>
    public void Subscribe(string eventId, Action callback)
        => AddHandler(eventId, callback);

    /// <summary>订阅带类型安全载荷的事件。</summary>
    public void Subscribe<T>(string eventId, Action<T> callback) where T : EventArg
        => AddHandler(eventId, callback);

    // --- 取消订阅 ---

    /// <summary>取消订阅无参事件。</summary>
    public void Unsubscribe(string eventId, Action callback)
        => RemoveHandler(eventId, callback);

    /// <summary>取消订阅带类型安全载荷的事件。</summary>
    public void Unsubscribe<T>(string eventId, Action<T> callback) where T : EventArg
        => RemoveHandler(eventId, callback);

    // --- 发射 ---

    /// <summary>发射无参事件。</summary>
    public void Emit(string eventId)
    {
        BeginEmit();
        try
        {
            if (_handlers.TryGetValue(eventId, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                    ((Action)list[i])();
            }
        }
        finally { EndEmit(); }
    }

    /// <summary>发射带类型安全载荷的事件。</summary>
    public void Emit<T>(string eventId, T arg) where T : EventArg
    {
        BeginEmit();
        try
        {
            if (_handlers.TryGetValue(eventId, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                    ((Action<T>)list[i])(arg);
            }
        }
        finally { EndEmit(); }
    }

    /// <summary>清除所有事件订阅。</summary>
    public void Clear()
    {
        _handlers.Clear();
        _pendingAdd.Clear();
        _pendingRemove.Clear();
    }

    /// <summary>清除指定事件的所有订阅。</summary>
    public void Clear(string eventId)
    {
        _handlers.Remove(eventId);
        _pendingAdd.Remove(eventId);
        _pendingRemove.RemoveWhere(t => t.eventId == eventId);
    }

    // --- 内部 ---

    private void AddHandler(string eventId, Delegate handler)
    {
        if (_isEmitting)
        {
            if (!_pendingAdd.TryGetValue(eventId, out var list))
            {
                list = new List<Delegate>();
                _pendingAdd[eventId] = list;
            }
            list.Add(handler);
        }
        else
        {
            if (!_handlers.TryGetValue(eventId, out var list))
            {
                list = new List<Delegate>();
                _handlers[eventId] = list;
            }
            list.Add(handler);
        }
    }

    private void RemoveHandler(string eventId, Delegate handler)
    {
        if (_isEmitting)
        {
            // 从待添加队列中移除（如果存在）
            if (_pendingAdd.TryGetValue(eventId, out var pendingList))
                pendingList.Remove(handler);
            // 加入待移除队列，防止 handlers 中已有该回调
            _pendingRemove.Add((eventId, handler));
        }
        else if (_handlers.TryGetValue(eventId, out var list))
        {
            list.Remove(handler);
        }
    }

    private void BeginEmit() => _isEmitting = true;

    private void EndEmit()
    {
        _isEmitting = false;
        // 处理延迟添加
        foreach (var kv in _pendingAdd)
        {
            if (!_handlers.TryGetValue(kv.Key, out var list))
            {
                list = new List<Delegate>();
                _handlers[kv.Key] = list;
            }
            list.AddRange(kv.Value);
        }
        _pendingAdd.Clear();
        // 处理延迟移除
        foreach (var (eventId, handler) in _pendingRemove)
        {
            if (_handlers.TryGetValue(eventId, out var list))
                list.Remove(handler);
        }
        _pendingRemove.Clear();
    }
}
