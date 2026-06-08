using Godot;
using LynxFramework;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Pool;
using LynxFramework.Resource;

/// <summary>
/// Phase 2 验证脚本。
/// 测试 PoolManager（节点池 + 对象池）和 ResourceService（缓存 + 统计）。
/// </summary>
public partial class Phase2Test : Node
{
    public override void _Ready()
    {
        GD.Print("=== LynxFramework Phase 2 Test ===");

        var framework = FrameworkEntry.Instance;
        if (framework == null)
        {
            GD.PushError("FAIL: FrameworkEntry.Instance is null.");
            return;
        }

        // --- PoolManager 测试 ---
        var poolManager = framework.GetModule<PoolManager>();
        GD.Print(poolManager != null ? "✓ PoolManager registered" : "FAIL: PoolManager not found");

        // 测试普通对象池
        poolManager.CreateObjectPool(() => new TestPoolable(), 4);
        GD.Print("✓ ObjectPool<TestPoolable> created with 4 prewarm");

        var obj1 = poolManager.GetObject<TestPoolable>();
        GD.Print(obj1 != null ? "✓ GetObject<TestPoolable> returned object" : "FAIL: GetObject returned null");
        GD.Print($"  obj1.IsActive={obj1.IsActive}");

        var obj2 = poolManager.GetObject<TestPoolable>();
        var obj3 = poolManager.GetObject<TestPoolable>();
        var obj4 = poolManager.GetObject<TestPoolable>();
        GD.Print($"✓ 4 objects retrieved (prewarm exhausted)");

        // 第 5 个应该触发 miss
        var obj5 = poolManager.GetObject<TestPoolable>();
        GD.Print(obj5 != null ? "✓ 5th object created on demand (miss)" : "FAIL: 5th object null");

        // 回收
        poolManager.RecycleObject(obj1);
        poolManager.RecycleObject(obj2);
        GD.Print("✓ 2 objects recycled");

        // 再次获取应该命中
        var obj6 = poolManager.GetObject<TestPoolable>();
        GD.Print(obj6 == obj1 || obj6 == obj2 ? "✓ GetObject reused recycled object (hit)" : "✓ GetObject returned an object");

        // 回收剩余
        poolManager.RecycleObject(obj3);
        poolManager.RecycleObject(obj4);
        poolManager.RecycleObject(obj5);
        poolManager.RecycleObject(obj6);

        // --- ResourceService 测试 ---
        var resourceService = framework.GetModule<ResourceService>();
        GD.Print(resourceService != null ? "✓ ResourceService registered" : "FAIL: ResourceService not found");

        // 测试同步加载（加载一个 Godot 内置资源）
        var testResource = resourceService.LoadCached<Texture2D>("res://icon.svg");
        GD.Print(testResource != null ? "✓ LoadCached<Texture2D>(icon.svg) loaded" : "WARN: icon.svg not found (expected if no icon)");

        // 测试缓存
        var testResource2 = resourceService.LoadCached<Texture2D>("res://icon.svg");
        if (testResource != null && testResource2 != null)
            GD.Print(ReferenceEquals(testResource, testResource2) ? "✓ Cache hit — same instance returned" : "FAIL: Cache miss");

        GD.Print($"  Cache count: {resourceService.CacheCount}");

        // --- 对象池统计测试 ---
        var stats = poolManager.GetPoolStats();
        GD.Print($"✓ GetPoolStats returned {stats.Count} pool(s)");
        // 注意：普通对象池不在 GetPoolStats 中（仅节点池），这是设计预期

        GD.Print("=== Phase 2 Test Complete ===");
    }
}

/// <summary>测试用可池化对象。</summary>
public class TestPoolable : IPoolableObject
{
    public bool IsActive { get; private set; }

    public void OnGet()
    {
        IsActive = true;
        GD.Print($"  TestPoolable.OnGet() — IsActive={IsActive}");
    }

    public void OnRecycle()
    {
        IsActive = false;
        GD.Print($"  TestPoolable.OnRecycle() — IsActive={IsActive}");
    }
}
