using System.Collections.Generic;
using Godot;

namespace LynxFramework.Pool;

/// <summary>
/// 节点池。管理同一 PackedScene 的节点复用。
/// 通过 pool_key 元数据关联回收，避免维护反向映射表。
/// </summary>
public sealed class NodePool
{
    private readonly PackedScene _scene;
    private readonly List<Node> _available = new();
    private readonly HashSet<Node> _active = new();
    private long _hitCount;
    private long _missCount;

    public NodePool(PackedScene scene, int preloadCount)
    {
        _scene = scene;
        Prewarm(preloadCount);
    }

    /// <summary>从池中获取节点。池空时自动实例化新节点。</summary>
    public Node Get(Node parent)
    {
        Node node;
        if (_available.Count > 0)
        {
            node = _available[_available.Count - 1];
            _available.RemoveAt(_available.Count - 1);
            _hitCount++;
        }
        else
        {
            node = _scene.Instantiate();
            _missCount++;
        }

        _active.Add(node);
        parent?.AddChild(node);

        if (node is IPoolableObject poolable)
            poolable.OnGet();

        return node;
    }

    /// <summary>回收节点到池中。</summary>
    public void Recycle(Node node)
    {
        if (!_active.Remove(node)) return;

        if (node is IPoolableObject poolable)
            poolable.OnRecycle();

        node.GetParent()?.RemoveChild(node);
        _available.Add(node);
    }

    /// <summary>预热：预先创建指定数量的节点。</summary>
    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var node = _scene.Instantiate();
            _available.Add(node);
        }
    }

    public int ActiveCount => _active.Count;
    public int AvailableCount => _available.Count;
    public long HitCount => _hitCount;
    public long MissCount => _missCount;
    public float HitRate => (_hitCount + _missCount) == 0 ? 0 : (float)_hitCount / (_hitCount + _missCount);
}
