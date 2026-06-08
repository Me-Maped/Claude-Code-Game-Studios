namespace LynxFramework.Procedure;

/// <summary>
/// 流程接口。实现此接口定义游戏流程状态。
/// </summary>
public interface IProcedure
{
    /// <summary>进入流程时调用。</summary>
    void OnEnter();

    /// <summary>流程每帧更新。</summary>
    void OnUpdate(float delta);

    /// <summary>离开流程时调用。</summary>
    void OnLeave();
}
