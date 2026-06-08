using Godot;

namespace LynxFramework.UI;

/// <summary>
/// UI 面板打开参数。
/// </summary>
public class UIOpenParam
{
    public Variant Data { get; set; }
    public static UIOpenParam Empty { get; } = new() { Data = default };
}
