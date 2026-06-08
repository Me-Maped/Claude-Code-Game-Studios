using Godot;
using LynxFramework;
using LynxFramework.Audio;
using LynxFramework.Core;
using LynxFramework.Procedure;
using LynxFramework.Save;
using LynxFramework.UI;

/// <summary>
/// 结算流程。
/// </summary>
public class GameOverProcedure : IProcedure
{
    public void OnEnter()
    {
        var fw = FrameworkEntry.Instance;

        // 保存最高分
        var save = fw.GetModule<SaveModule>();
        var data = save.Load("highscore", 0) ?? new Godot.Collections.Dictionary();
        int currentScore = data.ContainsKey("score") ? data["score"].AsInt32() : 0;
        // score 由 GameplayProcedure 通过 EventBus 传递，此处简化
        data["score"] = Mathf.Max(currentScore, 0);
        save.Save("highscore", 0, data);

        // 打开结算 UI
        var ui = fw.GetModule<UIModule>();
        ui.OpenUI("GameOver");

        // 播放音效
        fw.GetModule<AudioModule>().PlaySFX("res://audio/sfx/game_over.wav");
    }

    public void OnUpdate(float delta)
    {
        // 等待玩家点击重试或返回菜单
    }

    public void OnLeave()
    {
        FrameworkEntry.Instance.GetModule<UIModule>().CloseUI("GameOver");
    }
}
