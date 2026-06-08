# LynxFramework API 参考手册

> **版本**：v0.2 | **引擎**：Godot 4.6.3 / .NET 8 | **生成日期**：2026-06-01

---

## 目录

1. [框架入口](#1-框架入口)
2. [核心基础设施](#2-核心基础设施)
3. [资源与对象管理](#3-资源与对象管理)
4. [场景管理](#4-场景管理)
5. [UI 系统](#5-ui-系统)
6. [流程管理](#6-流程管理)
7. [实体管理](#7-实体管理)
8. [输入系统](#8-输入系统)
9. [摄像机](#9-摄像机)
10. [音频](#10-音频)
11. [数据与配置](#11-数据与配置)
12. [本地化](#12-本地化)
13. [存档系统](#13-存档系统)
14. [网络](#14-网络)
15. [世界流式加载](#15-世界流式加载)
16. [调试](#16-调试)
17. [热更新](#17-热更新)
18. [事件系统](#18-事件系统)

---

## 1. 框架入口

### FrameworkEntry

**命名空间**：`LynxFramework` | **继承**：`Node` | **Autoload 单例**

框架启动入口，管理所有模块的生命周期。

```csharp
// 获取实例
var fw = FrameworkEntry.Instance;

// 获取模块
var eventBus = fw.GetModule<EventBus>();
var log = fw.GetModule<LogService>();

// 获取所有模块（按优先级排序）
IReadOnlyList<IModule> modules = fw.GetAllModules();

// 关闭框架
fw.Shutdown();
```

**信号**：
| 信号 | 参数 | 说明 |
|------|------|------|
| `FrameworkReady` | 无 | 框架初始化完成 |

**方法**：
| 方法 | 返回 | 说明 |
|------|------|------|
| `GetModule<T>()` | `T` | 获取指定类型的模块实例 |
| `GetAllModules()` | `IReadOnlyList<IModule>` | 获取所有已注册模块 |
| `Shutdown()` | `void` | 关闭框架（逆序关闭所有模块） |

---

## 2. 核心基础设施

### IModule

**命名空间**：`LynxFramework`

所有模块必须实现的接口。

```csharp
public interface IModule
{
    int Priority { get; }      // 初始化优先级，值越小越先初始化
    void OnInit();             // 模块初始化
    void OnShutdown();         // 模块关闭
}
```

### EventBus

**命名空间**：`LynxFramework.Core` | **优先级**：0

高性能全局事件总线。支持无参和类型安全载荷两种模式。

```csharp
var eventBus = FrameworkEntry.Instance.GetModule<EventBus>();

// 无参事件
eventBus.Subscribe("game_start", () => GD.Print("Started!"));
eventBus.Emit("game_start");
eventBus.Unsubscribe("game_start", handler);

// 类型安全载荷事件
eventBus.Subscribe<DamageEventArg>("on_damage", (arg) =>
{
    GD.Print($"Damage: {arg.Amount}");
});
eventBus.Emit("on_damage", new DamageEventArg { Amount = 50 });

// 清除
eventBus.Clear();           // 清除所有
eventBus.Clear("on_damage"); // 清除指定事件
```

**方法**：
| 方法 | 说明 |
|------|------|
| `Subscribe(string eventId, Action callback)` | 订阅无参事件 |
| `Subscribe<T>(string eventId, Action<T> callback)` | 订阅带载荷事件 |
| `Unsubscribe(...)` | 取消订阅 |
| `Emit(string eventId)` | 发射无参事件 |
| `Emit<T>(string eventId, T arg)` | 发射带载荷事件 |
| `Clear()` / `Clear(string eventId)` | 清除订阅 |

### UpdateDriver

**命名空间**：`LynxFramework.Core` | **优先级**：40

统一主循环调度器。支持 PROCESS / PHYSICS_PROCESS 两种阶段。

```csharp
var updateDriver = FrameworkEntry.Instance.GetModule<UpdateDriver>();

// 注册更新回调
UpdateEntry entry = updateDriver.Register(UpdatePhase.Process, 100, (delta) =>
{
    // 每帧执行
});

// 暂停/恢复
entry.IsActive = false; // 暂停
entry.IsActive = true;  // 恢复

// 注销
updateDriver.Unregister(entry);
```

**枚举**：`UpdatePhase` — `Process` | `PhysicsProcess`

### LogService

**命名空间**：`LynxFramework.Log` | **优先级**：10

日志服务。支持多输出目标和级别过滤。

```csharp
var log = FrameworkEntry.Instance.GetModule<LogService>();

log.Debug("调试信息");
log.Info("普通信息");
log.Warning("警告信息");
log.Error("错误信息");

log.SetMinLevel(LogLevel.Warning); // 过滤 Debug/Info
log.AddOutput(new FileLogOutput()); // 添加文件输出
```

---

## 3. 资源与对象管理

### PoolManager

**命名空间**：`LynxFramework.Pool` | **优先级**：30

对象池管理器。支持节点池和普通对象池。

```csharp
var pool = FrameworkEntry.Instance.GetModule<PoolManager>();

// 节点池
pool.CreatePool("bullet", bulletScene, preloadCount: 20);
Node bullet = pool.GetNode("bullet", parent);
pool.RecycleNode(bullet); // 通过 pool_key 元数据自动定位

// 普通对象池
pool.CreateObjectPool(() => new DamageInfo(), 50);
var info = pool.GetObject<DamageInfo>();
pool.RecycleObject(info);

// 统计
Dictionary<string, PoolStat> stats = pool.GetPoolStats();
```

### ResourceService

**命名空间**：`LynxFramework.Resource` | **优先级**：20

资源加载服务。支持同步/异步加载、弱引用缓存、.pck 热更。

```csharp
var res = FrameworkEntry.Instance.GetModule<ResourceService>();

// 同步加载（带缓存）
var texture = res.LoadCached<Texture2D>("res://sprites/player.png");

// 同步加载（无缓存）
var scene = res.Load<PackedScene>("res://scenes/level1.tscn");

// 异步加载
ResourceRequest request = res.LoadAsync("res://models/hero.glb");
request.ProgressChanged += (progress) => GD.Print($"Loading: {progress:P0}");
request.Completed += (r) => { /* 加载完成 */ };

// .pck 热更
res.SetPatchPack("user://hotupdate/patch_v1.1.pck");

// 缓存管理
res.ReloadCached("res://sprites/player.png");
res.ClearCacheWithKeyPrefix("res://sprites/");
res.ClearAllCache();
```

---

## 4. 场景管理

### SceneModule

**命名空间**：`LynxFramework.Scene` | **优先级**：120

场景管理模块。支持异步加载、场景栈、转场动画。

```csharp
var scene = FrameworkEntry.Instance.GetModule<SceneModule>();

// 异步加载
scene.LoadSceneAsync("res://levels/level2.tscn", (progress) =>
{
    GD.Print($"Loading: {progress:P0}");
});

// 带转场切换
var transition = new FadeTransition(FrameworkEntry.Instance, 0.5f);
scene.SwitchToScene("res://levels/level3.tscn", transition);

// 返回上一个场景
scene.GoBack(transition);

// 查询
Node current = scene.GetCurrentScene();
string path = scene.GetCurrentScenePath();
int depth = scene.StackDepth;
```

### ISceneTransition

转场动画接口。可自定义实现。

```csharp
public interface ISceneTransition
{
    void PlayOut(Action onComplete);  // 退场动画
    void PlayIn(Action onComplete);   // 入场动画
}
```

内置实现：`FadeTransition`

---

## 5. UI 系统

### UIModule

**命名空间**：`LynxFramework.UI` | **优先级**：130

UI 栈管理模块。支持面板生命周期、输入拦截、过渡动画。

```csharp
var ui = FrameworkEntry.Instance.GetModule<UIModule>();

// 打开面板
ui.OpenUI("Inventory", new UIOpenParam { Data = playerData });

// 关闭面板
ui.CloseUI("Inventory");
ui.CloseTopUI();
ui.CloseAllUI();

// 查询
bool isOpen = ui.IsUIOpen("Inventory");
IUIPanel panel = ui.GetUI("Inventory");
int depth = ui.StackDepth;
```

### IUIPanel

UI 面板生命周期接口。

```csharp
public interface IUIPanel
{
    void OnOpen(UIOpenParam param);  // 打开
    void OnClose();                  // 关闭
    void OnPause();                  // 被覆盖
    void OnResume();                 // 恢复栈顶
    void OnCover();                  // 有新面板覆盖
    void OnUncover();                // 上层面板关闭露出
}
```

---

## 6. 流程管理

### ProcedureModule

**命名空间**：`LynxFramework.Procedure` | **优先级**：170

有限状态机，管理游戏流程切换。

```csharp
var proc = FrameworkEntry.Instance.GetModule<ProcedureModule>();

// 注册流程
proc.RegisterProcedure("MainMenu", () => new MainMenuProcedure());
proc.RegisterProcedure("Gameplay", () => new GameplayProcedure());

// 切换流程
proc.SetCurrentProcedure("Gameplay");

// 查询
IProcedure current = proc.GetCurrentProcedure();
string name = proc.GetCurrentProcedureName();
```

### IProcedure

```csharp
public interface IProcedure
{
    void OnEnter();              // 进入
    void OnUpdate(float delta);  // 每帧更新
    void OnLeave();              // 离开
}
```

---

## 7. 实体管理

### EntityModule

**命名空间**：`LynxFramework.Entity` | **优先级**：150

实体管理模块。优先使用对象池复用。

```csharp
var entity = FrameworkEntry.Instance.GetModule<EntityModule>();

// 生成实体（优先从池获取）
Node enemy = entity.SpawnEntity("Enemy", parent, new Vector3(10, 0, 5));

// 销毁实体（回收到池）
entity.DestroyEntity(enemy);

// 分组查询
IReadOnlyList<Node> enemies = entity.GetEntitiesByGroup("Enemy");

// 回收容器下所有实体
entity.RecycleEntitiesInNode(areaNode);
```

---

## 8. 输入系统

### InputModule

**命名空间**：`LynxFramework.Input` | **优先级**：80

输入管理模块。封装 Godot Input API。

```csharp
var input = FrameworkEntry.Instance.GetModule<InputModule>();

// 查询
bool pressed = input.IsActionPressed("jump");
bool justPressed = input.IsActionJustPressed("attack");
Vector2 dir = input.GetVector("left", "right", "up", "down");

// 启用/禁用
input.DisableAction("jump");  // 禁用（如过场动画中）
input.EnableAction("jump");   // 恢复

// 全局控制
input.DisableAll();
input.EnableAll();

// 重映射
input.RemapAction("jump", new InputEventKey { Keycode = Key.Space });
```

### InputBufferModule

**命名空间**：`LynxFramework.Input` | **优先级**：90

输入缓冲。在时间窗口内缓冲玩家输入。

```csharp
var buffer = FrameworkEntry.Instance.GetModule<InputBufferModule>();

// 缓冲动作（0.2 秒窗口）
buffer.BufferAction("attack", 0.2);

// 消费缓冲（成功返回 true）
if (buffer.ConsumeAction("attack"))
{
    // 执行攻击
}

// 查询
bool buffered = buffer.IsActionBuffered("attack");
```

---

## 9. 摄像机

### CameraModule

**命名空间**：`LynxFramework.Camera` | **优先级**：140

摄像机管理。支持跟随、震动、混合切换。

```csharp
var cam = FrameworkEntry.Instance.GetModule<CameraModule>();

// 设置活跃摄像机
cam.SetActiveCamera(playerCamera2D);

// 跟随目标
cam.FollowTarget(playerNode, smoothingSpeed: 5f);
cam.ClearTarget();

// 震动
cam.Shake(trauma: 0.8f, duration: 0.5f, decay: 5f);

// 切换摄像机
cam.SetActiveCamera(otherCamera2D);
```

---

## 10. 音频

### AudioModule

**命名空间**：`LynxFramework.Audio` | **优先级**：100

音频管理。BGM 淡入淡出、SFX 池化播放。

```csharp
var audio = FrameworkEntry.Instance.GetModule<AudioModule>();

// BGM
audio.PlayBGM("res://audio/bgm/main_theme.ogg", fadeIn: 2f);
audio.StopBGM(fadeOut: 1f);
audio.PauseBGM();
audio.ResumeBGM();

// SFX
audio.PlaySFX("res://audio/sfx/hit.wav");           // 池化播放
audio.PlaySFX2D("res://audio/sfx/explode.wav", pos); // 2D 定位
audio.PlaySFX3D("res://audio/sfx/gun.wav", pos3d);   // 3D 定位

// 音量
audio.SetBusVolume("BGM", -10f);
float vol = audio.GetBusVolume("Master");
```

---

## 11. 数据与配置

### DataTableModule

**命名空间**：`LynxFramework.Data` | **优先级**：60

数据表管理。从 JSON 文件加载表格数据。

```csharp
var dt = FrameworkEntry.Instance.GetModule<DataTableModule>();

// 加载表（从 res://data/tables/items.json）
DataTable items = dt.LoadTable("items");

// 查询
var sword = items.GetRow("sword_01");
var allWeapons = items.GetRowsByField("type", "weapon");
IReadOnlyDictionary<string, Godot.Collections.Dictionary> all = items.GetAll();

// 卸载
dt.UnloadTable("items");
```

### GameSettingsModule

**命名空间**：`LynxFramework.Settings` | **优先级**：50

游戏设置管理。

```csharp
var settings = FrameworkEntry.Instance.GetModule<GameSettingsModule>();

// 注册
settings.RegisterSetting("master_volume", 0.8f);
settings.RegisterSetting("fullscreen", false);

// 读写
float vol = settings.GetSetting("master_volume").AsSingle();
settings.SetSetting("master_volume", 0.5f);

// 重置
settings.ResetToDefault("master_volume");
settings.ResetAllToDefault();

// 持久化
settings.Save();
settings.Load();
settings.Apply(); // 应用分辨率、音量等
```

---

## 12. 本地化

### LocalizationModule

**命名空间**：`LynxFramework.Localization` | **优先级**：70

本地化模块。封装 Godot TranslationServer。

```csharp
var loc = FrameworkEntry.Instance.GetModule<LocalizationModule>();

// 切换语言
loc.SetLanguage("zh_CN");

// 翻译
string text = loc.GetText("MENU_START");

// 添加翻译资源
loc.AddTranslationFromResource("res://locale/zh_CN.po");
```

---

## 13. 存档系统

### SaveModule

**命名空间**：`LynxFramework.Save` | **优先级**：180

存档模块。支持版本迁移。

```csharp
var save = FrameworkEntry.Instance.GetModule<SaveModule>();

// 保存
var data = new Godot.Collections.Dictionary
{
    ["player_name"] = "Hero",
    ["level"] = 10
};
save.Save("game", slotIndex: 0, data);

// 加载
Godot.Collections.Dictionary loaded = save.Load("game", 0);

// 删除
save.Delete("game", 0);

// 查询
bool exists = save.SaveExists("game", 0);
List<SaveSlotInfo> slots = save.GetSlotsInfo("game");

// 版本迁移
save.SetCurrentVersion(3);
save.RegisterMigration(new SaveV1ToV2());
save.RegisterMigration(new SaveV2ToV3());
```

### ISaveMigration

```csharp
public interface ISaveMigration
{
    int FromVersion { get; }
    int ToVersion { get; }
    Godot.Collections.Dictionary Migrate(Godot.Collections.Dictionary data);
}
```

---

## 14. 网络

### NetworkModule

**命名空间**：`LynxFramework.Network` | **优先级**：110

TCP 网络模块。心跳、自动重连。

```csharp
var net = FrameworkEntry.Instance.GetModule<NetworkModule>();

// 连接
net.ConnectToServer("127.0.0.1", 8080);
net.SetHeartbeatInterval(30f);
net.SetReconnectInterval(5f);

// 发送
net.SendMessage(msgId: 1, data: myBytes);

// 接收（通过 EventBus）
eventBus.Subscribe<NetworkDataEventArg>("network_data_received", (arg) =>
{
    byte[] data = arg.Data;
});

// 状态
ConnectionState state = net.GetConnectionState();

// 协议
net.SetProtocolHandler(new MyProtobufHandler());

// 断开
net.Disconnect();
```

### IProtocolHandler

```csharp
public interface IProtocolHandler
{
    byte[] Encode(object msgStruct);
    object Decode(byte[] data);
}
```

---

## 15. 世界流式加载

### WorldStreamingModule

**命名空间**：`LynxFramework.Streaming` | **优先级**：160

世界流式加载模块。

```csharp
var world = FrameworkEntry.Instance.GetModule<WorldStreamingModule>();

// 设置加载策略
world.SetChunkStrategy(new GridChunkStrategy(chunkSize: 64));

// 手动加载/卸载
world.LoadArea("chunk_0_0", worldNode);
world.UnloadArea("chunk_0_0");

// 根据观察者位置自动更新
world.UpdateViewerPosition(player.GlobalPosition);

// 查询
bool loaded = world.IsAreaLoaded("chunk_0_0");
int count = world.LoadedAreaCount;
```

### IChunkStrategy

```csharp
public interface IChunkStrategy
{
    List<string> GetChunksToLoad(Vector3 viewerPosition, float loadRadius);
    List<string> GetChunksToUnload(Vector3 viewerPosition, float unloadRadius);
}
```

---

## 16. 调试

### DebugModule

**命名空间**：`LynxFramework.Debug` | **优先级**：190

调试模块。命令注册、统计信息。

```csharp
var debug = FrameworkEntry.Instance.GetModule<DebugModule>();

// 切换调试 UI
debug.ToggleDebugUI();

// 注册命令
debug.RegisterCommand("give", (args) =>
{
    // args[0] = item_id, args[1] = count
});

// 执行命令
debug.ExecuteCommand("give sword_01 5");

// 注册统计
debug.RegisterStat("entity_count", () => entityModule.ActiveEntityCount);

// 获取统计
Dictionary<string, float> stats = debug.GetStats();
```

---

## 17. 热更新

### HotUpdateModule

**命名空间**：`LynxFramework.HotUpdate` | **优先级**：200

热更新模块。版本检测、.pck 下载、回滚。

```csharp
var hot = FrameworkEntry.Instance.GetModule<HotUpdateModule>();

// 检查更新
hot.CheckUpdate();

// 下载
hot.DownloadUpdate((progress) => GD.Print($"Download: {progress:P0}"));

// 应用（自动备份 + 回滚标记）
hot.ApplyUpdate();

// 回滚
hot.RecoverLastVersion();

// 查询
string current = hot.GetCurrentVersion();
string latest = hot.GetLatestVersion();
```

---

## 18. 事件系统

### EventArg 基类

所有事件载荷继承自 `EventArg`（`Godot.Resource` 子类）。

```csharp
public partial class MyEventArg : EventArg
{
    public string Message;
    public int Value;
}
```

### 内置事件 ID 列表

| 模块 | 事件 ID | 载荷类型 |
|------|---------|----------|
| 通用 | `framework_ready` | 无 |
| 设置 | `settings_changed` | `SettingsChangedEventArg` |
| 本地化 | `language_changed` | `LanguageChangedEventArg` |
| 场景 | `scene_loaded` / `scene_unloaded` | `SceneEventArg` |
| UI | `ui_opened` / `ui_closed` | `UIEventArg` |
| 输入 | `input_remap` | `InputRemapEventArg` |
| 实体 | `entity_spawned` / `entity_destroyed` | `EntityEventArg` |
| 流程 | `procedure_changed` | `ProcedureEventArg` |
| 存档 | `save_completed` | `SaveEventArg` |
| 网络 | `network_data_received` | `NetworkDataEventArg` |
| 网络 | `network_state_changed` | `NetworkStateEventArg` |
| 热更 | `hotupdate_available` / `hotupdate_download_complete` / `hotupdate_applied` | `HotUpdateEventArg` |
