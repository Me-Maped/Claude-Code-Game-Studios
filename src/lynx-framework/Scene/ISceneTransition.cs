using System;

namespace LynxFramework.Scene;

/// <summary>
/// 场景转场动画接口。实现此接口可自定义转场效果。
/// </summary>
public interface ISceneTransition
{
    /// <summary>播放退场动画（当前场景消失）。</summary>
    void PlayOut(Action onComplete);

    /// <summary>播放入场动画（新场景出现）。</summary>
    void PlayIn(Action onComplete);
}
