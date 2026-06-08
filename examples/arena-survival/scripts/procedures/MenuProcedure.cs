using Godot;
using LynxFramework;
using LynxFramework.Audio;
using LynxFramework.Core;
using LynxFramework.Procedure;
using LynxFramework.Scene;
using LynxFramework.UI;

/// <summary>
/// 主菜单流程。
/// </summary>
public class MenuProcedure : IProcedure
{
    public void OnEnter()
    {
        var fw = FrameworkEntry.Instance;

        // 加载菜单场景
        var sceneModule = fw.GetModule<SceneModule>();
        sceneModule.LoadSceneAsync("res://scenes/MainMenu.tscn");

        // 打开菜单 UI
        var uiModule = fw.GetModule<UIModule>();
        uiModule.OpenUI("MainMenu");

        // 播放 BGM
        var audio = fw.GetModule<AudioModule>();
        audio.PlayBGM("res://audio/bgm/menu_theme.ogg", fadeIn: 1f);
    }

    public void OnUpdate(float delta)
    {
        // 菜单逻辑（按钮回调由 UI 面板处理）
    }

    public void OnLeave()
    {
        var fw = FrameworkEntry.Instance;
        var ui = fw.GetModule<UIModule>();
        ui.CloseUI("MainMenu");
    }
}
