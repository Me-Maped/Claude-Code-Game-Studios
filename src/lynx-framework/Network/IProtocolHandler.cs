namespace LynxFramework.Network;

/// <summary>
/// 协议编解码接口。实现此接口可自定义网络消息的序列化方式。
/// </summary>
public interface IProtocolHandler
{
    /// <summary>编码消息为字节数组。</summary>
    byte[] Encode(object msgStruct);

    /// <summary>解码字节数组为消息对象。</summary>
    object Decode(byte[] data);
}
