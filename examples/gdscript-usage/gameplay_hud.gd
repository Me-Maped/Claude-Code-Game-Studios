## GDScript HUD 示例
## 展示 IUIPanel 桥接、事件订阅、数据表查询

extends Control

@onready var score_label: Label = $ScoreLabel
@onready var hp_label: Label = $HPBar/Label
@onready var hp_bar: ProgressBar = $HPBar

var _score: int = 0
var _handler_score: int = -1
var _handler_damage: int = -1

func on_open(_param: Variant) -> void:
	# 订阅游戏事件
	_handler_score = FrameworkAPI.subscribe("enemy_killed", _on_enemy_killed)
	_handler_damage = FrameworkAPI.subscribe_variant("player_damaged", _on_player_damaged)

	# 加载数据表
	FrameworkAPI.load_table("enemies")

	_update_display()

func on_close() -> void:
	FrameworkAPI.unsubscribe("enemy_killed", _handler_score)
	FrameworkAPI.unsubscribe("player_damaged", _handler_damage)

func _on_enemy_killed() -> void:
	_score += 10
	score_label.text = "Score: %d" % _score
	FrameworkAPI.play_sfx("res://audio/sfx/score.wav")

func _on_player_damaged(data: Variant) -> void:
	var damage: int = data.get("amount", 0)
	hp_bar.value -= damage
	hp_label.text = "HP: %d" % hp_bar.value

	if hp_bar.value <= 0:
		FrameworkAPI.emit("player_died")

func _update_display() -> void:
	score_label.text = "Score: 0"
	hp_bar.value = 100
	hp_label.text = "HP: 100"

	# 查询数据表示例
	var slime_data := FrameworkAPI.get_data_row("enemies", "slime")
	if not slime_data.is_empty():
		FrameworkAPI.log_info("Slime HP: %d" % slime_data.get("health", 0))
