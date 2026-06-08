using System;
using Godot;

namespace LynxFramework.UI;

/// <summary>
/// UI 过渡动画接口。实现此接口可自定义 UI 打开/关闭动画。
/// </summary>
public interface IUITransition
{
    /// <summary>播放打开动画。</summary>
    void PlayOpen(Control panel, Action onComplete);

    /// <summary>播放关闭动画。</summary>
    void PlayClose(Control panel, Action onComplete);
}
