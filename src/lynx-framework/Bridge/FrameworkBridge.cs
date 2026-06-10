using Godot;
using LynxFramework.Audio;
using LynxFramework.Camera;
using LynxFramework.Core;
using LynxFramework.Data;
using LynxFramework.Entity;
using LynxFramework.Input;
using LynxFramework.Localization;
using LynxFramework.Log;
using LynxFramework.Pool;
using LynxFramework.Procedure;
using LynxFramework.Resource;
using LynxFramework.Save;
using LynxFramework.Scene;
using LynxFramework.Settings;
using LynxFramework.UI;

namespace LynxFramework.Bridge;

/// <summary>
/// GDScript 桥接层。
/// 提供静态方法供 GDScript 调用，无需获取模块实例。
/// GDScript 通过 FrameworkBridge.xxx() 直接访问框架功能。
/// </summary>
public static class FrameworkBridge
{
    // ═══════════════════════════════════════
    // 框架状态
    // ═══════════════════════════════════════

    /// <summary>框架是否就绪。</summary>
    public static bool IsReady => FrameworkEntry.Instance != null;

    // ═══════════════════════════════════════
    // EventBus 桥接
    // ═══════════════════════════════════════

    /// <summary>订阅事件（无参）。返回 handler ID 用于取消订阅。</summary>
    public static int Subscribe(string eventId, Callable callback)
    {
        var bus = FrameworkEntry.Instance?.GetModule<EventBus>();
        if (bus == null) return -1;
        return EventBusBridge.Subscribe(bus, eventId, callback);
    }

    /// <summary>取消订阅。</summary>
    public static void Unsubscribe(string eventId, int handlerId)
    {
        var bus = FrameworkEntry.Instance?.GetModule<EventBus>();
        if (bus == null) return;
        EventBusBridge.Unsubscribe(bus, eventId, handlerId);
    }

    /// <summary>发射事件（无参）。</summary>
    public static void Emit(string eventId)
    {
        FrameworkEntry.Instance?.GetModule<EventBus>()?.Emit(eventId);
    }

    /// <summary>发射事件（带 Variant 载荷）。</summary>
    public static void EmitVariant(string eventId, Variant data)
    {
        FrameworkEntry.Instance?.GetModule<EventBus>()?.Emit(eventId, new VariantEventArg { Data = data });
    }

    /// <summary>订阅事件（带 Variant 载荷）。</summary>
    public static int SubscribeVariant(string eventId, Callable callback)
    {
        var bus = FrameworkEntry.Instance?.GetModule<EventBus>();
        if (bus == null) return -1;
        return EventBusBridge.SubscribeVariant(bus, eventId, callback);
    }

    // ═══════════════════════════════════════
    // Log 桥接
    // ═══════════════════════════════════════

    public static void LogDebug(string message)
        => FrameworkEntry.Instance?.GetModule<LogService>()?.Debug(message);

    public static void LogInfo(string message)
        => FrameworkEntry.Instance?.GetModule<LogService>()?.Info(message);

    public static void LogWarning(string message)
        => FrameworkEntry.Instance?.GetModule<LogService>()?.Warning(message);

    public static void LogError(string message)
        => FrameworkEntry.Instance?.GetModule<LogService>()?.Error(message);

    // ═══════════════════════════════════════
    // Scene 桥接
    // ═══════════════════════════════════════

    /// <summary>加载场景。</summary>
    public static void LoadScene(string scenePath)
        => FrameworkEntry.Instance?.GetModule<SceneModule>()?.LoadSceneAsync(scenePath);

    /// <summary>切换场景。</summary>
    public static void SwitchScene(string scenePath)
        => FrameworkEntry.Instance?.GetModule<SceneModule>()?.SwitchToScene(scenePath);

    /// <summary>返回上一场景。</summary>
    public static void GoBack()
        => FrameworkEntry.Instance?.GetModule<SceneModule>()?.GoBack();

    // ═══════════════════════════════════════
    // UI 桥接
    // ═══════════════════════════════════════

    /// <summary>打开 UI 面板。</summary>
    public static void OpenUI(string uiName)
        => FrameworkEntry.Instance?.GetModule<UIModule>()?.OpenUI(uiName);

    /// <summary>关闭 UI 面板。</summary>
    public static void CloseUI(string uiName)
        => FrameworkEntry.Instance?.GetModule<UIModule>()?.CloseUI(uiName);

