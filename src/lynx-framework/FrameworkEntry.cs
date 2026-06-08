using Godot;
using LynxFramework.Audio;
using LynxFramework.Camera;
using LynxFramework.Core;
using LynxFramework.Data;
using LynxFramework.Debug;
using LynxFramework.Entity;
using LynxFramework.HotUpdate;
using LynxFramework.Input;
using LynxFramework.Localization;
using LynxFramework.Log;
using LynxFramework.Network;
using LynxFramework.Pool;
using LynxFramework.Procedure;
using LynxFramework.Resource;
using LynxFramework.Save;
using LynxFramework.Scene;
using LynxFramework.Settings;
using LynxFramework.Streaming;
using LynxFramework.UI;

namespace LynxFramework;

/// <summary>
/// 框架入口。Autoload 单例，按顺序初始化所有模块。
/// 将此节点设置为 Godot 项目 Autoload 即可启动整个框架。
/// </summary>
public partial class FrameworkEntry : Node
{
    public static FrameworkEntry Instance { get; private set; }

    private ModuleManager _moduleManager;
    private UpdateDriver _updateDriver;
    private bool _isReady;

    [Signal]
    public delegate void FrameworkReadyEventHandler();

    public override void _EnterTree()
    {
        Instance = this;
        _moduleManager = new ModuleManager();
    }

    public override void _Ready()
    {
        // 1. 检查热更新回滚标记
        CheckRollbackFlag();

        // 2. 按优先级注册所有模块
        RegisterBuiltinModules();

        // 3. 初始化所有模块
        _moduleManager.InitAll();

        // 4. 获取 UpdateDriver 引用并启动驱动
        _updateDriver = _moduleManager.GetModule<UpdateDriver>();
        _updateDriver?.Start();

        _isReady = true;
        EmitSignal(SignalName.FrameworkReady);

        var log = GetModule<LogService>();
        log?.Info("LynxFramework initialized successfully.");
    }

    public override void _Process(double delta)
    {
        if (!_isReady) return;
        _updateDriver?.DriveProcess((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isReady) return;
        _updateDriver?.DrivePhysicsProcess((float)delta);
    }

    /// <summary>获取指定类型的模块实例。</summary>
    public T GetModule<T>() where T : IModule => _moduleManager.GetModule<T>();

    /// <summary>获取所有已注册的模块列表（按优先级排序）。</summary>
    public System.Collections.Generic.IReadOnlyList<IModule> GetAllModules() => _moduleManager.GetAllModules();

    /// <summary>关闭框架，按优先级逆序关闭所有模块。</summary>
    public void Shutdown()
    {
        _updateDriver?.Stop();
        _moduleManager.ShutdownAll();
        _isReady = false;
    }

    private void CheckRollbackFlag()
    {
        // TODO: 读取 user://hotupdate_rollback 标记
        // 若存在则调用 HotUpdateModule.RecoverLastVersion()
    }

    /// <summary>
    /// 注册所有内置模块。按 Priority 升序排列。
    /// 后续阶段的模块在此处添加。
    /// </summary>
    private void RegisterBuiltinModules()
    {
        // Phase 1: 核心骨架
        _moduleManager.RegisterModule(new EventBus());          // Priority 0
        _moduleManager.RegisterModule(new LogService());        // Priority 10
        _moduleManager.RegisterModule(new UpdateDriver());      // Priority 40

        // Phase 2: 资源与对象管理
        _moduleManager.RegisterModule(new ResourceService());   // Priority 20
        _moduleManager.RegisterModule(new PoolManager());       // Priority 30

        // Phase 3: 场景与 UI
        _moduleManager.RegisterModule(new SceneModule());       // Priority 120
        _moduleManager.RegisterModule(new UIModule());          // Priority 130

        // Phase 4: 游戏逻辑基础设施
        _moduleManager.RegisterModule(new InputModule());       // Priority 80
        _moduleManager.RegisterModule(new InputBufferModule()); // Priority 90
        _moduleManager.RegisterModule(new AudioModule());       // Priority 100
        _moduleManager.RegisterModule(new CameraModule());      // Priority 140
        _moduleManager.RegisterModule(new EntityModule());      // Priority 150
        _moduleManager.RegisterModule(new ProcedureModule());   // Priority 170

        // Phase 5: 数据与配置
        _moduleManager.RegisterModule(new GameSettingsModule());// Priority 50
        _moduleManager.RegisterModule(new DataTableModule());   // Priority 60
        _moduleManager.RegisterModule(new LocalizationModule());// Priority 70
        _moduleManager.RegisterModule(new SaveModule());        // Priority 180

        // Phase 6: 网络与高级功能
        _moduleManager.RegisterModule(new NetworkModule());     // Priority 110
        _moduleManager.RegisterModule(new WorldStreamingModule()); // Priority 160
        _moduleManager.RegisterModule(new DebugModule());       // Priority 190
        _moduleManager.RegisterModule(new HotUpdateModule());   // Priority 200
    }
}
