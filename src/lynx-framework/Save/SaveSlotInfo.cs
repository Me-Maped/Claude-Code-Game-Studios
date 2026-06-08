using Godot;

namespace LynxFramework.Save;

/// <summary>
/// 存档槽位信息。
/// </summary>
public struct SaveSlotInfo
{
    public int SlotIndex;
    public string SaveTime;
    public int DataVersion;
    public Godot.Collections.Dictionary MetaData;
}
