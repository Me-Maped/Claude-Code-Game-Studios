using Godot;
using LynxFramework;
using LynxFramework.Core;
using LynxFramework.Entity;
using LynxFramework.Procedure;
using LynxFramework.UI;

/// <summary>
/// Arena Survival 用例测试入口。
/// 作为空场景的根节点脚本，依次调用 FrameworkEntry 各分组接口并打印结果。
/// 运行方式：将 TestEntry.tscn 设为 Godot 项目的启动场景后运行。
/// </summary>
public partial class TestEntry : Node
{
    private FrameworkEntry _framework;

    public override void _Ready()
    {
        _framework = FrameworkEntry.Instance;

        GD.Print("[Test:arena-survival] === 开始测试 ===");
        TestFrameworkState();
        TestLog();
        TestEvents();
        TestSettings();
        TestLocalization();
        TestArenaExampleCode();
        GD.Print("[Test:arena-survival] === 测试完成 ===");
    }

    // ─── 框架状态 ────────────────────────────────────────

    private void TestFrameworkState()
    {
        bool ready = _framework?.IsReady ?? false;
        GD.Print($"[Test] IsReady = {ready}");

        var fw = FrameworkEntry.Instance;
        GD.Print($"[Test] FrameworkEntry accessible = {fw != null}");
    }

    // ─── 日志 ────────────────────────────────────────────

    private void TestLog()
    {
        _framework?.LogDebug("LogDebug OK");
        _framework?.LogInfo("LogInfo OK");
        _framework?.LogWarning("LogWarning OK");
        _framework?.LogError("LogError OK");
        GD.Print("[Test] Log calls passed");
    }

    // ─── 事件 ────────────────────────────────────────────

    private void TestEvents()
    {
        _framework?.Emit("test_ping");
        _framework?.EmitVariant("test_data", new Godot.Collections.Dictionary { ["value"] = 99 });
        GD.Print("[Test] Event emit calls passed");
    }

    // ─── 设置 ────────────────────────────────────────────

    private void TestSettings()
    {
        _framework?.RegisterSetting("test_volume", 0.8f);
        var val = _framework?.GetSetting("test_volume") ?? default;
        GD.Print($"[Test] GetSetting('test_volume') = {val}");

        _framework?.SetSetting("test_volume", 0.5f);
        GD.Print($"[Test] SetSetting -> {_framework?.GetSetting("test_volume") ?? default}");
    }

    // ─── 本地化 ──────────────────────────────────────────

    private void TestLocalization()
    {
        string lang = _framework?.GetLanguage() ?? "en";
        GD.Print($"[Test] GetLanguage = {lang}");

        string text = _framework?.Tr("TEST_KEY") ?? "TEST_KEY";
        GD.Print($"[Test] Tr('TEST_KEY') = {text}");
    }

    // ─── Arena Survival 示例代码 ─────────────────────────

    private void TestArenaExampleCode()
    {
        GD.Print("[Test:arena-survival] --- 示例代码接入 ---");
        TestArenaProcedures();
        TestArenaEntities();
        TestArenaUiPanels();
        TestArenaDataFiles();
    }

    private void TestArenaProcedures()
    {
        var proc = FrameworkEntry.Instance?.GetModule<ProcedureModule>();
        if (proc == null)
        {
            GD.Print("[Test] ProcedureModule unavailable, skip procedure examples");
            return;
        }

        proc.RegisterProcedure("Menu", () => new MenuProcedure());
        proc.RegisterProcedure("Gameplay", () => new GameplayProcedure());
        proc.RegisterProcedure("GameOver", () => new GameOverProcedure());
        GD.Print("[Test] Registered example procedures: Menu, Gameplay, GameOver");
        GD.Print("[Test] Procedure examples registered only; full scenes/audio are not required for this TestEntry run");
    }

    private void TestArenaEntities()
    {
        if (FrameworkEntry.Instance == null)
        {
            GD.Print("[Test] FrameworkEntry unavailable, skip entity examples");
            return;
        }

        var container = new Node3D { Name = "ArenaExampleContainer" };
        AddChild(container);

        var player = new Player { Name = "PlayerExample" };
        player.SetPhysicsProcess(false);
        container.AddChild(player);
        player.AddToGroup("Player");
        player.OnInit(new EntityInitData { EntityName = player.EntityName, Position = Vector3.Zero });

        var enemy = new Enemy { Name = "EnemyExample", Health = 2 };
        enemy.SetPhysicsProcess(false);
        container.AddChild(enemy);
        enemy.OnInit(new EntityInitData { EntityName = enemy.EntityName, Position = new Vector3(2, 0, 0) });
        enemy.TakeDamage(1);

        var bullet = new Bullet { Name = "BulletExample" };
        bullet.SetPhysicsProcess(false);
        container.AddChild(bullet);
        bullet.OnRecycle();

        enemy.OnDeinit();
        player.OnDeinit();
        container.QueueFree();
        GD.Print("[Test] Entity examples attached: Player, Enemy, Bullet");
    }

    private void TestArenaUiPanels()
    {
        if (FrameworkEntry.Instance == null)
        {
            GD.Print("[Test] FrameworkEntry unavailable, skip UI examples");
            return;
        }

        var hud = new HUD { Name = "HUDExample" };
        hud.AddChild(new Label { Name = "ScoreLabel" });
        hud.AddChild(new Label { Name = "HealthLabel" });
        AddChild(hud);
        hud.OnOpen(UIOpenParam.Empty);

        var eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        eventBus?.Emit("score_changed");
        eventBus?.Emit("health_changed");
        hud.OnClose();
        hud.QueueFree();

        var pauseMenu = new PauseMenu { Name = "PauseMenuExample" };
        AddChild(pauseMenu);
        pauseMenu.OnOpen(UIOpenParam.Empty);
        pauseMenu.OnClose();
        pauseMenu.QueueFree();

        GD.Print("[Test] UI examples attached: HUD, PauseMenu");
    }

    private void TestArenaDataFiles()
    {
        PrintDataFileSummary("res://examples/arena-survival/data/tables/enemies.json", "enemies");
        PrintDataFileSummary("res://examples/arena-survival/data/tables/weapons.json", "weapons");
    }

    private void PrintDataFileSummary(string path, string tableName)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.Print($"[Test] Data file missing: {path}");
            return;
        }

        var text = FileAccess.GetFileAsString(path);
        var parsed = Json.ParseString(text);
        if (parsed.VariantType != Variant.Type.Array)
        {
            GD.Print($"[Test] Data file is not an array: {path}");
            return;
        }

        var rows = parsed.AsGodotArray();
        GD.Print($"[Test] Loaded example data file '{tableName}' rows = {rows.Count}");
    }
}
