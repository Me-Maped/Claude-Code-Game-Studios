using System;
using System.Collections.Generic;
using Godot;
using LynxFramework.Core;

namespace LynxFramework.Resource;

/// <summary>
/// 统一资源加载服务。
/// 支持同步/异步加载、弱引用缓存、热更 .pck 覆盖。
/// </summary>
public class ResourceService : IModule
{
    private readonly Dictionary<string, WeakReference<Godot.Resource>> _cache = new();
    private readonly List<string> _patchPacks = new();
    private UpdateDriver _updateDriver;

    public int Priority => 20;

    public void OnInit()
    {
        _updateDriver = FrameworkEntry.Instance.GetModule<UpdateDriver>();
    }

    public void OnShutdown() => ClearAllCache();

    // --- 异步加载 ---

    /// <summary>异步加载资源。返回 ResourceRequest 用于跟踪进度。</summary>
    public ResourceRequest LoadAsync(string path, string typeHint = null, int priority = 0)
    {
        var request = new ResourceRequest(path);

        // 检查缓存
        if (TryGetCached(path, out var cached))
        {
            request.SetResult(cached);
            return request;
        }

        // 使用 Godot ResourceLoader 异步加载
        var error = ResourceLoader.LoadThreadedRequest(path, typeHint);
        if (error != Error.Ok)
        {
            request.SetResult(null);
            return request;
        }

        // 轮询加载状态
        PollLoadRequest(request);
        return request;
    }

    // --- 同步加载 ---

    /// <summary>同步加载资源（带缓存）。优先从缓存获取。</summary>
    public T LoadCached<T>(string path) where T : Godot.Resource
    {
        if (TryGetCached(path, out var cached) && cached is T t)
            return t;

        var resource = GD.Load<T>(path);
        if (resource != null)
            _cache[path] = new WeakReference<Godot.Resource>(resource);
        return resource;
    }

    /// <summary>同步加载资源（无缓存）。</summary>
    public T Load<T>(string path) where T : Godot.Resource
    {
        return GD.Load<T>(path);
    }

    // --- .pck 热更 ---

    /// <summary>挂载热更资源包。加载的资源将覆盖同路径的原始资源。</summary>
    public bool SetPatchPack(string packPath)
    {
        if (ProjectSettings.LoadResourcePack(packPath))
        {
            _patchPacks.Add(packPath);
            return true;
        }
        return false;
    }

    // --- 缓存管理 ---

    /// <summary>强制重新加载指定路径的缓存资源。</summary>
    public void ReloadCached(string path)
    {
        _cache.Remove(path);
        ResourceLoader.LoadThreadedRequest(path);
    }

    /// <summary>清除指定前缀的缓存。</summary>
    public void ClearCacheWithKeyPrefix(string prefix)
    {
        var keysToRemove = new List<string>();
        foreach (var key in _cache.Keys)
        {
            if (key.StartsWith(prefix))
                keysToRemove.Add(key);
        }
        foreach (var key in keysToRemove)
            _cache.Remove(key);
    }

    /// <summary>清除所有缓存。</summary>
    public void ClearAllCache() => _cache.Clear();

    /// <summary>获取缓存中的资源数量。</summary>
    public int CacheCount => _cache.Count;

    // --- 内部 ---

    private bool TryGetCached(string path, out Godot.Resource resource)
    {
        resource = null;
        if (!_cache.TryGetValue(path, out var weakRef)) return false;
        if (!weakRef.TryGetTarget(out resource))
        {
            _cache.Remove(path);
            return false;
        }
        return true;
    }

    private void PollLoadRequest(ResourceRequest request)
    {
        // 通过 UpdateDriver 注册轮询回调
        var progressArray = new Godot.Collections.Array();
        UpdateEntry entry = null;
        entry = _updateDriver.Register(UpdatePhase.Process, 999, (delta) =>
        {
            var status = ResourceLoader.LoadThreadedGetStatus(request.Path, progressArray);
            switch (status)
            {
                case ResourceLoader.ThreadLoadStatus.InProgress:
                    request.SetProgress(progressArray.Count > 0 ? (float)progressArray[0] : 0f);
                    break;
                case ResourceLoader.ThreadLoadStatus.Loaded:
                    var res = ResourceLoader.LoadThreadedGet(request.Path);
                    _cache[request.Path] = new WeakReference<Godot.Resource>(res);
                    request.SetResult(res);
                    _updateDriver.Unregister(entry);
                    break;
                default:
                    request.SetResult(null);
                    _updateDriver.Unregister(entry);
                    break;
            }
        });
    }
}
