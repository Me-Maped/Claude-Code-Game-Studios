using Godot;
using LynxFramework;
using LynxFramework.Camera;
using LynxFramework.Core;
using LynxFramework.Entity;
using LynxFramework.Input;

/// <summary>
/// 玩家实体。
/// </summary>
public partial class Player : CharacterBody3D, IEntity
{
    public string EntityName => "Player";

    [Export] public float MoveSpeed = 5f;
    [Export] public float JumpVelocity = 4.5f;

    private InputModule _inputModule;
    private InputBufferModule _inputBuffer;
    private CameraModule _cameraModule;
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public void OnInit(EntityInitData data)
    {
        _inputModule = FrameworkEntry.Instance.GetModule<InputModule>();
        _inputBuffer = FrameworkEntry.Instance.GetModule<InputBufferModule>();
        _cameraModule = FrameworkEntry.Instance.GetModule<CameraModule>();

        _cameraModule.FollowTarget(this, 8f);
    }

    public void OnDeinit()
    {
        _cameraModule.ClearTarget();
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;
        var dt = (float)delta;

        // 重力
        if (!IsOnFloor())
            velocity.Y -= _gravity * dt;

        // 跳跃输入缓冲
        if (_inputBuffer.ConsumeAction("jump") && IsOnFloor())
            velocity.Y = JumpVelocity;

        // 移动
        var inputDir = _inputModule.GetVector("move_left", "move_right", "move_forward", "move_back");
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * MoveSpeed;
            velocity.Z = direction.Z * MoveSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, MoveSpeed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        // 缓冲跳跃输入
        if (@event.IsActionPressed("jump"))
            _inputBuffer.BufferAction("jump", 0.15);

        // 缓冲攻击输入
        if (@event.IsActionPressed("attack"))
            _inputBuffer.BufferAction("attack", 0.2);
    }

    // IEntity 接口（对象池复用时调用）
    public void OnGet() { }
    public void OnRecycle() { }
}
