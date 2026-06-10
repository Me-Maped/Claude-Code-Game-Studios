## GDScript 主菜单示例
## 展示 UI 流程、存档、设置、本地化的 GDScript 用法

extends Control

@onready var start_button: Button = $VBoxContainer/StartButton
@onready var continue_button: Button = $VBoxContainer/ContinueButton
@onready var settings_button: Button = $VBoxContainer/SettingsButton
@onready var quit_button: Button = $VBoxContainer/QuitButton
@onready var title_label: Label = $TitleLabel

func _ready() -> void:
	# 翻译标题
	title_label.text = FrameworkAPI.tr("MENU_TITLE")

	# 检查存档，决定是否显示"继续"按钮
	continue_button.visible = FrameworkAPI.save_exists("game", 0)

	# 连接按钮信号
	start_button.pressed.connect(_on_start_pressed)
	continue_button.pressed.connect(_on_continue_pressed)
	settings_button.pressed.connect(_on_settings_pressed)
	quit_button.pressed.connect(_on_quit_pressed)

	# 播放菜单 BGM
	FrameworkAPI.play_bgm("res://audio/bgm/menu.ogg", 1.0)

func _on_start_pressed() -> void:
	# 新游戏：清除存档，切换到游戏流程
	FrameworkAPI.play_sfx("res://audio/sfx/click.wav")
	FrameworkAPI.set_procedure("Gameplay")

func _on_continue_pressed() -> void:
	# 继续：加载存档
	FrameworkAPI.play_sfx("res://audio/sfx/click.wav")
	var data := FrameworkAPI.load_data("game", 0)
	FrameworkAPI.log_info("Loaded save: %s" % data)
	FrameworkAPI.set_procedure("Gameplay")

func _on_settings_pressed() -> void:
	FrameworkAPI.play_sfx("res://audio/sfx/click.wav")
	FrameworkAPI.open_ui("SettingsPanel")

func _on_quit_pressed() -> void:
	FrameworkAPI.play_sfx("res://audio/sfx/click.wav")
	get_tree().quit()
