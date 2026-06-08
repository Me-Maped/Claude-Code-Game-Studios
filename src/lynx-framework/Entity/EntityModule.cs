using System;
using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Pool;

namespace LynxFramework.Entity;

/// <summary>
/// 实体管理模块。
/// 负责实体的生成、回收、分组管理。优先使用对象池复用。
/// </summary>
public class EntityModule : IModule
{
    private PoolManager _poolManager;
    private EventBus _eventBus;
    private LogService _logService;
    private readonly Dictionary<string, List<Node>> _entityGroups = new();
    private readonly HashSet<Node> _activeEntities = new();

    public int Priority => 150;

    public void OnInit()
    {
        _poolManager = FrameworkEntry.Instance.GetModule<PoolManager>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
    }

    public void OnShutdown()
    {
        foreach (var entity in _activeEntities)
            entity.QueueFree();
        _activeEntities.Clear();
        _entityGroups.Clear();
    }

    /// <summary>生成实体。优先从对象池获取，兜底直接实例化。</summary>
    public Node SpawnEntity(string entityName, Node parent, Vector3 position = default)
    {
        // 优先从对象池获取
        Node entity = null;
        if (_poolManager.HasPool(entityName))
            entity = _poolManager.GetNode(entityName, parent);

        // 兜底直接实例化
        if (entity == null)
        {
            var scenePath = $"res://entities/{entityName}.tscn";
            var scene = GD.Load<PackedScene>(scenePath);
            if (scene == null)
            {
                _logService?.Warning($"Entity scene not found: {scenePath}");
                return null;
            }
            entity = scene.Instantiate();
            parent?.AddChild(entity);
        }

        // 设置位置
        if (entity is Node2D node2D)
            node2D.Position = new Vector2(position.X, position.Y);
        else if (entity is Node3D node3D)
            node3D.Position = position;

        // 初始化
        if (entity is IEntity iEntity)
        {
            var data = new EntityInitData { EntityName = entityName, Position = position };
            iEntity.OnInit(data);
        }

        _activeEntities.Add(entity);

        // 分组
        if (!string.IsNullOrEmpty(entityName))
        {
            if (!_entityGroups.ContainsKey(entityName))
                _entityGroups[entityName] = new List<Node>();
            _entityGroups[entityName].Add(entity);
        }

        _eventBus?.Emit("entity_spawned", new EntityEventArg { Entity = entity, EntityName = entityName });
        _logService?.Debug($"Entity spawned: {entityName}");
        return entity;
    }

    /// <summary>销毁实体。回收到对象池或直接释放。</summary>
    public void DestroyEntity(Node entity)
    {
        if (!_activeEntities.Remove(entity)) return;

        if (entity is IEntity iEntity)
        {
            iEntity.OnDeinit();
        }

        // 从分组中移除
        foreach (var group in _entityGroups.Values)
            group.Remove(entity);

        // 尝试回收到对象池
        if (_poolManager.HasPool(entity.Name))
            _poolManager.RecycleNode(entity);
        else
            entity.QueueFree();

        _eventBus?.Emit("entity_destroyed", new EntityEventArg { EntityName = entity.Name });
        _logService?.Debug($"Entity destroyed: {entity.Name}");
    }

    /// <summary>获取指定分组的所有实体。</summary>
    public IReadOnlyList<Node> GetEntitiesByGroup(string groupName)
        => _entityGroups.TryGetValue(groupName, out var list) ? list : (IReadOnlyList<Node>)Array.Empty<Node>();

    /// <summary>回收容器节点下的所有活跃实体。</summary>
    public void RecycleEntitiesInNode(Node container)
    {
        var children = new List<Node>(container.GetChildren());
        foreach (var child in children)
        {
            if (_activeEntities.Contains(child))
                DestroyEntity(child);
        }
    }

    /// <summary>获取活跃实体数量。</summary>
    public int ActiveEntityCount => _activeEntities.Count;
}
