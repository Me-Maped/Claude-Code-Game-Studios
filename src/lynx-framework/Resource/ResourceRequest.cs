using System;
using Godot;

namespace LynxFramework.Resource;

/// <summary>
/// 资源加载请求。跟踪异步加载状态和进度。
/// </summary>
public class ResourceRequest
{
    public string Path { get; }
    public Godot.Resource Result { get; private set; }
    public bool IsCompleted { get; private set; }
    public float Progress { get; private set; }

    /// <summary>加载完成时触发。</summary>
    public event Action<ResourceRequest> Completed;

    internal ResourceRequest(string path)
    {
        Path = path;
    }

    internal void SetProgress(float progress) => Progress = progress;

    internal void SetResult(Godot.Resource result)
    {
        Result = result;
        IsCompleted = true;
        Completed?.Invoke(this);
    }

    /// <summary>同步等待加载完成。注意：仅在资源已加载时有效。</summary>
    public Godot.Resource WaitForCompletion() => Result;
}
