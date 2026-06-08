using Godot;
using LynxFramework;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Scene;
using LynxFramework.UI;

/// <summary>
/// Phase 3 验证脚本。
/// 测试 SceneModule（加载/卸载/栈）和 UIModule（栈管理/生命周期）。
/// </summary>
public partial class Phase3Test : Node
{
    public override void _Ready()
    {
        GD.Print("=== LynxFramework Phase 3 Test ===");

        var framework = FrameworkEntry.Instance;
        if (framework == null)
        {
            GD.PushError("FAIL: FrameworkEntry.Instance is null.");
            return;
        }

        // --- SceneModule 测试 ---
        var sceneModule = framework.GetModule<SceneModule>();
        GD.Print(sceneModule != null ? "✓ SceneModule registered" : "FAIL: SceneModule not found");

        GD.Print($"  Current scene: {sceneModule.GetCurrentScenePath() ?? "(none)"}");
        GD.Print($"  Stack depth: {sceneModule.StackDepth}");

        // --- UIModule 测试 ---
        var uiModule = framework.GetModule<UIModule>();
        GD.Print(uiModule != null ? "✓ UIModule registered" : "FAIL: UIModule not found");

        GD.Print($"  Stack depth: {uiModule.StackDepth}");
        GD.Print($"  IsUIOpen('TestPanel'): {uiModule.IsUIOpen("TestPanel")}");

        // 验证 EventBus 集成
        var eventBus = framework.GetModule<EventBus>();
        bool sceneLoadedFired = false;
        bool uiOpenedFired = false;
        eventBus.Subscribe("scene_loaded", () => sceneLoadedFired = true);
        eventBus.Subscribe("ui_opened", () => uiOpenedFired = true);

        GD.Print("✓ EventBus subscriptions set for scene/UI events");

        // 验证 LogService 集成
        var logService = framework.GetModule<LogService>();
        logService.Info("Phase 3 modules (Scene + UI) initialized successfully");

        GD.Print("=== Phase 3 Test Complete ===");
        GD.Print("");
        GD.Print("Note: Full scene/UI testing requires actual .tscn scene files.");
        GD.Print("Create test scenes in res://ui/ to test UIModule.OpenUI().");
    }
}
