namespace LynxFramework.Network;

/// <summary>网络数据接收事件载荷。</summary>
public partial class NetworkDataEventArg : EventArg
{
    public byte[] Data { get; set; }
}

/// <summary>网络连接状态变更事件载荷。</summary>
public partial class NetworkStateEventArg : EventArg
{
    public ConnectionState State { get; set; }
}
