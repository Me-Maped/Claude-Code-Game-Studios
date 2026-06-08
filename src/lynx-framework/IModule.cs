namespace LynxFramework;

/// <summary>
/// 模块接口，所有框架模块必须实现此接口。
/// 模块按 Priority 升序初始化，按降序关闭。
/// </summary>
public interface IModule
{
    /// <summary>初始化优先级，值越小越先初始化</summary>
    int Priority { get; }

    /// <summary>模块初始化，在 FrameworkEntry._Ready() 中按优先级调用</summary>
    void OnInit();

    /// <summary>模块关闭，在 ShutdownAll() 中按优先级逆序调用</summary>
    void OnShutdown();
}
