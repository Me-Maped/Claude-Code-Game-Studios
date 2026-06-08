using Godot;
using LynxFramework;
using LynxFramework.Audio;
using LynxFramework.Camera;
using LynxFramework.Core;
using LynxFramework.Entity;
using LynxFramework.Input;
using LynxFramework.Log;
using LynxFramework.Procedure;

/// <summary>
/// Phase 4 验证脚本。
/// 测试 ProcedureModule、EntityModule、InputModule、InputBufferModule、CameraModule、AudioModule。
/// </summary>
public partial class Phase4Test : Node
{
    public override void _Ready()
    {
        GD.Print("=== LynxFramework Phase 4 Test ===");

        var framework = FrameworkEntry.Instance;
        if (framework == null)
        {
            GD.PushError("FAIL: FrameworkEntry.Instance is null.");
            return;
        }

        // --- ProcedureModule 测试 ---
        var procModule = framework.GetModule<ProcedureModule>();
        GD.Print(procModule != null ? "✓ ProcedureModule registered" : "FAIL: ProcedureModule not found");

        procModule.RegisterProcedure("TestProc", () => new TestProcedure());
        GD.Print(procModule.HasProcedure("TestProc") ? "✓ Procedure registered" : "FAIL: Procedure not registered");

        procModule.SetCurrentProcedure("TestProc");
        GD.Print(procModule.GetCurrentProcedureName() == "TestProc" ? "✓ Procedure switched" : "FAIL: Procedure switch failed");

        // --- EntityModule 测试 ---
        var entityModule = framework.GetModule<EntityModule>();
        GD.Print(entityModule != null ? "✓ EntityModule registered" : "FAIL: EntityModule not found");
        GD.Print($"  Active entities: {entityModule.ActiveEntityCount}");

        // --- InputModule 测试 ---
        var inputModule = framework.GetModule<InputModule>();
        GD.Print(inputModule != null ? "✓ InputModule registered" : "FAIL: InputModule not found");

        inputModule.DisableAction("test_action");
        GD.Print(!inputModule.IsActionEnabled("test_action") ? "✓ DisableAction works" : "FAIL: DisableAction failed");
        inputModule.EnableAction("test_action");
        GD.Print(inputModule.IsActionEnabled("test_action") ? "✓ EnableAction works" : "FAIL: EnableAction failed");

        // --- InputBufferModule 测试 ---
        var inputBuffer = framework.GetModule<InputBufferModule>();
        GD.Print(inputBuffer != null ? "✓ InputBufferModule registered" : "FAIL: InputBufferModule not found");

        inputBuffer.BufferAction("attack", 0.3);
        GD.Print(inputBuffer.IsActionBuffered("attack") ? "✓ BufferAction works" : "FAIL: BufferAction failed");

        var consumed = inputBuffer.ConsumeAction("attack");
        GD.Print(consumed ? "✓ ConsumeAction works" : "FAIL: ConsumeAction failed");
        GD.Print(!inputBuffer.ConsumeAction("attack") ? "✓ Double-consume prevented" : "FAIL: Double-consume allowed");

        // --- CameraModule 测试 ---
        var cameraModule = framework.GetModule<CameraModule>();
        GD.Print(cameraModule != null ? "✓ CameraModule registered" : "FAIL: CameraModule not found");

        var testCamera2D = new Camera2D { Name = "TestCamera2D" };
        AddChild(testCamera2D);
        cameraModule.SetActiveCamera(testCamera2D);
        GD.Print(cameraModule.GetCamera2D() == testCamera2D ? "✓ SetActiveCamera(Camera2D) works" : "FAIL: SetActiveCamera failed");

        cameraModule.Shake(0.8f, 0.5f);
        GD.Print("✓ Shake() called (visual verification needed)");

        // --- AudioModule 测试 ---
        var audioModule = framework.GetModule<AudioModule>();
        GD.Print(audioModule != null ? "✓ AudioModule registered" : "FAIL: AudioModule not found");

        // 设置音量（不需要实际音频文件）
        audioModule.SetBusVolume("Master", -10f);
        GD.Print("✓ SetBusVolume() called");

        // --- 验证模块优先级顺序 ---
        GD.Print("");
        GD.Print("--- Module Priority Order ---");
        foreach (var module in framework.GetAllModules())
        {
            GD.Print($"  [{module.Priority,3}] {module.GetType().Name}");
        }

        GD.Print("=== Phase 4 Test Complete ===");
    }
}

/// <summary>测试用流程。</summary>
public class TestProcedure : IProcedure
{
    public void OnEnter() => GD.Print("  TestProcedure.OnEnter()");
    public void OnUpdate(float delta) { }
    public void OnLeave() => GD.Print("  TestProcedure.OnLeave()");
}
