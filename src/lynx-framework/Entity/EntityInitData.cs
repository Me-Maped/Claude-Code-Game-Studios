using Godot;

namespace LynxFramework.Entity;

/// <summary>
/// 实体初始化数据。
/// </summary>
public class EntityInitData
{
    public string EntityName { get; set; }
    public Vector3 Position { get; set; }
    public string Group { get; set; }
    public Variant ExtraData { get; set; }
}
