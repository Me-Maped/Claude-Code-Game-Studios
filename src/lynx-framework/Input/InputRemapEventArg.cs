using Godot;

namespace LynxFramework.Input;

/// <summary>键位重映射事件载荷。</summary>
public partial class InputRemapEventArg : EventArg
{
    public string Action { get; set; }
    public InputEvent Event { get; set; }
}
