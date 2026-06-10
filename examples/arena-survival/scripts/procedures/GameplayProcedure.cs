using Godot;
using LynxFramework;
using LynxFramework.Audio;
using LynxFramework.Camera;
using LynxFramework.Core;
using LynxFramework.Data;
using LynxFramework.Entity;
using LynxFramework.Input;
using LynxFramework.Pool;
using LynxFramework.Procedure;
using LynxFramework.Scene;

/// <summary>
/// 游戏流程。管理游戏内所有逻辑。
/// </summary>
public class GameplayProcedure : IProcedure
{
    private EventBus _eventBus;
    private EntityModule _entityModule;
    private InputModule _inputModule;
    private CameraModule _cameraModule;
    private AudioModule _audioModule;
    private PoolManager _poolManager;

    private Node _player;
    private float _spawnTimer;
    private float _spawnInterval = 2f;
    private int _score;

    public void OnEnter()
    {
        var fw = FrameworkEntry.Instance;

        // 获取模块引用
        _eventBus = fw.GetModule<EventBus>();
        _entityModule = fw.GetModule<EntityModule>();
        _inputModule = fw.GetModule<InputModule>();
        _cameraModule = fw.GetModule<CameraModule>();
        _audioModule = fw.GetModule<AudioModule>();
        _poolManager = fw.GetModule<PoolManager>();

        // 创建子弹池
        _poolManager.CreatePool("Bullet", GD.Load<PackedScene>("res://scenes/Bullet.tscn"), 20);

        // 加载竞技场场景
        var scene = fw.GetModule<SceneModule>();
        scene.LoadSceneAsync("res://scenes/Arena.tscn");

        // 生成玩家
        _player = _entityModule.SpawnEntity("Player", fw, Vector3.Zero);
        _cameraModule.FollowTarget(_player, 8f);

        // 订阅事件
        _eventBus.Subscribe("enemy_killed", OnEnemyKilled);
        _eventBus.Subscribe("player_died", OnPlayerDied);

        // 播放 BGM
        _audioModule.PlayBGM("res://audio/bgm/battle_theme.ogg", fadeIn: 0.5f);

        _score = 0;
        _spawnTimer = 0;
    }

    public void OnUpdate(float delta)
    {
        // 敌人生成逻辑
        _spawnTimer += delta;
        if (_spawnTimer >= _spawnInterval)
        {
            SpawnEnemy();
            _spawnTimer = 0;
            _spawnInterval = Mathf.Max(0.5f, _spawnInterval - 0.05f); // 逐渐加速
        }

        // 输入缓冲消费
        var inputBuffer = FrameworkEntry.Instance.GetModule<InputBufferModule>();
        if (inputBuffer.ConsumeAction("attack"))
        {
            FireBullet();
        }
    }

    public void OnLeave()
    {
        _eventBus.Unsubscribe("enemy_killed", OnEnemyKilled);
        _eventBus.Unsubscribe("player_died", OnPlayerDied);

        var fw = FrameworkEntry.Instance;
        fw.GetModule<AudioModule>().StopBGM(fadeOut: 1f);
    }

    private void SpawnEnemy()
    {
        var pos = new Vector3(
            GD.Randf() * 20 - 10,
            0,
            GD.Randf() * 20 - 10
        );
        _entityModule.SpawnEntity("Enemy", FrameworkEntry.Instance, pos);
    }

    private void FireBullet()
    {
        if (_player == null) return;
        var bullet = _poolManager.GetNode("Bullet", FrameworkEntry.Instance);
        if (bullet is Node3D node3D)
        {
            node3D.Position = ((Node3D)_player).Position + Vector3.Forward;
        }
        _audioModule.PlaySFX("res://audio/sfx/shoot.wav");
        _cameraModule.Shake(0.3f, 0.1f);
    }

    private void OnEnemyKilled()
    {
        _score++;
        _audioModule.PlaySFX("res://audio/sfx/enemy_death.wav");
        _cameraModule.Shake(0.5f, 0.2f);
    }

    private void OnPlayerDied()
    {
        var proc = FrameworkEntry.Instance.GetModule<ProcedureModule>();
        proc.SetCurrentProcedure("GameOver");
    }
}