    /// <summary>关闭栈顶 UI。</summary>
    public static void CloseTopUI()
        => FrameworkEntry.Instance?.GetModule<UIModule>()?.CloseTopUI();

    /// <summary>关闭所有 UI。</summary>
    public static void CloseAllUI()
        => FrameworkEntry.Instance?.GetModule<UIModule>()?.CloseAllUI();

    /// <summary>UI 是否打开。</summary>
    public static bool IsUIOpen(string uiName)
        => FrameworkEntry.Instance?.GetModule<UIModule>()?.IsUIOpen(uiName) ?? false;

    // ═══════════════════════════════════════
    // Audio 桥接
    // ═══════════════════════════════════════

    /// <summary>播放 BGM。</summary>
    public static void PlayBGM(string path, float fadeIn = 0f)
        => FrameworkEntry.Instance?.GetModule<AudioModule>()?.PlayBGM(path, fadeIn);

    /// <summary>停止 BGM。</summary>
    public static void StopBGM(float fadeOut = 0f)
        => FrameworkEntry.Instance?.GetModule<AudioModule>()?.StopBGM(fadeOut);

    /// <summary>播放 SFX。</summary>
    public static void PlaySFX(string path)
        => FrameworkEntry.Instance?.GetModule<AudioModule>()?.PlaySFX(path);

    /// <summary>播放 2D SFX。</summary>
    public static void PlaySFX2D(string path, Vector2 position)
        => FrameworkEntry.Instance?.GetModule<AudioModule>()?.PlaySFX2D(path, position);

    /// <summary>播放 3D SFX。</summary>
    public static void PlaySFX3D(string path, Vector3 position)
        => FrameworkEntry.Instance?.GetModule<AudioModule>()?.PlaySFX3D(path, position);

    /// <summary>设置音量。</summary>
    public static void SetVolume(string busName, float volumeDb)
        => FrameworkEntry.Instance?.GetModule<AudioModule>()?.SetBusVolume(busName, volumeDb);

    // ═══════════════════════════════════════
    // Input 桥接
    // ═══════════════════════════════════════

    /// <summary>动作是否按下。</summary>
    public static bool IsActionPressed(string action)
        => FrameworkEntry.Instance?.GetModule<InputModule>()?.IsActionPressed(action) ?? false;

    /// <summary>动作是否刚按下。</summary>
    public static bool IsActionJustPressed(string action)
        => FrameworkEntry.Instance?.GetModule<InputModule>()?.IsActionJustPressed(action) ?? false;

    /// <summary>动作是否刚释放。</summary>
    public static bool IsActionJustReleased(string action)
        => FrameworkEntry.Instance?.GetModule<InputModule>()?.IsActionJustReleased(action) ?? false;

    /// <summary>获取方向向量。</summary>
    public static Vector2 GetInputVector(string negX, string posX, string negY, string posY)
        => FrameworkEntry.Instance?.GetModule<InputModule>()?.GetVector(negX, posX, negY, posY) ?? Vector2.Zero;

    /// <summary>禁用动作。</summary>
    public static void DisableAction(string action)
        => FrameworkEntry.Instance?.GetModule<InputModule>()?.DisableAction(action);

    /// <summary>启用动作。</summary>
    public static void EnableAction(string action)
        => FrameworkEntry.Instance?.GetModule<InputModule>()?.EnableAction(action);

    /// <summary>缓冲动作。</summary>
    public static void BufferAction(string action, double maxTimeSec = 0.2)
        => FrameworkEntry.Instance?.GetModule<InputBufferModule>()?.BufferAction(action, maxTimeSec);

    /// <summary>消费缓冲动作。</summary>
    public static bool ConsumeAction(string action)
        => FrameworkEntry.Instance?.GetModule<InputBufferModule>()?.ConsumeAction(action) ?? false;

    // ═══════════════════════════════════════
    // Camera 桥接
    // ═══════════════════════════════════════

    /// <summary>设置跟随目标。</summary>
    public static void FollowTarget(Node target, float smoothing = 5f)
        => FrameworkEntry.Instance?.GetModule<CameraModule>()?.FollowTarget(target, smoothing);

    /// <summary>清除跟随。</summary>
    public static void ClearCameraTarget()
        => FrameworkEntry.Instance?.GetModule<CameraModule>()?.ClearTarget();

