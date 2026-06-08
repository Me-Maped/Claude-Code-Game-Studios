using System;
using System.Collections.Generic;

namespace LynxFramework.Pool;

/// <summary>
/// 泛型对象池。管理普通 C# 对象的复用，降低 GC 压力。
/// 约束 T : class, IPoolableObject, new()，确保可实例化和可复用。
/// </summary>
public sealed class ObjectPool<T> where T : class, IPoolableObject, new()
{
    private readonly Stack<T> _available = new();
    private readonly HashSet<T> _active = new();
    private readonly Func<T> _factory;

    public ObjectPool(Func<T> factory = null, int preloadCount = 0)
    {
        _factory = factory ?? (() => new T());
        for (int i = 0; i < preloadCount; i++)
            _available.Push(_factory());
    }

    /// <summary>从池中获取对象。池空时通过工厂创建新实例。</summary>
    public T Get()
    {
        var obj = _available.Count > 0 ? _available.Pop() : _factory();
        _active.Add(obj);
        obj.OnGet();
        return obj;
    }

    /// <summary>回收对象到池中。</summary>
    public void Recycle(T obj)
    {
        if (!_active.Remove(obj)) return;
        obj.OnRecycle();
        _available.Push(obj);
    }

    public int ActiveCount => _active.Count;
    public int AvailableCount => _available.Count;
}
