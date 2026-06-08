using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Save;

namespace LynxFramework.Settings;

/// <summary>
/// 游戏设置模块。
/// 管理设置项的注册、读写、持久化。支持设置变更事件通知。
/// </summary>
public class GameSettingsModule : IModule
{
    private EventBus _eventBus;
    private LogService _logService;
    private SaveModule _saveModule;
    private readonly Dictionary<string, SettingEntry> _settings = new();

    public int Priority => 50;

    public void OnInit()
    {
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
        _saveModule = FrameworkEntry.Instance.GetModule<SaveModule>();
        Load();
    }

    public void OnShutdown() => Save();

    /// <summary>注册设置项。</summary>
    public void RegisterSetting(string key, Variant defaultValue)
    {
        if (!_settings.ContainsKey(key))
            _settings[key] = new SettingEntry { Key = key, DefaultValue = defaultValue, Value = defaultValue };
    }

    /// <summary>获取设置值。</summary>
    public Variant GetSetting(string key)
        => _settings.TryGetValue(key, out var entry) ? entry.Value : default;

    /// <summary>设置值。触发 settings_changed 事件。</summary>
    public void SetSetting(string key, Variant value)
    {
        if (!_settings.TryGetValue(key, out var entry)) return;
        entry.Value = value;
        entry.IsDirty = true;
        _eventBus?.Emit("settings_changed", new SettingsChangedEventArg { Key = key, Value = value });
    }

    /// <summary>检查设置项是否存在。</summary>
    public bool HasSetting(string key) => _settings.ContainsKey(key);

    /// <summary>应用所有设置（分辨率、音量等）。</summary>
    public void Apply()
    {
        if (_settings.TryGetValue("resolution", out var res))
            DisplayServer.WindowSetSize(res.Value.AsVector2I());

        if (_settings.TryGetValue("master_volume", out var vol))
            AudioServer.SetBusVolumeDb(0, vol.Value.AsSingle());
    }

    /// <summary>重置指定设置为默认值。</summary>
    public void ResetToDefault(string key)
    {
        if (_settings.TryGetValue(key, out var entry))
        {
            entry.Value = entry.DefaultValue;
            entry.IsDirty = true;
        }
    }

    /// <summary>重置所有设置为默认值。</summary>
    public void ResetAllToDefault()
    {
        foreach (var entry in _settings.Values)
        {
            entry.Value = entry.DefaultValue;
            entry.IsDirty = true;
        }
    }

    /// <summary>保存设置到存档。</summary>
    public void Save()
    {
        if (_saveModule == null) return;
        var data = new Godot.Collections.Dictionary();
        foreach (var kv in _settings)
            data[kv.Key] = kv.Value.Value;
        _saveModule.Save("settings", 0, data);
    }

    /// <summary>从存档加载设置。</summary>
    public void Load()
    {
        if (_saveModule == null) return;
        var data = _saveModule.Load("settings", 0);
        if (data == null) return;
        foreach (var kv in data)
        {
            if (_settings.TryGetValue(kv.Key.AsString(), out var entry))
                entry.Value = kv.Value;
        }
    }
}

/// <summary>设置项条目。</summary>
public class SettingEntry
{
    public string Key;
    public Variant DefaultValue;
    public Variant Value;
    public bool IsDirty;
}
