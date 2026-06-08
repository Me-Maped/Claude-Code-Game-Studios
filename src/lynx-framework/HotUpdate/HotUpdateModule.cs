using System;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Resource;

namespace LynxFramework.HotUpdate;

/// <summary>
/// 热更新模块。
/// 负责版本检测、.pck 下载、应用、回滚。
/// </summary>
public class HotUpdateModule : IModule
{
    private ResourceService _resourceService;
    private LogService _logService;
    private EventBus _eventBus;
    private string _currentVersion = "1.0.0";
    private string _latestVersion;
    private readonly string _downloadPath = "user://hotupdate/";
    private readonly string _backupPath = "user://hotupdate/backup/";
    private readonly string _rollbackFlagPath = "user://hotupdate/rollback_flag";

    public int Priority => 200;

    public void OnInit()
    {
        _resourceService = FrameworkEntry.Instance.GetModule<ResourceService>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();

        DirAccess.MakeDirRecursiveAbsolute(_downloadPath);
        DirAccess.MakeDirRecursiveAbsolute(_backupPath);
    }

    public void OnShutdown() { }

    /// <summary>检查更新。具体实现依赖版本检测策略（可热更）。</summary>
    public void CheckUpdate()
    {
        // TODO: 通过 HTTP 请求获取最新版本信息
        // 如果 _latestVersion > _currentVersion，通知有更新
        _logService?.Info($"Current version: {_currentVersion}");
    }

    /// <summary>下载更新包。</summary>
    public async void DownloadUpdate(Action<float> progress = null)
    {
        // TODO: 下载 .pck 文件到 _downloadPath，支持断点续传
        _logService?.Info("Downloading update...");

        _eventBus?.Emit("hotupdate_download_complete", new HotUpdateEventArg
        {
            Version = _latestVersion
        });
    }

    /// <summary>应用更新。备份当前版本 → 写入回滚标记 → 加载新 .pck → 清除标记。</summary>
    public void ApplyUpdate()
    {
        var pckPath = $"{_downloadPath}patch_{_latestVersion}.pck";

        // 1. 备份当前版本
        BackupCurrentVersion();

        // 2. 写入回滚标记
        WriteRollbackFlag();

        // 3. 加载新 .pck
        var success = _resourceService.SetPatchPack(pckPath);
        if (!success)
        {
            _logService?.Error($"Failed to apply update: {pckPath}");
            RecoverLastVersion();
            return;
        }

        // 4. 清除回滚标记（应用成功）
        ClearRollbackFlag();

        _currentVersion = _latestVersion;
        _eventBus?.Emit("hotupdate_applied", new HotUpdateEventArg { Version = _currentVersion });
        _logService?.Info($"Update applied: {_currentVersion}");
    }

    /// <summary>回滚到上一个版本。</summary>
    public void RecoverLastVersion()
    {
        var backupFile = $"{_backupPath}patch_{_currentVersion}.pck.bak";
        if (FileAccess.FileExists(backupFile))
        {
            _resourceService.SetPatchPack(backupFile);
            _logService?.Info("Rollback to previous version completed.");
        }
        else
        {
            _logService?.Warning("No backup file found for rollback.");
        }
        ClearRollbackFlag();
    }

    /// <summary>获取当前版本号。</summary>
    public string GetCurrentVersion() => _currentVersion;

    /// <summary>获取最新版本号。</summary>
    public string GetLatestVersion() => _latestVersion;

    private void BackupCurrentVersion()
    {
        var currentPck = $"{_downloadPath}patch_{_currentVersion}.pck";
        if (FileAccess.FileExists(currentPck))
        {
            var backupFile = $"{_backupPath}patch_{_currentVersion}.pck.bak";
            DirAccess.CopyAbsolute(currentPck, backupFile);
        }
    }

    private void WriteRollbackFlag()
    {
        var file = FileAccess.Open(_rollbackFlagPath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(_currentVersion);
            file.Close();
        }
    }

    private void ClearRollbackFlag()
    {
        if (FileAccess.FileExists(_rollbackFlagPath))
            DirAccess.RemoveAbsolute(_rollbackFlagPath);
    }
}
