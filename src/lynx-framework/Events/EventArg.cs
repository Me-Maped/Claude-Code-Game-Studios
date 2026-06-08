namespace LynxFramework;

/// <summary>
/// 所有事件载荷的基类。继承自 Godot.Resource 以支持类型安全的泛型约束。
/// 具体事件载荷应继承此类并添加所需字段。
/// </summary>
public partial class EventArg : Godot.Resource { }
