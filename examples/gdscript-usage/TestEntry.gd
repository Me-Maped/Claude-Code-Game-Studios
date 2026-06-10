## GDScript 用例测试入口
## 作为空场景的根节点脚本，依次调用 FrameworkAPI 各分组接口并打印结果。
## 运行方式：将 TestEntry.tscn 设为 Godot 项目的启动场景后运行。

extends Node

func _ready() -> void:
	print("[Test:gdscript-usage] === 开始测试 ===")
	_test_framework_state()
	_test_log()
	_test_events()
	_test_settings()
	_test_localization()
	print("[Test:gdscript-usage] === 测试完成 ===")

# ─── 框架状态 ────────────────────────────────────────
func _test_framework_state() -> void:
	var ready := FrameworkAPI.is_ready()
	print("[Test] is_ready = ", ready)

# ─── 日志 ────────────────────────────────────────────
func _test_log() -> void:
	FrameworkAPI.log_debug("log_debug OK")
	FrameworkAPI.log_info("log_info OK")
	FrameworkAPI.log_warning("log_warning OK")
	FrameworkAPI.log_error("log_error OK")
	print("[Test] log calls passed")

# ─── 事件 ────────────────────────────────────────────
func _test_events() -> void:
	var id := FrameworkAPI.subscribe("test_ping", _on_ping)
	FrameworkAPI.emit("test_ping")
	FrameworkAPI.unsubscribe("test_ping", id)

	var vid := FrameworkAPI.subscribe_variant("test_data", _on_data)
	FrameworkAPI.emit_variant("test_data", {"value": 99})
	FrameworkAPI.unsubscribe("test_data", vid)

	print("[Test] event subscribe/emit/unsubscribe passed")

func _on_ping() -> void:
	print("[Test]   _on_ping received")

func _on_data(payload: Variant) -> void:
	print("[Test]   _on_data received: ", payload)

# ─── 设置 ────────────────────────────────────────────
func _test_settings() -> void:
	FrameworkAPI.register_setting("test_volume", 0.8)
	var val: Variant = FrameworkAPI.get_setting("test_volume")
	print("[Test] get_setting('test_volume') = ", val)
	FrameworkAPI.set_setting("test_volume", 0.5)
	print("[Test] set_setting -> ", FrameworkAPI.get_setting("test_volume"))

# ─── 本地化 ──────────────────────────────────────────
func _test_localization() -> void:
	var lang := FrameworkAPI.get_language()
	print("[Test] get_language = ", lang)
	var text := FrameworkAPI.translate("TEST_KEY")
	print("[Test] translate('TEST_KEY') = ", text)
