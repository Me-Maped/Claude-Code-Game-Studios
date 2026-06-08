namespace LynxFramework.Pool;

/// <summary>
/// 对象池统计信息。供 DebugModule 展示。
/// </summary>
public struct PoolStat
{
    public string PoolKey;
    public int ActiveCount;
    public int AvailableCount;
    public long HitCount;
    public long MissCount;
    public float HitRate;
}
