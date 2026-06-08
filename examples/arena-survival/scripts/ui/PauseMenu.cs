using Godot;
using LynxFramework;
using LynxFramework.Input;
using LynxFramework.Procedure;
using LynxFramework.UI;

/// <summary>
/// 暂停菜单面板。
/// </summary>
public partial class PauseMenu : Control, IUIPanel
{
    public void OnOpen(UIOpenParam param)
    {
        // 暂停游戏
        GetTree().Paused = true;
        // 禁用游戏输入
        FrameworkEntry.Instance.GetModule<InputModule>().DisableAll();
    }

    public void OnClose()
    {
        GetTree().Paused = false;
        FrameworkEntry.Instance.GetModule<InputModule>().EnableAll();
    }

    public void OnPause() { }
    public void OnResume() { }
    public void OnCover() { }
    public void OnUncover() { }

    // 按钮回调
    private void OnResumePressed()
    {
        FrameworkEntry.Instance.GetModule<UIModule>().CloseUI("PauseMenu");
    }

    private void OnRestartPressed()
    {
        FrameworkEntry.Instance.GetModule<UIModule>().CloseUI("PauseMenu");
        FrameworkEntry.Instance.GetModule<ProcedureModule>().SetCurrentProcedure("Gameplay");
    }

    private void OnQuitPressed()
    {
        FrameworkEntry.Instance.GetModule<UIModule>().CloseUI("PauseMenu");
        FrameworkEntry.Instance.GetModule<ProcedureModule>().SetCurrentProcedure("Menu");
    }
}
