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
	return FrameworkEntry.IsReady

# ═══════════════════════════════════════
# 日志
# ═══════════════════════════════════════

static func log_debug(msg: String) -> void:
	FrameworkEntry.LogDebug(msg)

static func log_info(msg: String) -> void:
	FrameworkEntry.LogInfo(msg)

static func log_warning(msg: String) -> void:
	FrameworkEntry.LogWarning(msg)

static func log_error(msg: String) -> void:
	FrameworkEntry.LogError(msg)

# ═══════════════════════════════════════
# 事件
# ═══════════════════════════════════════

## 订阅无参事件。返回 handler ID，用于取消订阅。
static func subscribe(event_id: String, callback: Callable) -> int:
	return FrameworkEntry.Subscribe(event_id, callback)

## 订阅带数据事件。数据通过 Variant 传递。
static func subscribe_variant(event_id: String, callback: Callable) -> int:
	return FrameworkEntry.SubscribeVariant(event_id, callback)

## 取消订阅。
static func unsubscribe(event_id: String, handler_id: int) -> void:
	FrameworkEntry.Unsubscribe(event_id, handler_id)

## 发射无参事件。
static func emit(event_id: String) -> void:
	FrameworkEntry.Emit(event_id)

## 发射带数据事件。
static func emit_variant(event_id: String, data: Variant) -> void:
	FrameworkEntry.EmitVariant(event_id, data)

# ═══════════════════════════════════════
# 场景
# ═══════════════════════════════════════

static func load_scene(path: String) -> void:
	FrameworkEntry.LoadScene(path)

static func switch_scene(path: String) -> void:
	FrameworkEntry.SwitchScene(path)

static func go_back() -> void:
	FrameworkEntry.GoBack()

# ═══════════════════════════════════════
# UI
# ═══════════════════════════════════════

static func open_ui(ui_name: String) -> void:
	FrameworkEntry.OpenUI(ui_name)

static func close_ui(ui_name: String) -> void:
	FrameworkEntry.CloseUI(ui_name)

static func close_top_ui() -> void:
	FrameworkEntry.CloseTopUI()

static func close_all_ui() -> void:
	FrameworkEntry.CloseAllUI()

static func is_ui_open(ui_name: String) -> bool:
	return FrameworkEntry.IsUIOpen(ui_name)

# ═══════════════════════════════════════
# 音频
# ═══════════════════════════════════════

static func play_bgm(path: String, fade_in: float = 0.0) -> void:
	FrameworkEntry.PlayBGM(path, fade_in)

static func stop_bgm(fade_out: float = 0.0) -> void:
	FrameworkEntry.StopBGM(fade_out)

static func play_sfx(path: String) -> void:
	FrameworkEntry.PlaySFX(path)

static func play_sfx_2d(path: String, pos: Vector2) -> void:
	FrameworkEntry.PlaySFX2D(path, pos)

static func play_sfx_3d(path: String, pos: Vector3) -> void:
	FrameworkEntry.PlaySFX3D(path, pos)

static func set_volume(bus_name: String, db: float) -> void:
	FrameworkEntry.SetVolume(bus_name, db)

# ═══════════════════════════════════════
# 输入
# ═══════════════════════════════════════

static func is_action_pressed(action: String) -> bool:
	return FrameworkEntry.IsActionPressed(action)

static func is_action_just_pressed(action: String) -> bool:
	return FrameworkEntry.IsActionJustPressed(action)

static func is_action_just_released(action: String) -> bool:
	return FrameworkEntry.IsActionJustReleased(action)

static func get_input_vector(neg_x: String, pos_x: String, neg_y: String, pos_y: String) -> Vector2:
	return FrameworkEntry.GetInputVector(neg_x, pos_x, neg_y, pos_y)

static func disable_action(action: String) -> void:
	FrameworkEntry.DisableAction(action)

static func enable_action(action: String) -> void:
	FrameworkEntry.EnableAction(action)

static func buffer_action(action: String, max_time: float = 0.2) -> void:
	FrameworkEntry.BufferAction(action, max_time)

static func consume_action(action: String) -> bool:
	return FrameworkEntry.ConsumeAction(action)

# ═══════════════════════════════════════
# 摄像机
# ═══════════════════════════════════════

static func follow_target(target: Node, smoothing: float = 5.0) -> void:
	FrameworkEntry.FollowTarget(target, smoothing)

static func clear_camera_target() -> void:
	FrameworkEntry.ClearCameraTarget()

static func camera_shake(trauma: float, duration: float, decay: float = 5.0) -> void:
	FrameworkEntry.CameraShake(trauma, duration, decay)

# ═══════════════════════════════════════
# 流程
# ═══════════════════════════════════════

static func set_procedure(proc_name: String) -> void:
	FrameworkEntry.SetProcedure(proc_name)

static func get_current_procedure() -> String:
	return FrameworkEntry.GetCurrentProcedure()

# ═══════════════════════════════════════
# 存档
# ═══════════════════════════════════════

static func save_data(slot_key: String, slot_index: int, data: Dictionary) -> void:
	FrameworkEntry.SaveData(slot_key, slot_index, data)

static func load_data(slot_key: String, slot_index: int) -> Dictionary:
	return FrameworkEntry.LoadData(slot_key, slot_index)

static func delete_save(slot_key: String, slot_index: int) -> void:
	FrameworkEntry.DeleteSave(slot_key, slot_index)

static func save_exists(slot_key: String, slot_index: int) -> bool:
	return FrameworkEntry.SaveExists(slot_key, slot_index)

# ═══════════════════════════════════════
# 设置
# ═══════════════════════════════════════

static func get_setting(key: String) -> Variant:
	return FrameworkEntry.GetSetting(key)

static func set_setting(key: String, value: Variant) -> void:
	FrameworkEntry.SetSetting(key, value)

static func register_setting(key: String, default_value: Variant) -> void:
	FrameworkEntry.RegisterSetting(key, default_value)

# ═══════════════════════════════════════
# 本地化
# ═══════════════════════════════════════

static func translate(key: String) -> String:
	return FrameworkEntry.Tr(key)

static func set_language(lang: String) -> void:
	FrameworkEntry.SetLanguage(lang)

static func get_language() -> String:
	return FrameworkEntry.GetLanguage()

# ═══════════════════════════════════════
# 数据表
# ═══════════════════════════════════════

static func get_data_row(table: String, id: String) -> Dictionary:
	return FrameworkEntry.GetDataRow(table, id)

static func load_table(table_name: String) -> void:
	FrameworkEntry.LoadTable(table_name)

# ═══════════════════════════════════════
# 实体
# ═══════════════════════════════════════

static func spawn_entity(entity_name: String, parent: Node, pos: Vector3 = Vector3.ZERO) -> Node:
	return FrameworkEntry.SpawnEntity(entity_name, parent, pos)

static func destroy_entity(entity: Node) -> void:
	FrameworkEntry.DestroyEntity(entity)
