namespace LynxFramework.HotUpdate;

/// <summary>热更新事件载荷。</summary>
public partial class HotUpdateEventArg : EventArg
{
    public string Version { get; set; }
}
