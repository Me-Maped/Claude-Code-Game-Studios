using Godot;

namespace LynxFramework.Entity;

/// <summary>实体生成/销毁事件载荷。</summary>
public partial class EntityEventArg : EventArg
{
    public Node Entity { get; set; }
    public string EntityName { get; set; }
}
