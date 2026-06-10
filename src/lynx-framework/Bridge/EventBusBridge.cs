using System;
using System.Collections.Generic;
using Godot;
using LynxFramework.Core;

namespace LynxFramework.Bridge;

/// <summary>
/// EventBus 的 GDScript 桥接层。
/// 将 Godot Callable 包装为 C# Action，支持从 GDScript 订阅/发射事件。
/// </summary>
public static class EventBusBridge
{
    private static readonly Dictionary<int, (string eventId, Delegate handler)> _handlers = new();
    private static int _nextId = 1;

    /// <summary>从 GDScript 订阅无参事件。返回 handler ID。</summary>
    public static int Subscribe(EventBus bus, string eventId, Callable callback)
    {
        var id = _nextId++;
        Action handler = () => callback.Call();
        bus.Subscribe(eventId, handler);
        _handlers[id] = (eventId, handler);
        return id;
    }

    /// <summary>从 GDScript 订阅带 Variant 载荷的事件。返回 handler ID。</summary>
    public static int SubscribeVariant(EventBus bus, string eventId, Callable callback)
    {
        var id = _nextId++;
        Action<VariantEventArg> handler = (arg) => callback.Call(arg.Data);
        bus.Subscribe(eventId, handler);
        _handlers[id] = (eventId, handler);
        return id;
    }

    /// <summary>取消订阅。</summary>
    public static void Unsubscribe(EventBus bus, string eventId, int handlerId)
    {
        if (!_handlers.TryGetValue(handlerId, out var entry)) return;

        if (entry.handler is Action action)
            bus.Unsubscribe(eventId, action);
        else if (entry.handler is Action<VariantEventArg> variantAction)
            bus.Unsubscribe(eventId, variantAction);

        _handlers.Remove(handlerId);
    }
}

/// <summary>
/// Variant 包装事件载荷。用于 GDScript 传递任意数据。
/// </summary>
public partial class VariantEventArg : EventArg
{
    public Variant Data { get; set; }
}
