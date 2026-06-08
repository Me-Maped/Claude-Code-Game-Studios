using System;
using Godot;
using LynxFramework;
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

/// <summary>
/// LynxFramework 全模块集成测试。
/// 覆盖 Phase 1-6 所有模块，输出明确的 PASS/FAIL 结果。
/// </summary>
public partial class FullIntegrationTest : Node
{
    private int _passCount;
    private int _failCount;

    public override void _Ready()
    {
        GD.Print("╔══════════════════════════════════════════╗");
        GD.Print("║   LynxFramework Full Integration Test    ║");
        GD.Print("╚══════════════════════════════════════════╝");
        GD.Print("");

        var fw = FrameworkEntry.Instance;
        if (fw == null)
        {
            GD.PushError("FATAL: FrameworkEntry.Instance is null.");
            PrintResult();
            return;
        }

        // ═══════════════════════════════════════
        // Phase 1: 核心骨架
        // ═══════════════════════════════════════
        GD.Print("── Phase 1: Core ──");

        var eventBus = fw.GetModule<EventBus>();
        Assert(eventBus != null, "EventBus registered");

        var logService = fw.GetModule<LogService>();
        Assert(logService != null, "LogService registered");

        var updateDriver = fw.GetModule<UpdateDriver>();
        Assert(updateDriver != null, "UpdateDriver registered");

        // EventBus 订阅/发射
        bool evtReceived = false;
        Action handler = () => evtReceived = true;
        eventBus.Subscribe("test_evt", handler);
        eventBus.Emit("test_evt");
        Assert(evtReceived, "EventBus subscribe/emit works");
        eventBus.Unsubscribe("test_evt", handler);

        // EventBus 类型安全载荷
        bool typedReceived = false;
        eventBus.Subscribe<TestPayload>("test_typed", (arg) => typedReceived = arg.Value == 42);
        eventBus.Emit("test_typed", new TestPayload { Value = 42 });
        Assert(typedReceived, "EventBus typed payload works");

        GD.Print("");

        // ═══════════════════════════════════════
        // Phase 2: 资源与对象管理
        // ═══════════════════════════════════════
        GD.Print("── Phase 2: Pool & Resource ──");

        var poolManager = fw.GetModule<PoolManager>();
        Assert(poolManager != null, "PoolManager registered");

        var resourceService = fw.GetModule<ResourceService>();
        Assert(resourceService != null, "ResourceService registered");

        // 对象池测试
        poolManager.CreateObjectPool(() => new TestPoolableObj(), 2);
        var pObj1 = poolManager.GetObject<TestPoolableObj>();
        Assert(pObj1 != null, "ObjectPool get works");
        Assert(pObj1.IsActive, "ObjectPool OnGet called");
        poolManager.RecycleObject(pObj1);
        Assert(!pObj1.IsActive, "ObjectPool OnRecycle called");

        // 资源加载测试
        var icon = resourceService.LoadCached<Texture2D>("res://icon.svg");
        Assert(icon != null, "ResourceService LoadCached works");

        GD.Print("");

        // ═══════════════════════════════════════
        // Phase 3: 场景与 UI
        // ═══════════════════════════════════════
        GD.Print("── Phase 3: Scene & UI ──");

        var sceneModule = fw.GetModule<SceneModule>();
        Assert(sceneModule != null, "SceneModule registered");
        Assert(sceneModule.StackDepth == 0, "Scene stack initially empty");

        var uiModule = fw.GetModule<UIModule>();
        Assert(uiModule != null, "UIModule registered");
        Assert(uiModule.StackDepth == 0, "UI stack initially empty");
        Assert(!uiModule.IsUIOpen("test"), "IsUIOpen returns false for non-existent panel");

        GD.Print("");

        // ═══════════════════════════════════════
        // Phase 4: 游戏逻辑基础设施
        // ═══════════════════════════════════════
        GD.Print("── Phase 4: Game Logic ──");

        var procModule = fw.GetModule<ProcedureModule>();
        Assert(procModule != null, "ProcedureModule registered");
        procModule.RegisterProcedure("test_proc", () => new TestProc());
        procModule.SetCurrentProcedure("test_proc");
        Assert(procModule.GetCurrentProcedureName() == "test_proc", "Procedure switch works");

        var entityModule = fw.GetModule<EntityModule>();
        Assert(entityModule != null, "EntityModule registered");
        Assert(entityModule.ActiveEntityCount == 0, "No active entities initially");

        var inputModule = fw.GetModule<InputModule>();
        Assert(inputModule != null, "InputModule registered");
        inputModule.DisableAction("test_act");
        Assert(!inputModule.IsActionEnabled("test_act"), "DisableAction works");
        inputModule.EnableAction("test_act");
        Assert(inputModule.IsActionEnabled("test_act"), "EnableAction works");

        var inputBuffer = fw.GetModule<InputBufferModule>();
        Assert(inputBuffer != null, "InputBufferModule registered");
        inputBuffer.BufferAction("atk", 0.3);
        Assert(inputBuffer.IsActionBuffered("atk"), "BufferAction works");
        Assert(inputBuffer.ConsumeAction("atk"), "ConsumeAction works");
        Assert(!inputBuffer.ConsumeAction("atk"), "Double-consume prevented");

        var cameraModule = fw.GetModule<CameraModule>();
        Assert(cameraModule != null, "CameraModule registered");

        var audioModule = fw.GetModule<AudioModule>();
        Assert(audioModule != null, "AudioModule registered");

        GD.Print("");

        // ═══════════════════════════════════════
        // Phase 5: 数据与配置
        // ═══════════════════════════════════════
        GD.Print("── Phase 5: Data & Config ──");

        var saveModule = fw.GetModule<SaveModule>();
        Assert(saveModule != null, "SaveModule registered");

        var testData = new Godot.Collections.Dictionary { ["k"] = "v" };
        saveModule.Save("itest", 0, testData);
        Assert(saveModule.SaveExists("itest", 0), "Save works");
        var loaded = saveModule.Load("itest", 0);
        Assert(loaded != null && loaded["k"].AsString() == "v", "Load works");
        saveModule.Delete("itest", 0);
        Assert(!saveModule.SaveExists("itest", 0), "Delete works");

        var settingsModule = fw.GetModule<GameSettingsModule>();
        Assert(settingsModule != null, "GameSettingsModule registered");

        var locModule = fw.GetModule<LocalizationModule>();
        Assert(locModule != null, "LocalizationModule registered");

        var dataTableModule = fw.GetModule<DataTableModule>();
        Assert(dataTableModule != null, "DataTableModule registered");

        GD.Print("");

        // ═══════════════════════════════════════
        // Phase 6: 网络与高级功能
        // ═══════════════════════════════════════
        GD.Print("── Phase 6: Network & Advanced ──");

        var networkModule = fw.GetModule<NetworkModule>();
        Assert(networkModule != null, "NetworkModule registered");
        Assert(networkModule.GetConnectionState() == ConnectionState.Disconnected, "Initial state Disconnected");

        var worldModule = fw.GetModule<WorldStreamingModule>();
        Assert(worldModule != null, "WorldStreamingModule registered");
        Assert(worldModule.LoadedAreaCount == 0, "No areas loaded initially");

        var debugModule = fw.GetModule<DebugModule>();
        Assert(debugModule != null, "DebugModule registered");
        Assert(!debugModule.IsVisible, "Debug UI initially hidden");

        var hotUpdateModule = fw.GetModule<HotUpdateModule>();
        Assert(hotUpdateModule != null, "HotUpdateModule registered");
        Assert(hotUpdateModule.GetCurrentVersion() == "1.0.0", "Default version is 1.0.0");

        GD.Print("");

        // ═══════════════════════════════════════
        // 模块优先级排序验证
        // ═══════════════════════════════════════
        GD.Print("── Module Priority Order ──");
        int lastPriority = -1;
        bool orderCorrect = true;
        foreach (var module in fw.GetAllModules())
        {
            GD.Print($"  [{module.Priority,3}] {module.GetType().Name}");
            if (module.Priority < lastPriority) orderCorrect = false;
            lastPriority = module.Priority;
        }
        Assert(orderCorrect, "Modules sorted by priority (ascending)");

        GD.Print("");

        // ═══════════════════════════════════════
        // 最终结果
        // ═══════════════════════════════════════
        PrintResult();
    }

    private void Assert(bool condition, string testName)
    {
        if (condition)
        {
            GD.Print($"  ✓ {testName}");
            _passCount++;
        }
        else
        {
            GD.PushError($"  ✗ FAIL: {testName}");
            _failCount++;
        }
    }

    private void PrintResult()
    {
        GD.Print("");
        GD.Print("╔══════════════════════════════════════════╗");
        if (_failCount == 0)
        {
            GD.Print($"║  RESULT: PASS  ({_passCount}/{_passCount} tests passed)       ║");
        }
        else
        {
            GD.Print($"║  RESULT: FAIL  ({_failCount} failed, {_passCount} passed)    ║");
        }
        GD.Print("╚══════════════════════════════════════════╝");
    }
}

public partial class TestPayload : EventArg { public int Value; }
public class TestPoolableObj : IPoolableObject
{
    public bool IsActive;
    public void OnGet() => IsActive = true;
    public void OnRecycle() => IsActive = false;
}
public class TestProc : IProcedure
{
    public void OnEnter() { }
    public void OnUpdate(float delta) { }
    public void OnLeave() { }
}
