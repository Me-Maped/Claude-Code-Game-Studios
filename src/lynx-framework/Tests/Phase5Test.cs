using Godot;
using LynxFramework;
using LynxFramework.Core;
using LynxFramework.Data;
using LynxFramework.Localization;
using LynxFramework.Log;
using LynxFramework.Save;
using LynxFramework.Settings;

/// <summary>
/// Phase 5 验证脚本。
/// 测试 DataTableModule、GameSettingsModule、LocalizationModule、SaveModule。
/// </summary>
public partial class Phase5Test : Node
{
    private int _passCount;
    private int _failCount;

    public override void _Ready()
    {
        GD.Print("=== LynxFramework Phase 5 Test ===");
        GD.Print("");

        var framework = FrameworkEntry.Instance;
        if (framework == null)
        {
            GD.PushError("FATAL: FrameworkEntry.Instance is null.");
            PrintResult();
            return;
        }

        // --- SaveModule 测试 ---
        var saveModule = framework.GetModule<SaveModule>();
        Assert(saveModule != null, "SaveModule registered");

        // 测试保存
        var testData = new Godot.Collections.Dictionary
        {
            ["player_name"] = "TestPlayer",
            ["level"] = 10,
            ["score"] = 9999
        };
        saveModule.Save("test_save", 0, testData);
        Assert(saveModule.SaveExists("test_save", 0), "Save file created");

        // 测试加载
        var loaded = saveModule.Load("test_save", 0);
        Assert(loaded != null, "Save file loaded");
        Assert(loaded["player_name"].AsString() == "TestPlayer", "Save data integrity (player_name)");
        Assert(loaded["level"].AsInt32() == 10, "Save data integrity (level)");
        Assert(loaded["score"].AsInt32() == 9999, "Save data integrity (score)");

        // 测试版本号
        Assert(loaded["_version"].AsInt32() == 1, "Save version is 1");

        // 测试删除
        saveModule.Delete("test_save", 0);
        Assert(!saveModule.SaveExists("test_save", 0), "Save file deleted");

        // --- GameSettingsModule 测试 ---
        var settingsModule = framework.GetModule<GameSettingsModule>();
        Assert(settingsModule != null, "GameSettingsModule registered");

        settingsModule.RegisterSetting("master_volume", 0.8f);
        settingsModule.RegisterSetting("fullscreen", false);
        Assert(settingsModule.HasSetting("master_volume"), "Setting 'master_volume' registered");
        Assert(settingsModule.HasSetting("fullscreen"), "Setting 'fullscreen' registered");

        var volume = settingsModule.GetSetting("master_volume");
        Assert(volume.AsSingle() == 0.8f, "Default setting value correct");

        settingsModule.SetSetting("master_volume", 0.5f);
        var newVolume = settingsModule.GetSetting("master_volume");
        Assert(newVolume.AsSingle() == 0.5f, "Setting value updated");

        settingsModule.ResetToDefault("master_volume");
        var resetVolume = settingsModule.GetSetting("master_volume");
        Assert(resetVolume.AsSingle() == 0.8f, "Reset to default works");

        // --- LocalizationModule 测试 ---
        var locModule = framework.GetModule<LocalizationModule>();
        Assert(locModule != null, "LocalizationModule registered");
        Assert(locModule.GetLanguage() == "en", "Default language is 'en'");

        // GetText 测试（使用 Godot 内置翻译系统，无翻译时返回 key 本身）
        var text = locModule.GetText("test_key");
        Assert(text == "test_key", "GetText returns key when no translation loaded");

        // --- DataTableModule 测试 ---
        var dataTableModule = framework.GetModule<DataTableModule>();
        Assert(dataTableModule != null, "DataTableModule registered");
        Assert(!dataTableModule.HasTable("test_table"), "Table not loaded initially");

        // --- SaveModule 版本迁移测试 ---
        GD.Print("");
        GD.Print("--- SaveModule Migration Test ---");

        // 注册迁移脚本
        saveModule.SetCurrentVersion(3);
        saveModule.RegisterMigration(new TestMigration1());
        saveModule.RegisterMigration(new TestMigration2());

        // 保存 v1 数据
        var v1Data = new Godot.Collections.Dictionary
        {
            ["_version"] = 1,
            ["name"] = "LegacyPlayer"
        };
        // 直接写文件模拟 v1 存档
        var filePath = "user://saves/migration_test_0.sav";
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(Json.Stringify(v1Data, "\t"));
        file.Close();

        // 加载（应自动迁移到 v3）
        var migrated = saveModule.Load("migration_test", 0);
        Assert(migrated != null, "Migration: file loaded");
        Assert(migrated["player_name"].AsString() == "LegacyPlayer", "Migration v1→v2: 'name' renamed to 'player_name'");
        Assert(migrated["title"].AsString() == "Newcomer", "Migration v2→v3: 'title' field added");

        // 清理
        saveModule.Delete("migration_test", 0);
        saveModule.SetCurrentVersion(1);

        // --- 最终结果 ---
        GD.Print("");
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
        GD.Print("========================================");
        if (_failCount == 0)
        {
            GD.Print($"  RESULT: PASS ({_passCount}/{_passCount} tests passed)");
        }
        else
        {
            GD.Print($"  RESULT: FAIL ({_failCount} failed, {_passCount} passed, {_passCount + _failCount} total)");
        }
        GD.Print("========================================");
    }
}

/// <summary>测试迁移脚本 v1 → v2：将 'name' 重命名为 'player_name'。</summary>
public class TestMigration1 : ISaveMigration
{
    public int FromVersion => 1;
    public int ToVersion => 2;

    public Godot.Collections.Dictionary Migrate(Godot.Collections.Dictionary data)
    {
        if (data.ContainsKey("name"))
        {
            data["player_name"] = data["name"];
            data.Remove("name");
        }
        data["_version"] = 2;
        return data;
    }
}

/// <summary>测试迁移脚本 v2 → v3：添加 'title' 字段。</summary>
public class TestMigration2 : ISaveMigration
{
    public int FromVersion => 2;
    public int ToVersion => 3;

    public Godot.Collections.Dictionary Migrate(Godot.Collections.Dictionary data)
    {
        data["title"] = "Newcomer";
        data["_version"] = 3;
        return data;
    }
}
