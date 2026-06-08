using Godot;

namespace LynxFramework.Settings;

/// <summary>设置变更事件载荷。</summary>
public partial class SettingsChangedEventArg : EventArg
{
    public string Key { get; set; }
    public Variant Value { get; set; }
}
