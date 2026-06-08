namespace LynxFramework.Scene;

/// <summary>场景加载/卸载事件载荷。</summary>
public partial class SceneEventArg : EventArg
{
    public string ScenePath { get; set; }
}
