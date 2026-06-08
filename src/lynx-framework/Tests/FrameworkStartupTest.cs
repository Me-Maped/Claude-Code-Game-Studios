using Godot;
using LynxFramework;
using LynxFramework.Core;
using LynxFramework.Log;

/// <summary>
/// Phase 1 验证脚本。
/// 将此脚本附加到场景中的 Node 上，运行场景即可验证框架启动流程。
/// 前提：FrameworkEntry 已设置为 Autoload。
/// </summary>
public partial class FrameworkStartupTest : Node
{
    public override void _Ready()
    {
        GD.Print("=== LynxFramework Phase 1 Startup Test ===");

        // 验证 FrameworkEntry 单例
        var framework = FrameworkEntry.Instance;
        if (framework == null)
        {
            GD.PushError("FAIL: FrameworkEntry.Instance is null. Is it set as Autoload?");
            return;
        }
        GD.Print("✓ FrameworkEntry.Instance exists");

        // 验证模块注册
        var eventBus = framework.GetModule<EventBus>();
        GD.Print(eventBus != null ? "✓ EventBus registered" : "FAIL: EventBus not found");

        var logService = framework.GetModule<LogService>();
        GD.Print(logService != null ? "✓ LogService registered" : "FAIL: LogService not found");

        var updateDriver = framework.GetModule<UpdateDriver>();
        GD.Print(updateDriver != null ? "✓ UpdateDriver registered" : "FAIL: UpdateDriver not found");

        // 验证 EventBus 订阅/发射
        bool eventReceived = false;
        eventBus.Subscribe("test_event", () => eventReceived = true);
        eventBus.Emit("test_event");
        GD.Print(eventReceived ? "✓ EventBus subscribe/emit works" : "FAIL: EventBus event not received");

        // 验证 EventBus 类型安全载荷
        bool typedEventReceived = false;
        eventBus.Subscribe<TestEventArg>("test_typed_event", (arg) =>
        {
            typedEventReceived = arg.Message == "hello";
        });
        eventBus.Emit("test_typed_event", new TestEventArg { Message = "hello" });
        GD.Print(typedEventReceived ? "✓ EventBus typed payload works" : "FAIL: Typed event not received");

        // 验证 LogService
        logService.Info("LogService is working (this message is from the test)");
        GD.Print("✓ LogService output works");

        // 验证 UpdateDriver
        int updateCount = 0;
        var entry = updateDriver.Register(UpdatePhase.Process, 0, (delta) =>
        {
            updateCount++;
            if (updateCount == 1)
                GD.Print("✓ UpdateDriver Process callback fired");
        });
        GD.Print("✓ UpdateDriver registration works (callback will fire next frame)");

        // 清理测试订阅
        eventBus.Unsubscribe("test_event", () => eventReceived = true);

        GD.Print("=== Phase 1 Test Complete ===");
    }
}

/// <summary>测试用事件载荷。</summary>
public partial class TestEventArg : EventArg
{
    public string Message { get; set; }
}
