using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Pool;
using LynxFramework.Resource;

namespace LynxFramework.Scene;

/// <summary>
/// 场景管理模块。
/// 负责异步加载/卸载场景、场景栈管理、转场动画。
/// </summary>
public class SceneModule : IModule
{
    private ResourceService _resourceService;
    private PoolManager _poolManager;
    private EventBus _eventBus;
    private LogService _logService;
    private Node _sceneContainer;
    private readonly Stack<string> _sceneStack = new();
    private string _currentScenePath;

    public int Priority => 120;

    public void OnInit()
    {
        _resourceService = FrameworkEntry.Instance.GetModule<ResourceService>();
        _poolManager = FrameworkEntry.Instance.GetModule<PoolManager>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();

        _sceneContainer = new Node { Name = "SceneContainer" };
        FrameworkEntry.Instance.AddChild(_sceneContainer);
    }

    public void OnShutdown()
    {
        foreach (var child in _sceneContainer.GetChildren())
            child.QueueFree();
        _sceneContainer.QueueFree();
    }

    /// <summary>异步加载场景并添加到场景容器。</summary>
    public async void LoadSceneAsync(string scenePath, Action<float> progress = null)
    {
        _logService?.Info($"Loading scene: {scenePath}");

        var request = _resourceService.LoadAsync(scenePath);

        // 轮询进度
        while (!request.IsCompleted)
        {
            progress?.Invoke(request.Progress);
            await FrameworkEntry.Instance.ToSignal(
                Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame);
        }

        if (request.Result is PackedScene scene)
        {
            var instance = scene.Instantiate();
            _sceneContainer.AddChild(instance);
            _currentScenePath = scenePath;
            _eventBus?.Emit("scene_loaded", new SceneEventArg { ScenePath = scenePath });
            _logService?.Info($"Scene loaded: {scenePath}");
        }
        else
        {
            _logService?.Error($"Failed to load scene: {scenePath}");
        }
    }

    /// <summary>卸载指定路径的场景。</summary>
    public void UnloadScene(string scenePath)
    {
        foreach (var child in _sceneContainer.GetChildren())
        {
            if (child.SceneFilePath == scenePath)
            {
                child.QueueFree();
                _eventBus?.Emit("scene_unloaded", new SceneEventArg { ScenePath = scenePath });
                _logService?.Info($"Scene unloaded: {scenePath}");
                return;
            }
        }
    }

    /// <summary>切换场景（带转场动画）。当前场景入栈。</summary>
    public async void SwitchToScene(string scenePath, ISceneTransition transition = null)
    {
        // 1. 播放转场出场动画
        if (transition != null)
            await PlayTransitionOut(transition);

        // 2. 卸载当前场景
        if (!string.IsNullOrEmpty(_currentScenePath))
        {
            _sceneStack.Push(_currentScenePath);
            UnloadScene(_currentScenePath);
        }

        // 3. 加载新场景
        LoadSceneAsync(scenePath);

        // 4. 播放转场入场动画
        if (transition != null)
            await PlayTransitionIn(transition);
    }

    /// <summary>返回上一个场景（从栈中弹出）。</summary>
    public async void GoBack(ISceneTransition transition = null)
    {
        if (_sceneStack.Count == 0)
        {
            _logService?.Warning("Scene stack is empty, cannot go back.");
            return;
        }

        var previousPath = _sceneStack.Pop();

        if (transition != null)
            await PlayTransitionOut(transition);

        UnloadScene(_currentScenePath);
        LoadSceneAsync(previousPath);

        if (transition != null)
            await PlayTransitionIn(transition);
    }

    /// <summary>获取当前场景节点。</summary>
    public Node GetCurrentScene()
        => _sceneContainer.GetChildCount() > 0 ? _sceneContainer.GetChild(0) : null;

    /// <summary>获取当前场景路径。</summary>
    public string GetCurrentScenePath() => _currentScenePath;

    /// <summary>获取场景栈深度。</summary>
    public int StackDepth => _sceneStack.Count;

    private async Task PlayTransitionOut(ISceneTransition transition)
    {
        var tcs = new TaskCompletionSource<bool>();
        transition.PlayOut(() => tcs.SetResult(true));
        await tcs.Task;
    }

    private async Task PlayTransitionIn(ISceneTransition transition)
    {
        var tcs = new TaskCompletionSource<bool>();
        transition.PlayIn(() => tcs.SetResult(true));
        await tcs.Task;
    }
}
