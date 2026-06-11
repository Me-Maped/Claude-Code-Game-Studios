using Godot;
using LynxFramework.Audio;
using LynxFramework.Bridge;
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

    // ═══════════════════════════════════════
    // GDScript 桥接 API
    // ═══════════════════════════════════════

    /// <summary>框架是否已完成初始化。</summary>
    public bool IsReady => _isReady;

    /// <summary>订阅无参事件，返回 handler ID 供 GDScript 取消订阅。</summary>
    public int Subscribe(string eventId, Callable callback)
    {
        var bus = GetModule<EventBus>();
        return bus == null ? -1 : EventBusBridge.Subscribe(bus, eventId, callback);
    }

    /// <summary>订阅 Variant 载荷事件，返回 handler ID 供 GDScript 取消订阅。</summary>
    public int SubscribeVariant(string eventId, Callable callback)
    {
        var bus = GetModule<EventBus>();
        return bus == null ? -1 : EventBusBridge.SubscribeVariant(bus, eventId, callback);
    }

    /// <summary>通过 handler ID 取消订阅 GDScript 事件回调。</summary>
    public void Unsubscribe(string eventId, int handlerId)
    {
        var bus = GetModule<EventBus>();
        if (bus != null)
            EventBusBridge.Unsubscribe(bus, eventId, handlerId);
    }

    /// <summary>发射无参事件。</summary>
    public void Emit(string eventId)
        => GetModule<EventBus>()?.Emit(eventId);

    /// <summary>发射 Variant 载荷事件。</summary>
    public void EmitVariant(string eventId, Variant data)
        => GetModule<EventBus>()?.Emit(eventId, new VariantEventArg { Data = data });

    /// <summary>输出 Debug 日志。</summary>
    public void LogDebug(string message)
        => GetModule<LogService>()?.Debug(message);

    /// <summary>输出 Info 日志。</summary>
    public void LogInfo(string message)
        => GetModule<LogService>()?.Info(message);

    /// <summary>输出 Warning 日志。</summary>
    public void LogWarning(string message)
        => GetModule<LogService>()?.Warning(message);

    /// <summary>输出 Error 日志。</summary>
    public void LogError(string message)
        => GetModule<LogService>()?.Error(message);

    /// <summary>加载场景。</summary>
    public void LoadScene(string scenePath)
        => GetModule<SceneModule>()?.LoadSceneAsync(scenePath);

    /// <summary>切换到指定场景。</summary>
    public void SwitchScene(string scenePath)
        => GetModule<SceneModule>()?.SwitchToScene(scenePath);

    /// <summary>返回上一场景。</summary>
    public void GoBack()
        => GetModule<SceneModule>()?.GoBack();

    /// <summary>打开 UI 面板。</summary>
    public void OpenUI(string uiName)
        => GetModule<UIModule>()?.OpenUI(uiName);

    /// <summary>关闭指定 UI 面板。</summary>
    public void CloseUI(string uiName)
        => GetModule<UIModule>()?.CloseUI(uiName);

    /// <summary>关闭栈顶 UI 面板。</summary>
    public void CloseTopUI()
        => GetModule<UIModule>()?.CloseTopUI();

    /// <summary>关闭所有 UI 面板。</summary>
    public void CloseAllUI()
        => GetModule<UIModule>()?.CloseAllUI();

    /// <summary>检查指定 UI 面板是否打开。</summary>
    public bool IsUIOpen(string uiName)
        => GetModule<UIModule>()?.IsUIOpen(uiName) ?? false;

    /// <summary>播放背景音乐。</summary>
    public void PlayBGM(string path, float fadeIn = 0f)
        => GetModule<AudioModule>()?.PlayBGM(path, fadeIn);

    /// <summary>停止背景音乐。</summary>
    public void StopBGM(float fadeOut = 0f)
        => GetModule<AudioModule>()?.StopBGM(fadeOut);

    /// <summary>播放音效。</summary>
    public void PlaySFX(string path)
        => GetModule<AudioModule>()?.PlaySFX(path);

    /// <summary>播放 2D 音效。</summary>
    public void PlaySFX2D(string path, Vector2 position)
        => GetModule<AudioModule>()?.PlaySFX2D(path, position);

    /// <summary>播放 3D 音效。</summary>
    public void PlaySFX3D(string path, Vector3 position)
        => GetModule<AudioModule>()?.PlaySFX3D(path, position);

    /// <summary>设置音频总线音量。</summary>
    public void SetVolume(string busName, float volumeDb)
        => GetModule<AudioModule>()?.SetBusVolume(busName, volumeDb);

    /// <summary>检查动作是否按下。</summary>
    public bool IsActionPressed(string action)
        => GetModule<InputModule>()?.IsActionPressed(action) ?? false;

    /// <summary>检查动作是否刚按下。</summary>
    public bool IsActionJustPressed(string action)
        => GetModule<InputModule>()?.IsActionJustPressed(action) ?? false;

    /// <summary>检查动作是否刚释放。</summary>
    public bool IsActionJustReleased(string action)
        => GetModule<InputModule>()?.IsActionJustReleased(action) ?? false;

    /// <summary>获取输入方向向量。</summary>
    public Vector2 GetInputVector(string negX, string posX, string negY, string posY)
        => GetModule<InputModule>()?.GetVector(negX, posX, negY, posY) ?? Vector2.Zero;

    /// <summary>禁用指定输入动作。</summary>
    public void DisableAction(string action)
        => GetModule<InputModule>()?.DisableAction(action);

    /// <summary>启用指定输入动作。</summary>
    public void EnableAction(string action)
        => GetModule<InputModule>()?.EnableAction(action);

    /// <summary>缓冲指定输入动作。</summary>
    public void BufferAction(string action, double maxTimeSec = 0.2)
        => GetModule<InputBufferModule>()?.BufferAction(action, maxTimeSec);

    /// <summary>消费指定缓冲动作。</summary>
    public bool ConsumeAction(string action)
        => GetModule<InputBufferModule>()?.ConsumeAction(action) ?? false;

    /// <summary>设置摄像机跟随目标。</summary>
    public void FollowTarget(Node target, float smoothing = 5f)
        => GetModule<CameraModule>()?.FollowTarget(target, smoothing);

    /// <summary>清除摄像机跟随目标。</summary>
    public void ClearCameraTarget()
        => GetModule<CameraModule>()?.ClearTarget();

    /// <summary>触发摄像机震动。</summary>
    public void CameraShake(float trauma, float duration, float decay = 5f)
        => GetModule<CameraModule>()?.Shake(trauma, duration, decay);

    /// <summary>切换当前流程。</summary>
    public void SetProcedure(string procName)
        => GetModule<ProcedureModule>()?.SetCurrentProcedure(procName);

    /// <summary>获取当前流程名称。</summary>
    public string GetCurrentProcedure()
        => GetModule<ProcedureModule>()?.GetCurrentProcedureName() ?? "";

    /// <summary>保存数据。</summary>
    public void SaveData(string slotKey, int slotIndex, Godot.Collections.Dictionary data)
        => GetModule<SaveModule>()?.Save(slotKey, slotIndex, data);

    /// <summary>加载数据。</summary>
    public Godot.Collections.Dictionary LoadData(string slotKey, int slotIndex)
        => GetModule<SaveModule>()?.Load(slotKey, slotIndex);

    /// <summary>删除存档。</summary>
    public void DeleteSave(string slotKey, int slotIndex)
        => GetModule<SaveModule>()?.Delete(slotKey, slotIndex);

    /// <summary>检查存档是否存在。</summary>
    public bool SaveExists(string slotKey, int slotIndex)
        => GetModule<SaveModule>()?.SaveExists(slotKey, slotIndex) ?? false;

    /// <summary>获取设置值。</summary>
    public Variant GetSetting(string key)
        => GetModule<GameSettingsModule>()?.GetSetting(key) ?? default;

    /// <summary>设置配置值。</summary>
    public void SetSetting(string key, Variant value)
        => GetModule<GameSettingsModule>()?.SetSetting(key, value);

    /// <summary>注册配置项。</summary>
    public void RegisterSetting(string key, Variant defaultValue)
        => GetModule<GameSettingsModule>()?.RegisterSetting(key, defaultValue);

    /// <summary>获取本地化文本。</summary>
    public string Tr(string key)
        => GetModule<LocalizationModule>()?.GetText(key) ?? key;

    /// <summary>切换语言。</summary>
    public void SetLanguage(string lang)
        => GetModule<LocalizationModule>()?.SetLanguage(lang);

    /// <summary>获取当前语言。</summary>
    public string GetLanguage()
        => GetModule<LocalizationModule>()?.GetLanguage() ?? "en";

    /// <summary>获取数据表行。</summary>
    public Godot.Collections.Dictionary GetDataRow(string tableName, string id)
        => GetModule<DataTableModule>()?.GetRow(tableName, id);

    /// <summary>加载数据表。</summary>
    public void LoadTable(string tableName)
        => GetModule<DataTableModule>()?.LoadTable(tableName);

    /// <summary>生成实体。</summary>
    public Node SpawnEntity(string entityName, Node parent, Vector3 position = default)
        => GetModule<EntityModule>()?.SpawnEntity(entityName, parent, position);

    /// <summary>销毁实体。</summary>
    public void DestroyEntity(Node entity)
        => GetModule<EntityModule>()?.DestroyEntity(entity);

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
