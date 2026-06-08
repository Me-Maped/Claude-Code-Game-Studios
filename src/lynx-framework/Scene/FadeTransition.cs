using System;
using Godot;

namespace LynxFramework.Scene;

/// <summary>
/// 淡入淡出转场动画实现。
/// </summary>
public partial class FadeTransition : ISceneTransition
{
    private readonly float _duration;
    private readonly Color _color;
    private readonly Node _parentNode;

    /// <param name="parentNode">用于创建 Tween 的父节点</param>
    /// <param name="duration">转场时长（秒）</param>
    /// <param name="color">遮罩颜色</param>
    public FadeTransition(Node parentNode, float duration = 0.5f, Color color = default)
    {
        _parentNode = parentNode;
        _duration = duration;
        _color = color == default ? new Color(0, 0, 0, 1) : color;
    }

    public void PlayOut(Action onComplete)
    {
        // 创建全屏遮罩，从透明渐变到不透明
        var overlay = CreateOverlay();
        overlay.Modulate = new Color(1, 1, 1, 0);

        var tween = _parentNode.CreateTween();
        tween.TweenProperty(overlay, "modulate:a", 1.0f, _duration);
        tween.TweenCallback(Callable.From(() => onComplete?.Invoke()));
    }

    public void PlayIn(Action onComplete)
    {
        // 创建全屏遮罩，从不透明渐变到透明
        var overlay = CreateOverlay();
        overlay.Modulate = new Color(1, 1, 1, 1);

        var tween = _parentNode.CreateTween();
        tween.TweenProperty(overlay, "modulate:a", 0.0f, _duration);
        tween.TweenCallback(Callable.From(() =>
        {
            overlay.QueueFree();
            onComplete?.Invoke();
        }));
    }

    private ColorRect CreateOverlay()
    {
        var overlay = new ColorRect
        {
            Name = "TransitionOverlay",
            Color = _color,
            AnchorsPreset = (int)Control.LayoutPreset.FullRect,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        var canvasLayer = new CanvasLayer { Layer = 1000 };
        canvasLayer.AddChild(overlay);
        _parentNode.AddChild(canvasLayer);
        return overlay;
    }
}
