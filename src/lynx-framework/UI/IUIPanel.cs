namespace LynxFramework.UI;

/// <summary>
/// UI 面板接口。所有 UI 面板应实现此接口以接收生命周期回调。
/// </summary>
public interface IUIPanel
{
    /// <summary>面板打开时调用。</summary>
    void OnOpen(UIOpenParam param);

    /// <summary>面板关闭时调用。</summary>
    void OnClose();

    /// <summary>被新面板覆盖时调用（不再是栈顶）。</summary>
    void OnPause();

    /// <summary>上层面板关闭，恢复为栈顶时调用。</summary>
    void OnResume();

    /// <summary>有新面板打开覆盖当前面板。</summary>
    void OnCover();

    /// <summary>上层面板关闭，当前面板露出。</summary>
    void OnUncover();
}
