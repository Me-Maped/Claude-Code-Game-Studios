using LynxFramework.Pool;

namespace LynxFramework.Entity;

/// <summary>
/// 实体接口。继承 IPoolableObject 以支持对象池复用。
/// </summary>
public interface IEntity : IPoolableObject
{
    /// <summary>实体名称（用于池化和分组）。</summary>
    string EntityName { get; }

    /// <summary>实体初始化。</summary>
    void OnInit(EntityInitData data);

    /// <summary>实体反初始化（回收前清理）。</summary>
    void OnDeinit();
}
