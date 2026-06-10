## GDScript 玩家示例
## 展示如何通过 FrameworkAPI 桥接层访问 LynxFramework。
##
## 架构分工：
##   C# (src/lynx-framework/) → 框架核心，不热更
##   GDScript (scripts/)       → 业务逻辑，可热更

extends CharacterBody3D

@export var move_speed: float = 5.0
@export var jump_velocity: float = 4.5

var _handler_id: int = -1
var gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity")

func _ready() -> void:
	# 摄像机跟随
	FrameworkAPI.follow_target(self, 8.0)

	# 订阅事件
	_handler_id = FrameworkAPI.subscribe("player_heal", _on_heal)

	# 注册设置项
	FrameworkAPI.register_setting("sensitivity", 0.5)

	# 日志
	FrameworkAPI.log_info("Player spawned")

func _exit_tree() -> void:
	FrameworkAPI.unsubscribe("player_heal", _handler_id)
	FrameworkAPI.clear_camera_target()

func _physics_process(delta: float) -> void:
	# 重力
	if not is_on_floor():
		velocity.y -= gravity * delta

	# 跳跃（带输入缓冲）
	if FrameworkAPI.consume_action("jump") and is_on_floor():
		velocity.y = jump_velocity
		FrameworkAPI.play_sfx("res://audio/sfx/jump.wav")

	# 移动
	var input_dir := FrameworkAPI.get_input_vector("move_left", "move_right", "move_forward", "move_back")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()

	if direction != Vector3.ZERO:
		velocity.x = direction.x * move_speed
		velocity.z = direction.z * move_speed
	else:
		velocity.x = move_toward(velocity.x, 0, move_speed)
		velocity.z = move_toward(velocity.z, 0, move_speed)

	move_and_slide()

func _input(event: InputEvent) -> void:
	# 缓冲跳跃输入
	if event.is_action_pressed("jump"):
		FrameworkAPI.buffer_action("jump", 0.15)

	# 缓冲攻击输入
	if event.is_action_pressed("attack"):
		FrameworkAPI.buffer_action("attack", 0.2)
		FrameworkAPI.camera_shake(0.3, 0.1)

func _on_heal() -> void:
	FrameworkAPI.log_info("Player healed!")
	FrameworkAPI.play_sfx("res://audio/sfx/heal.wav")
