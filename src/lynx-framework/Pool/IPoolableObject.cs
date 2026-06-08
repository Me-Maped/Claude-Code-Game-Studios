namespace LynxFramework.Pool;

/// <summary>
/// 可池化对象接口。实现此接口的对象可被 ObjectPool 或 NodePool 管理。
/// </summary>
public interface IPoolableObject
{
    /// <summary>从池中取出时调用。</summary>
    void OnGet();

    /// <summary>回收到池中时调用。</summary>
    void OnRecycle();
}
