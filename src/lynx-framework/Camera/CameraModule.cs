using Godot;
using LynxFramework.Core;
using LynxFramework.Log;

namespace LynxFramework.Camera;

/// <summary>
/// 摄像机管理模块。
/// 支持目标跟随、震动效果、摄像机混合切换。
/// </summary>
public class CameraModule : IModule
{
    private EventBus _eventBus;
    private LogService _logService;
    private UpdateDriver _updateDriver;
    private Camera2D _camera2D;
    private Camera3D _camera3D;
    private Node _currentTarget;
    private float _smoothingSpeed = 5f;
    private float _shakeTrauma;
    private float _shakeDecay = 5f;
    private double _shakeEndTime;
    private UpdateEntry _updateEntry;

    public int Priority => 140;

    public void OnInit()
    {
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
        _updateDriver = FrameworkEntry.Instance.GetModule<UpdateDriver>();
        _updateEntry = _updateDriver.Register(UpdatePhase.Process, -100, OnUpdate);
    }

    public void OnShutdown()
    {
        if (_updateEntry != null)
            _updateDriver.Unregister(_updateEntry);
    }

    /// <summary>设置活跃的 2D 摄像机。</summary>
    public void SetActiveCamera(Camera2D camera) => _camera2D = camera;

    /// <summary>设置活跃的 3D 摄像机。</summary>
    public void SetActiveCamera(Camera3D camera) => _camera3D = camera;

    /// <summary>获取当前 2D 摄像机。</summary>
    public Camera2D GetCamera2D() => _camera2D;

    /// <summary>获取当前 3D 摄像机。</summary>
    public Camera3D GetCamera3D() => _camera3D;

    /// <summary>设置跟随目标。</summary>
    public void FollowTarget(Node target, float smoothingSpeed = 5f)
    {
        _currentTarget = target;
        _smoothingSpeed = smoothingSpeed;
    }

    /// <summary>清除跟随目标。</summary>
    public void ClearTarget()
    {
        _currentTarget = null;
    }

    /// <summary>触发摄像机震动。</summary>
    public void Shake(float trauma, float duration, float decay = 5f)
    {
        _shakeTrauma = trauma;
        _shakeDecay = decay;
        _shakeEndTime = Time.GetTicksMsec() / 1000.0 + duration;
    }

    /// <summary>切换到新的 2D 摄像机。</summary>
    public void BlendTo(Camera2D camera, float time = 0f)
    {
        if (_camera2D != null) _camera2D.Enabled = false;
        _camera2D = camera;
        _camera2D.Enabled = true;
    }

    /// <summary>切换到新的 3D 摄像机。</summary>
    public void BlendTo(Camera3D camera, float time = 0f)
    {
        if (_camera3D != null) _camera3D.Current = false;
        _camera3D = camera;
        _camera3D.Current = true;
    }

    private void OnUpdate(float delta)
    {
        // 跟随目标
        if (_currentTarget != null)
        {
            if (_camera2D != null && _currentTarget is Node2D target2D)
            {
                var targetPos = target2D.GlobalPosition;
                _camera2D.Position = _camera2D.Position.Lerp(targetPos, _smoothingSpeed * delta);
            }
            else if (_camera3D != null && _currentTarget is Node3D target3D)
            {
                var targetPos = target3D.GlobalPosition;
                _camera3D.Position = _camera3D.Position.Lerp(targetPos, _smoothingSpeed * delta);
            }
        }

        // 摄像机震动
        if (_shakeTrauma > 0)
        {
            var now = Time.GetTicksMsec() / 1000.0;
            if (now > _shakeEndTime)
            {
                _shakeTrauma = 0;
                ApplyShakeOffset(Vector2.Zero);
            }
            else
            {
                _shakeTrauma = Mathf.Max(0, _shakeTrauma - _shakeDecay * delta);
                var offsetX = _shakeTrauma * _shakeTrauma * (GD.Randf() * 2 - 1) * 10f;
                var offsetY = _shakeTrauma * _shakeTrauma * (GD.Randf() * 2 - 1) * 10f;
                ApplyShakeOffset(new Vector2(offsetX, offsetY));
            }
        }
    }

    private void ApplyShakeOffset(Vector2 offset)
    {
        if (_camera2D != null)
            _camera2D.Offset = offset;
        if (_camera3D != null)
        {
            _camera3D.HOffset = offset.X;
            _camera3D.VOffset = offset.Y;
        }
    }
}
