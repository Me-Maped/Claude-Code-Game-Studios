using Godot;

namespace LynxFramework.UI;

/// <summary>
/// UI 栈条目。记录面板的状态信息。
/// </summary>
public sealed class UIStackEntry
{
    public string Name;
    public Control Panel;
    public IUIPanel PanelInterface;
    public IUITransition Transition;
}