    /// <summary>摄像机震动。</summary>
    public static void CameraShake(float trauma, float duration, float decay = 5f)
        => FrameworkEntry.Instance?.GetModule<CameraModule>()?.Shake(trauma, duration, decay);

    // ═══════════════════════════════════════
    // Procedure 桥接
    // ═══════════════════════════════════════

    /// <summary>切换流程。</summary>
    public static void SetProcedure(string procName)
        => FrameworkEntry.Instance?.GetModule<ProcedureModule>()?.SetCurrentProcedure(procName);

    /// <summary>获取当前流程名。</summary>
    public static string GetCurrentProcedure()
        => FrameworkEntry.Instance?.GetModule<ProcedureModule>()?.GetCurrentProcedureName() ?? "";

    // ═══════════════════════════════════════
    // Save 桥接
    // ═══════════════════════════════════════

    /// <summary>保存数据。</summary>
    public static void SaveData(string slotKey, int slotIndex, Godot.Collections.Dictionary data)
        => FrameworkEntry.Instance?.GetModule<SaveModule>()?.Save(slotKey, slotIndex, data);

    /// <summary>加载数据。</summary>
    public static Godot.Collections.Dictionary LoadData(string slotKey, int slotIndex)
        => FrameworkEntry.Instance?.GetModule<SaveModule>()?.Load(slotKey, slotIndex);

    /// <summary>删除存档。</summary>
    public static void DeleteSave(string slotKey, int slotIndex)
        => FrameworkEntry.Instance?.GetModule<SaveModule>()?.Delete(slotKey, slotIndex);

    /// <summary>存档是否存在。</summary>
    public static bool SaveExists(string slotKey, int slotIndex)
        => FrameworkEntry.Instance?.GetModule<SaveModule>()?.SaveExists(slotKey, slotIndex) ?? false;

    // ═══════════════════════════════════════
    // Settings 桥接
    // ═══════════════════════════════════════

    /// <summary>获取设置值。</summary>
    public static Variant GetSetting(string key)
        => FrameworkEntry.Instance?.GetModule<GameSettingsModule>()?.GetSetting(key) ?? default;

    /// <summary>设置值。</summary>
    public static void SetSetting(string key, Variant value)
        => FrameworkEntry.Instance?.GetModule<GameSettingsModule>()?.SetSetting(key, value);

    /// <summary>注册设置项。</summary>
    public static void RegisterSetting(string key, Variant defaultValue)
        => FrameworkEntry.Instance?.GetModule<GameSettingsModule>()?.RegisterSetting(key, defaultValue);

    // ═══════════════════════════════════════
    // Localization 桥接
    // ═══════════════════════════════════════

    /// <summary>翻译 key。</summary>
    public static string Tr(string key)
        => FrameworkEntry.Instance?.GetModule<LocalizationModule>()?.GetText(key) ?? key;

    /// <summary>切换语言。</summary>
    public static void SetLanguage(string lang)
        => FrameworkEntry.Instance?.GetModule<LocalizationModule>()?.SetLanguage(lang);

    /// <summary>获取当前语言。</summary>
    public static string GetLanguage()
        => FrameworkEntry.Instance?.GetModule<LocalizationModule>()?.GetLanguage() ?? "en";

    // ═══════════════════════════════════════
    // Data 桥接
    // ═══════════════════════════════════════

    /// <summary>获取数据行。</summary>
    public static Godot.Collections.Dictionary GetDataRow(string tableName, string id)
        => FrameworkEntry.Instance?.GetModule<DataTableModule>()?.GetRow(tableName, id);

    /// <summary>加载数据表。</summary>
    public static void LoadTable(string tableName)
        => FrameworkEntry.Instance?.GetModule<DataTableModule>()?.LoadTable(tableName);

    // ═══════════════════════════════════════
    // Entity 桥接
    // ═══════════════════════════════════════

    /// <summary>生成实体。</summary>
    public static Node SpawnEntity(string entityName, Node parent, Vector3 position = default)
        => FrameworkEntry.Instance?.GetModule<EntityModule>()?.SpawnEntity(entityName, parent, position);

    /// <summary>销毁实体。</summary>
    public static void DestroyEntity(Node entity)
        => FrameworkEntry.Instance?.GetModule<EntityModule>()?.DestroyEntity(entity);
}
