using Godot;
using LynxFramework;
using LynxFramework.Core;
using LynxFramework.Entity;

/// <summary>
/// 敌人实体。实现 IEntity 接口以支持对象池复用。
/// </summary>
public partial class Enemy : CharacterBody3D, IEntity
{
    public string EntityName => "Enemy";

    [Export] public float MoveSpeed = 3f;
    [Export] public int Health = 3;

    private Node3D _target;
    private EventBus _eventBus;

    public void OnInit(EntityInitData data)
    {
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        // 查找玩家作为目标
        _target = GetTree().GetFirstNodeInGroup("Player") as Node3D;
    }

    public void OnDeinit()
    {
        _target = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null) return;

        var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
        direction.Y = 0;

        Velocity = direction * MoveSpeed;
        MoveAndSlide();
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            _eventBus?.Emit("enemy_killed");
            Die();
        }
    }

    private void Die()
    {
        // 回收到对象池
        FrameworkEntry.Instance.GetModule<EntityModule>().DestroyEntity(this);
    }

    // IEntity 接口
    public void OnGet()
    {
        Health = 3;
        Visible = true;
    }

    public void OnRecycle()
    {
        Visible = false;
    }
}
