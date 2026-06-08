using Godot;
using LynxFramework;
using LynxFramework.Pool;

/// <summary>
/// 子弹实体。通过对象池管理。
/// </summary>
public partial class Bullet : Area3D, IPoolableObject
{
    [Export] public float Speed = 15f;
    [Export] public float Lifetime = 3f;

    private float _timer;

    public override void _PhysicsProcess(double delta)
    {
        Position += -GlobalTransform.Basis.Z * Speed * (float)delta;

        _timer += (float)delta;
        if (_timer >= Lifetime)
            Recycle();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(1);
            Recycle();
        }
    }

    private void Recycle()
    {
        _timer = 0;
        FrameworkEntry.Instance.GetModule<PoolManager>().RecycleNode(this);
    }

    // IPoolableObject 接口
    public void OnGet()
    {
        _timer = 0;
        Visible = true;
        SetPhysicsProcess(true);
    }

    public void OnRecycle()
    {
        Visible = false;
        SetPhysicsProcess(false);
    }
}
