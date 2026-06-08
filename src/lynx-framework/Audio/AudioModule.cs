using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Resource;

namespace LynxFramework.Audio;

/// <summary>
/// 音频管理模块。
/// 管理 BGM 播放（支持淡入淡出）、SFX 播放（2D/3D）、音量控制。
/// </summary>
public class AudioModule : IModule
{
    private ResourceService _resourceService;
    private EventBus _eventBus;
    private LogService _logService;
    private AudioStreamPlayer _bgmPlayer;
    private readonly List<AudioStreamPlayer> _sfxPlayers = new();
    private int _sfxPoolSize = 8;
    private int _sfxIndex;

    public int Priority => 100;

    public void OnInit()
    {
        _resourceService = FrameworkEntry.Instance.GetModule<ResourceService>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();

        // 创建 BGM 播放器
        _bgmPlayer = new AudioStreamPlayer { Name = "BGMPlayer", Bus = "BGM" };
        FrameworkEntry.Instance.AddChild(_bgmPlayer);

        // 创建 SFX 播放器池
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            var player = new AudioStreamPlayer { Name = $"SFXPlayer_{i}", Bus = "SFX" };
            FrameworkEntry.Instance.AddChild(player);
            _sfxPlayers.Add(player);
        }
    }

    public void OnShutdown()
    {
        _bgmPlayer?.QueueFree();
        foreach (var p in _sfxPlayers) p.QueueFree();
        _sfxPlayers.Clear();
    }

    /// <summary>播放背景音乐。支持淡入。</summary>
    public void PlayBGM(string streamPath, float fadeIn = 0f)
    {
        var stream = _resourceService.LoadCached<AudioStream>(streamPath);
        if (stream == null)
        {
            _logService?.Warning($"BGM not found: {streamPath}");
            return;
        }

        if (fadeIn > 0)
        {
            _bgmPlayer.VolumeDb = -80f;
            var tween = _bgmPlayer.CreateTween();
            tween.TweenProperty(_bgmPlayer, "volume_db", 0f, fadeIn);
        }
        else
        {
            _bgmPlayer.VolumeDb = 0f;
        }

        _bgmPlayer.Stream = stream;
        _bgmPlayer.Play();
        _logService?.Info($"BGM playing: {streamPath}");
    }

    /// <summary>停止背景音乐。支持淡出。</summary>
    public void StopBGM(float fadeOut = 0f)
    {
        if (fadeOut > 0)
        {
            var tween = _bgmPlayer.CreateTween();
            tween.TweenProperty(_bgmPlayer, "volume_db", -80f, fadeOut);
            tween.TweenCallback(Callable.From(() => _bgmPlayer.Stop()));
        }
        else
        {
            _bgmPlayer.Stop();
        }
    }

    /// <summary>播放 2D 音效。创建临时 AudioStreamPlayer2D，播放完自动释放。</summary>
    public void PlaySFX2D(string streamPath, Vector2 position = default)
    {
        var stream = _resourceService.LoadCached<AudioStream>(streamPath);
        if (stream == null) return;

        var player2D = new AudioStreamPlayer2D
        {
            Position = position,
            Stream = stream,
            Bus = "SFX"
        };
        FrameworkEntry.Instance.AddChild(player2D);
        player2D.Play();
        player2D.Finished += () => player2D.QueueFree();
    }

    /// <summary>播放 3D 音效。创建临时 AudioStreamPlayer3D，播放完自动释放。</summary>
    public void PlaySFX3D(string streamPath, Vector3 position = default)
    {
        var stream = _resourceService.LoadCached<AudioStream>(streamPath);
        if (stream == null) return;

        var player3D = new AudioStreamPlayer3D
        {
            Position = position,
            Stream = stream,
            Bus = "SFX"
        };
        FrameworkEntry.Instance.AddChild(player3D);
        player3D.Play();
        player3D.Finished += () => player3D.QueueFree();
    }

    /// <summary>使用池中的 SFX 播放器播放音效（避免创建临时节点）。</summary>
    public void PlaySFX(string streamPath)
    {
        var stream = _resourceService.LoadCached<AudioStream>(streamPath);
        if (stream == null) return;

        var player = _sfxPlayers[_sfxIndex];
        _sfxIndex = (_sfxIndex + 1) % _sfxPoolSize;

        player.Stream = stream;
        player.Play();
    }

    /// <summary>设置音频总线音量。</summary>
    public void SetBusVolume(string busName, float volumeDb)
    {
        var busIndex = AudioServer.GetBusIndex(busName);
        if (busIndex >= 0)
            AudioServer.SetBusVolumeDb(busIndex, volumeDb);
    }

    /// <summary>获取音频总线音量。</summary>
    public float GetBusVolume(string busName)
    {
        var busIndex = AudioServer.GetBusIndex(busName);
        return busIndex >= 0 ? AudioServer.GetBusVolumeDb(busIndex) : 0f;
    }

    /// <summary>暂停 BGM。</summary>
    public void PauseBGM() => _bgmPlayer.StreamPaused = true;

    /// <summary>恢复 BGM。</summary>
    public void ResumeBGM() => _bgmPlayer.StreamPaused = false;
}
