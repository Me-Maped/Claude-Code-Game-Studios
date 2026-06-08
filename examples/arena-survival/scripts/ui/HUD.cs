using Godot;
using LynxFramework;
using LynxFramework.Core;
using LynxFramework.UI;

/// <summary>
/// 游戏内 HUD 面板。
/// </summary>
public partial class HUD : Control, IUIPanel
{
    private Label _scoreLabel;
    private Label _healthLabel;
    private EventBus _eventBus;

    public void OnOpen(UIOpenParam param)
    {
        _scoreLabel = GetNode<Label>("ScoreLabel");
        _healthLabel = GetNode<Label>("HealthLabel");

        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _eventBus.Subscribe("score_changed", OnScoreChanged);
        _eventBus.Subscribe("health_changed", OnHealthChanged);
    }

    public void OnClose()
    {
        _eventBus?.Unsubscribe("score_changed", OnScoreChanged);
        _eventBus?.Unsubscribe("health_changed", OnHealthChanged);
    }

    public void OnPause() { }
    public void OnResume() { }
    public void OnCover() { }
    public void OnUncover() { }

    private void OnScoreChanged()
    {
        // 实际实现中从事件载荷获取分数
        _scoreLabel.Text = $"Score: 0";
    }

    private void OnHealthChanged()
    {
        _healthLabel.Text = $"HP: 100";
    }
}
