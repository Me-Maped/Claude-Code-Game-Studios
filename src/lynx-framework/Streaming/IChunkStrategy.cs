using System.Collections.Generic;
using Godot;

namespace LynxFramework.Streaming;

/// <summary>
/// 区块加载策略接口。实现此接口可自定义世界流式加载逻辑。
/// </summary>
public interface IChunkStrategy
{
    /// <summary>获取需要加载的区块 ID 列表。</summary>
    List<string> GetChunksToLoad(Vector3 viewerPosition, float loadRadius);

    /// <summary>获取需要卸载的区块 ID 列表。</summary>
    List<string> GetChunksToUnload(Vector3 viewerPosition, float unloadRadius);
}
