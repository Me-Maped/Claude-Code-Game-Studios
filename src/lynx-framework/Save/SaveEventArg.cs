namespace LynxFramework.Save;

/// <summary>存档事件载荷。</summary>
public partial class SaveEventArg : EventArg
{
    public string SlotKey { get; set; }
    public int SlotIndex { get; set; }
}
