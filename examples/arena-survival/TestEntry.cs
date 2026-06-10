using Godot;
using LynxFramework;
using LynxFramework.Bridge;

/// <summary>
/// Arena Survival 用例测试入口。
/// 作为空场景的根节点脚本，依次调用 FrameworkBridge 各分组接口并打印结果。
/// 运行方式：将 TestEntry.tscn 设为 Godot 项目的启动场景后运行。
/// </summary>
public partial class TestEntry : Node
{
    public override void _Ready()
    {
        GD.Print("[Test:arena-survival] === 开始测试 ===");
        TestFrameworkState();
        TestLog();
        TestEvents();
        TestSettings();
        TestLocalization();
        GD.Print("[Test:arena-survival] === 测试完成 ===");
    }

    // ─── 框架状态 ────────────────────────────────────────

    private void TestFrameworkState()
    {
        bool ready = FrameworkBridge.IsReady;
        GD.Print($"[Test] IsReady = {ready}");

        var fw = FrameworkEntry.Instance;
        GD.Print($"[Test] FrameworkEntry accessible = {fw != null}");
    }

    // ─── 日志 ────────────────────────────────────────────

    private void TestLog()
    {
        FrameworkBridge.LogDebug("LogDebug OK");
        FrameworkBridge.LogInfo("LogInfo OK");
        FrameworkBridge.LogWarning("LogWarning OK");
        FrameworkBridge.LogError("LogError OK");
        GD.Print("[Test] Log calls passed");
    }

    // ─── 事件 ────────────────────────────────────────────

    private void TestEvents()
    {
        FrameworkBridge.Emit("test_ping");
        FrameworkBridge.EmitVariant("test_data", new Godot.Collections.Dictionary { ["value"] = 99 });
        GD.Print("[Test] Event emit calls passed");
    }

    // ─── 设置 ────────────────────────────────────────────

    private void TestSettings()
    {
        FrameworkBridge.RegisterSetting("test_volume", 0.8f);
        var val = FrameworkBridge.GetSetting("test_volume");
        GD.Print($"[Test] GetSetting('test_volume') = {val}");

        FrameworkBridge.SetSetting("test_volume", 0.5f);
        GD.Print($"[Test] SetSetting -> {FrameworkBridge.GetSetting("test_volume")}");
    }

    // ─── 本地化 ──────────────────────────────────────────

    private void TestLocalization()
    {
        string lang = FrameworkBridge.GetLanguage();
        GD.Print($"[Test] GetLanguage = {lang}");

        string text = FrameworkBridge.Tr("TEST_KEY");
        GD.Print($"[Test] Tr('TEST_KEY') = {text}");
    }
}
