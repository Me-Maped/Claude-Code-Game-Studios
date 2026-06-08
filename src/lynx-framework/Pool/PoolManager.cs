using System;
using System.Collections.Generic;
using Godot;

namespace LynxFramework.Pool;

/// <summary>
/// 对象池管理器。统一管理节点池与普通对象池。
/// 提供池化分配/回收，降低 GC 压力。
/// </summary>
public class PoolManager : IModule
{
    private readonly Dictionary<string, NodePool> _nodePools = new();
    private readonly Dictionary<Type, object> _objectPools = new();

    public int Priority => 30;
    public void OnInit() { }

    public void OnShutdown()
    {
        _nodePools.Clear();
        _objectPools.Clear();
    }

    // --- 节点池 ---

    /// <summary>创建节点池。</summary>
    /// <param name="poolKey">池的唯一标识</param>
    /// <param name="scene">节点场景</param>
    /// <param name="preloadCount">预热数量</param>
    public void CreatePool(string poolKey, PackedScene scene, int preloadCount = 0)
        => _nodePools[poolKey] = new NodePool(scene, preloadCount);

    /// <summary>从节点池获取节点。</summary>
    public Node GetNode(string poolKey, Node parent = null)
    {
        if (!_nodePools.TryGetValue(poolKey, out var pool)) return null;
        var node = pool.Get(parent);
        // 记录 pool_key 元数据，用于回收时定位所属池
        node.SetMeta("pool_key", poolKey);
        return node;
    }

    /// <summary>回收节点到所属池。通过 pool_key 元数据自动定位。</summary>
    public void RecycleNode(Node node)
    {
        if (node.HasMeta("pool_key"))
        {
            var key = node.GetMeta("pool_key").AsString();
            if (_nodePools.TryGetValue(key, out var pool))
                pool.Recycle(node);
        }
    }

    /// <summary>检查指定池是否存在。</summary>
    public bool HasPool(string poolKey) => _nodePools.ContainsKey(poolKey);

    /// <summary>移除指定节点池。</summary>
    public void RemovePool(string poolKey) => _nodePools.Remove(poolKey);

    // --- 普通对象池 ---

    /// <summary>创建普通对象池。</summary>
    public void CreateObjectPool<T>(Func<T> factory = null, int preloadCount = 0) where T : class, IPoolableObject, new()
        => _objectPools[typeof(T)] = new ObjectPool<T>(factory, preloadCount);

    /// <summary>从对象池获取对象。</summary>
    public T GetObject<T>() where T : class, IPoolableObject, new()
        => _objectPools.TryGetValue(typeof(T), out var pool) ? ((ObjectPool<T>)pool).Get() : default;

    /// <summary>回收对象到对象池。</summary>
    public void RecycleObject<T>(T obj) where T : class, IPoolableObject, new()
    {
        if (_objectPools.TryGetValue(typeof(T), out var pool))
            ((ObjectPool<T>)pool).Recycle(obj);
    }

    /// <summary>检查指定对象池是否存在。</summary>
    public bool HasObjectPool<T>() where T : class, IPoolableObject, new()
        => _objectPools.ContainsKey(typeof(T));

    // --- 统计 ---

    /// <summary>获取所有节点池的统计信息。</summary>
    public Dictionary<string, PoolStat> GetPoolStats()
    {
        var stats = new Dictionary<string, PoolStat>();
        foreach (var kv in _nodePools)
        {
            stats[kv.Key] = new PoolStat
            {
                PoolKey = kv.Key,
                ActiveCount = kv.Value.ActiveCount,
                AvailableCount = kv.Value.AvailableCount,
                HitCount = kv.Value.HitCount,
                MissCount = kv.Value.MissCount,
                HitRate = kv.Value.HitRate
            };
        }
        return stats;
    }
}
