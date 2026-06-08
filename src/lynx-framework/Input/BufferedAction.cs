namespace LynxFramework.Input;

/// <summary>
/// 缓冲动作条目。
/// </summary>
public struct BufferedAction
{
    public string ActionName;
    public double Timestamp;
    public double MaxTimeSec;
    public bool IsConsumed;
}
