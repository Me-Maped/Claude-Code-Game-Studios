# LynxFramework 示例项目

## Arena Survival

一个简单的竞技场生存游戏，演示 LynxFramework 的核心功能。

### 演示的模块

| 模块 | 用途 |
|------|------|
| FrameworkEntry | 框架启动入口 |
| EventBus | 伤害/死亡/分数事件 |
| UpdateDriver | 游戏主循环 |
| PoolManager | 子弹对象池 |
| EntityModule | 敌人生成/销毁 |
| ProcedureModule | 游戏流程（菜单→游戏→结算） |
| InputModule | 玩家输入 |
| InputBufferModule | 攻击输入缓冲 |
| CameraModule | 摄像机跟随 + 震动 |
| AudioModule | BGM + 音效 |
| UIModule | HUD / 菜单 |
| SaveModule | 最高分存档 |
| GameSettingsModule | 音量设置 |

### 文件结构

```
examples/arena-survival/
├── project.godot
├── scenes/
│   ├── MainMenu.tscn        # 主菜单场景
│   ├── Arena.tscn            # 竞技场场景
│   └── GameOver.tscn         # 结算场景
├── scripts/
│   ├── procedures/
│   │   ├── MenuProcedure.cs      # 菜单流程
│   │   ├── GameplayProcedure.cs  # 游戏流程
│   │   └── GameOverProcedure.cs  # 结算流程
│   ├── entities/
│   │   ├── Player.cs             # 玩家实体
│   │   ├── Enemy.cs              # 敌人实体（可池化）
│   │   └── Bullet.cs             # 子弹实体（可池化）
│   ├── ui/
│   │   ├── HUD.cs                # 游戏内 HUD
│   │   └── PauseMenu.cs          # 暂停菜单
│   └── data/
│       ├── EnemyData.cs          # 敌人数据表
│       └── WeaponData.cs         # 武器数据表
└── data/
    └── tables/
        ├── enemies.json          # 敌人配置表
        └── weapons.json          # 武器配置表
```

### 运行方式

1. 用 Godot 4.6.3 打开 `examples/arena-survival/project.godot`
2. 确保 `LynxFramework` 的 `FrameworkEntry` 已配置为 Autoload
3. 按 F5 运行
