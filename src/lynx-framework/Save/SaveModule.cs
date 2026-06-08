using System;
using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;

namespace LynxFramework.Save;

/// <summary>
/// 存档模块。
/// 负责存档的读写、加密、版本迁移。
/// </summary>
public class SaveModule : IModule
{
    private LogService _logService;
    private EventBus _eventBus;
    private readonly List<ISaveMigration> _migrations = new();
    private int _currentVersion = 1;
    private readonly string _saveDir = "user://saves/";

    public int Priority => 180;

    public void OnInit()
    {
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        DirAccess.MakeDirRecursiveAbsolute(_saveDir);
    }

    public void OnShutdown() { }

    /// <summary>保存数据到指定槽位。</summary>
    public void Save(string slotKey, int slotIndex, Godot.Collections.Dictionary data)
    {
        data["_version"] = _currentVersion;
        data["_save_time"] = DateTime.Now.ToString("O");

        var filePath = GetFilePath(slotKey, slotIndex);
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            _logService?.Error($"Save failed: {FileAccess.GetOpenError()}");
            return;
        }

        var json = Json.Stringify(data, "\t");
        file.StoreString(json);
        file.Close();

        _eventBus?.Emit("save_completed", new SaveEventArg { SlotKey = slotKey, SlotIndex = slotIndex });
        _logService?.Info($"Saved: {slotKey}[{slotIndex}]");
    }

    /// <summary>从指定槽位加载数据。</summary>
    public Godot.Collections.Dictionary Load(string slotKey, int slotIndex)
    {
        var filePath = GetFilePath(slotKey, slotIndex);
        if (!FileAccess.FileExists(filePath)) return null;

        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null) return null;

        var json = file.GetAsText();
        file.Close();

        var result = Json.ParseString(json);
        if (result.VariantType != Variant.Type.Dictionary) return null;

        var data = result.AsGodotDictionary();

        // 版本迁移
        var version = data.ContainsKey("_version") ? data["_version"].AsInt32() : 1;
        if (version < _currentVersion)
            data = MigrateData(data, version);

        return data;
    }

    /// <summary>删除指定槽位的存档。</summary>
    public void Delete(string slotKey, int slotIndex)
    {
        var filePath = GetFilePath(slotKey, slotIndex);
        if (FileAccess.FileExists(filePath))
        {
            DirAccess.RemoveAbsolute(filePath);
            _logService?.Info($"Deleted save: {slotKey}[{slotIndex}]");
        }
    }

    /// <summary>获取所有槽位信息。</summary>
    public List<SaveSlotInfo> GetSlotsInfo(string slotKey)
    {
        var result = new List<SaveSlotInfo>();
        var dir = DirAccess.Open(_saveDir);
        if (dir == null) return result;

        dir.ListDirBegin();
        var fileName = dir.GetNext();
        while (fileName != string.Empty)
        {
            if (fileName.StartsWith(slotKey) && fileName.EndsWith(".sav"))
            {
                result.Add(ParseSlotInfo(fileName));
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();
        return result;
    }

    /// <summary>注册存档迁移脚本。</summary>
    public void RegisterMigration(ISaveMigration migration) => _migrations.Add(migration);

    /// <summary>设置当前存档版本号。</summary>
    public void SetCurrentVersion(int version) => _currentVersion = version;

    /// <summary>检查存档是否存在。</summary>
    public bool SaveExists(string slotKey, int slotIndex)
        => FileAccess.FileExists(GetFilePath(slotKey, slotIndex));

    private Godot.Collections.Dictionary MigrateData(Godot.Collections.Dictionary data, int fromVersion)
    {
        var currentVersion = fromVersion;
        while (currentVersion < _currentVersion)
        {
            var migration = _migrations.Find(m => m.FromVersion == currentVersion);
            if (migration == null)
            {
                _logService?.Warning($"No migration from v{currentVersion} to v{currentVersion + 1}");
                break;
            }
            data = migration.Migrate(data);
            currentVersion = migration.ToVersion;
        }
        return data;
    }

    private string GetFilePath(string slotKey, int slotIndex)
        => $"{_saveDir}{slotKey}_{slotIndex}.sav";

    private SaveSlotInfo ParseSlotInfo(string fileName)
    {
        var filePath = _saveDir + fileName;
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        if (file == null) return new SaveSlotInfo();

        var json = file.GetAsText();
        file.Close();

        var result = Json.ParseString(json);
        if (result.VariantType != Variant.Type.Dictionary)
            return new SaveSlotInfo { SlotIndex = -1 };

        var data = result.AsGodotDictionary();
        return new SaveSlotInfo
        {
            SlotIndex = 0,
            SaveTime = data.ContainsKey("_save_time") ? data["_save_time"].AsString() : "",
            DataVersion = data.ContainsKey("_version") ? data["_version"].AsInt32() : 1,
            MetaData = data
        };
    }
}
