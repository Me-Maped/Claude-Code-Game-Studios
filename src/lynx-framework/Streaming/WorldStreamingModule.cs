using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using LynxFramework.Core;
using LynxFramework.Entity;
using LynxFramework.Log;
using LynxFramework.Pool;
using LynxFramework.Resource;

namespace LynxFramework.Streaming;

/// <summary>
/// 世界流式加载模块。
/// 根据观察者位置动态加载/卸载世界区块。
/// </summary>
public class WorldStreamingModule : IModule
{
    private ResourceService _resourceService;
    private PoolManager _poolManager;
    private EntityModule _entityModule;
    private EventBus _eventBus;
    private LogService _logService;
    private IChunkStrategy _strategy;
    private readonly Dictionary<string, Node> _loadedAreas = new();
    private readonly HashSet<string> _loadingAreas = new();

    public int Priority => 160;

    public void OnInit()
    {
        _resourceService = FrameworkEntry.Instance.GetModule<ResourceService>();
        _poolManager = FrameworkEntry.Instance.GetModule<PoolManager>();
        _entityModule = FrameworkEntry.Instance.GetModule<EntityModule>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
    }

    public void OnShutdown()
    {
        foreach (var area in _loadedAreas.Values)
            area.QueueFree();
        _loadedAreas.Clear();
    }

    /// <summary>设置区块加载策略。</summary>
    public void SetChunkStrategy(IChunkStrategy strategy) => _strategy = strategy;

    /// <summary>异步加载指定区块。</summary>
    public async void LoadArea(string areaId, Node parent)
    {
        if (_loadedAreas.ContainsKey(areaId) || _loadingAreas.Contains(areaId)) return;
        _loadingAreas.Add(areaId);

        var scenePath = $"res://world/areas/{areaId}.tscn";
        var request = _resourceService.LoadAsync(scenePath);
        while (!request.IsCompleted)
            await FrameworkEntry.Instance.ToSignal(
                Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame);

        if (request.Result is PackedScene scene)
        {
            var instance = scene.Instantiate();
            parent?.AddChild(instance);
            _loadedAreas[areaId] = instance;
            _logService?.Info($"Area loaded: {areaId}");
        }
        else
        {
            _logService?.Warning($"Area not found: {scenePath}");
        }

        _loadingAreas.Remove(areaId);
    }

    /// <summary>卸载指定区块。</summary>
    public void UnloadArea(string areaId)
    {
        if (!_loadedAreas.TryGetValue(areaId, out var node)) return;
        _entityModule?.RecycleEntitiesInNode(node);
        node.QueueFree();
        _loadedAreas.Remove(areaId);
        _logService?.Info($"Area unloaded: {areaId}");
    }

    /// <summary>检查区块是否已加载。</summary>
    public bool IsAreaLoaded(string areaId) => _loadedAreas.ContainsKey(areaId);

    /// <summary>根据观察者位置更新区块加载/卸载。</summary>
    public void UpdateViewerPosition(Vector3 position)
    {
        if (_strategy == null) return;

        var toLoad = _strategy.GetChunksToLoad(position, 100f);
        foreach (var chunkId in toLoad)
            LoadArea(chunkId, FrameworkEntry.Instance);

        var toUnload = _strategy.GetChunksToUnload(position, 150f);
        foreach (var chunkId in toUnload)
            UnloadArea(chunkId);
    }

    /// <summary>获取已加载区块数量。</summary>
    public int LoadedAreaCount => _loadedAreas.Count;
}
