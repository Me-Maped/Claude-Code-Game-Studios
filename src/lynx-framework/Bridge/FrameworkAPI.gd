## LynxFramework GDScript API
## GDScript 业务层通过此单例访问框架功能。
## 用法: FrameworkAPI.log_info("hello")
##       FrameworkAPI.play_bgm("res://audio/bgm.ogg")
##       var id = FrameworkAPI.subscribe("event_name", _on_event)

class_name FrameworkAPI
extends RefCounted

# ═══════════════════════════════════════
# 框架状态
# ═══════════════════════════════════════

static func is_ready() -> bool:
	return FrameworkBridge.IsReady

# ═══════════════════════════════════════
# 日志
# ═══════════════════════════════════════

static func log_debug(msg: String) -> void:
	FrameworkBridge.LogDebug(msg)

static func log_info(msg: String) -> void:
	FrameworkBridge.LogInfo(msg)

static func log_warning(msg: String) -> void:
	FrameworkBridge.LogWarning(msg)

static func log_error(msg: String) -> void:
	FrameworkBridge.LogError(msg)

# ═══════════════════════════════════════
# 事件
# ═══════════════════════════════════════

## 订阅无参事件。返回 handler ID，用于取消订阅。
static func subscribe(event_id: String, callback: Callable) -> int:
	return FrameworkBridge.Subscribe(event_id, callback)

## 订阅带数据事件。数据通过 Variant 传递。
static func subscribe_variant(event_id: String, callback: Callable) -> int:
	return FrameworkBridge.SubscribeVariant(event_id, callback)

## 取消订阅。
static func unsubscribe(event_id: String, handler_id: int) -> void:
	FrameworkBridge.Unsubscribe(event_id, handler_id)

## 发射无参事件。
static func emit(event_id: String) -> void:
	FrameworkBridge.Emit(event_id)

## 发射带数据事件。
static func emit_variant(event_id: String, data: Variant) -> void:
	FrameworkBridge.EmitVariant(event_id, data)

# ═══════════════════════════════════════
# 场景
# ═══════════════════════════════════════

static func load_scene(path: String) -> void:
	FrameworkBridge.LoadScene(path)

static func switch_scene(path: String) -> void:
	FrameworkBridge.SwitchScene(path)

static func go_back() -> void:
	FrameworkBridge.GoBack()

# ═══════════════════════════════════════
# UI
# ═══════════════════════════════════════

static func open_ui(ui_name: String) -> void:
	FrameworkBridge.OpenUI(ui_name)

static func close_ui(ui_name: String) -> void:
	FrameworkBridge.CloseUI(ui_name)

static func close_top_ui() -> void:
	FrameworkBridge.CloseTopUI()

static func close_all_ui() -> void:
	FrameworkBridge.CloseAllUI()

static func is_ui_open(ui_name: String) -> bool:
	return FrameworkBridge.IsUIOpen(ui_name)

# ═══════════════════════════════════════
# 音频
# ═══════════════════════════════════════

static func play_bgm(path: String, fade_in: float = 0.0) -> void:
	FrameworkBridge.PlayBGM(path, fade_in)

static func stop_bgm(fade_out: float = 0.0) -> void:
	FrameworkBridge.StopBGM(fade_out)

static func play_sfx(path: String) -> void:
	FrameworkBridge.PlaySFX(path)

static func play_sfx_2d(path: String, pos: Vector2) -> void:
	FrameworkBridge.PlaySFX2D(path, pos)

static func play_sfx_3d(path: String, pos: Vector3) -> void:
	FrameworkBridge.PlaySFX3D(path, pos)

static func set_volume(bus_name: String, db: float) -> void:
	FrameworkBridge.SetVolume(bus_name, db)

# ═══════════════════════════════════════
# 输入
# ═══════════════════════════════════════

static func is_action_pressed(action: String) -> bool:
	return FrameworkBridge.IsActionPressed(action)

static func is_action_just_pressed(action: String) -> bool:
	return FrameworkBridge.IsActionJustPressed(action)

static func is_action_just_released(action: String) -> bool:
	return FrameworkBridge.IsActionJustReleased(action)

static func get_input_vector(neg_x: String, pos_x: String, neg_y: String, pos_y: String) -> Vector2:
	return FrameworkBridge.GetInputVector(neg_x, pos_x, neg_y, pos_y)

static func disable_action(action: String) -> void:
	FrameworkBridge.DisableAction(action)

static func enable_action(action: String) -> void:
	FrameworkBridge.EnableAction(action)

static func buffer_action(action: String, max_time: float = 0.2) -> void:
	FrameworkBridge.BufferAction(action, max_time)

static func consume_action(action: String) -> bool:
	return FrameworkBridge.ConsumeAction(action)

# ═══════════════════════════════════════
# 摄像机
# ═══════════════════════════════════════

static func follow_target(target: Node, smoothing: float = 5.0) -> void:
	FrameworkBridge.FollowTarget(target, smoothing)

static func clear_camera_target() -> void:
	FrameworkBridge.ClearCameraTarget()

static func camera_shake(trauma: float, duration: float, decay: float = 5.0) -> void:
	FrameworkBridge.CameraShake(trauma, duration, decay)

# ═══════════════════════════════════════
# 流程
# ═══════════════════════════════════════

static func set_procedure(proc_name: String) -> void:
	FrameworkBridge.SetProcedure(proc_name)

static func get_current_procedure() -> String:
	return FrameworkBridge.GetCurrentProcedure()

# ═══════════════════════════════════════
# 存档
# ═══════════════════════════════════════

static func save_data(slot_key: String, slot_index: int, data: Dictionary) -> void:
	FrameworkBridge.SaveData(slot_key, slot_index, data)

static func load_data(slot_key: String, slot_index: int) -> Dictionary:
	return FrameworkBridge.LoadData(slot_key, slot_index)

static func delete_save(slot_key: String, slot_index: int) -> void:
	FrameworkBridge.DeleteSave(slot_key, slot_index)

static func save_exists(slot_key: String, slot_index: int) -> bool:
	return FrameworkBridge.SaveExists(slot_key, slot_index)

# ═══════════════════════════════════════
# 设置
# ═══════════════════════════════════════

static func get_setting(key: String) -> Variant:
	return FrameworkBridge.GetSetting(key)

static func set_setting(key: String, value: Variant) -> void:
	FrameworkBridge.SetSetting(key, value)

static func register_setting(key: String, default_value: Variant) -> void:
	FrameworkBridge.RegisterSetting(key, default_value)

# ═══════════════════════════════════════
# 本地化
# ═══════════════════════════════════════

static func tr(key: String) -> String:
	return FrameworkBridge.Tr(key)

static func set_language(lang: String) -> void:
	FrameworkBridge.SetLanguage(lang)

static func get_language() -> String:
	return FrameworkBridge.GetLanguage()

# ═══════════════════════════════════════
# 数据表
# ═══════════════════════════════════════

static func get_data_row(table: String, id: String) -> Dictionary:
	return FrameworkBridge.GetDataRow(table, id)

static func load_table(table_name: String) -> void:
	FrameworkBridge.LoadTable(table_name)

# ═══════════════════════════════════════
# 实体
# ═══════════════════════════════════════

static func spawn_entity(entity_name: String, parent: Node, pos: Vector3 = Vector3.ZERO) -> Node:
	return FrameworkBridge.SpawnEntity(entity_name, parent, pos)

static func destroy_entity(entity: Node) -> void:
	FrameworkBridge.DestroyEntity(entity)
