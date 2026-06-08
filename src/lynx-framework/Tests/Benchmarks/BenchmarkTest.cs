using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using LynxFramework;
using LynxFramework.Core;
using LynxFramework.Entity;
using LynxFramework.Input;
using LynxFramework.Log;
using LynxFramework.Pool;
using LynxFramework.Resource;

/// <summary>
/// LynxFramework 性能基准测试。
/// 测试各模块在高负载下的性能表现。
/// </summary>
public partial class BenchmarkTest : Node
{
    private int _passCount;
    private int _failCount;

    public override void _Ready()
    {
        GD.Print("╔══════════════════════════════════════════╗");
        GD.Print("║   LynxFramework Performance Benchmark    ║");
        GD.Print("╚══════════════════════════════════════════╝");
        GD.Print("");

        var fw = FrameworkEntry.Instance;
        if (fw == null) { GD.PushError("FATAL: FrameworkEntry.Instance is null."); return; }

        // ═══════════════════════════════════════
        // Benchmark 1: EventBus 吞吐量
        // ═══════════════════════════════════════
        GD.Print("── Benchmark 1: EventBus Throughput ──");
        var eventBus = fw.GetModule<EventBus>();
        int eventCount = 0;
        Action handler = () => eventCount++;
        eventBus.Subscribe("bench_event", handler);

        var sw = Stopwatch.StartNew();
        const int EVENT_ITERATIONS = 100_000;
        for (int i = 0; i < EVENT_ITERATIONS; i++)
            eventBus.Emit("bench_event");
        sw.Stop();

        var eventThroughput = EVENT_ITERATIONS / sw.Elapsed.TotalSeconds;
        GD.Print($"  {EVENT_ITERATIONS:N0} events in {sw.ElapsedMilliseconds}ms");
        GD.Print($"  Throughput: {eventThroughput:N0} events/sec");
        Assert(eventThroughput > 100_000, "EventBus >100K events/sec");
        eventBus.Unsubscribe("bench_event", handler);

        // ═══════════════════════════════════════
        // Benchmark 2: EventBus 类型安全载荷
        // ═══════════════════════════════════════
        GD.Print("");
        GD.Print("── Benchmark 2: EventBus Typed Payload ──");
        int typedCount = 0;
        eventBus.Subscribe<BenchPayload>("bench_typed", (arg) => typedCount++);

        sw.Restart();
        for (int i = 0; i < EVENT_ITERATIONS; i++)
            eventBus.Emit("bench_typed", new BenchPayload { Value = i });
        sw.Stop();

        var typedThroughput = EVENT_ITERATIONS / sw.Elapsed.TotalSeconds;
        GD.Print($"  {EVENT_ITERATIONS:N0} typed events in {sw.ElapsedMilliseconds}ms");
        GD.Print($"  Throughput: {typedThroughput:N0} events/sec");
        Assert(typedThroughput > 50_000, "Typed EventBus >50K events/sec");

        // ═══════════════════════════════════════
        // Benchmark 3: ObjectPool Get/Recycle
        // ═══════════════════════════════════════
        GD.Print("");
        GD.Print("── Benchmark 3: ObjectPool Get/Recycle ──");
        var poolManager = fw.GetModule<PoolManager>();
        poolManager.CreateObjectPool(() => new BenchPoolable(), 1000);

        const int POOL_ITERATIONS = 100_000;
        sw.Restart();
        for (int i = 0; i < POOL_ITERATIONS; i++)
        {
            var obj = poolManager.GetObject<BenchPoolable>();
            poolManager.RecycleObject(obj);
        }
        sw.Stop();

        var poolThroughput = POOL_ITERATIONS / sw.Elapsed.TotalSeconds;
        GD.Print($"  {POOL_ITERATIONS:N0} get/recycle cycles in {sw.ElapsedMilliseconds}ms");
        GD.Print($"  Throughput: {poolThroughput:N0} cycles/sec");
        Assert(poolThroughput > 100_000, "ObjectPool >100K cycles/sec");

        // ═══════════════════════════════════════
        // Benchmark 4: InputBuffer 操作
        // ═══════════════════════════════════════
        GD.Print("");
        GD.Print("── Benchmark 4: InputBuffer Operations ──");
        var inputBuffer = fw.GetModule<InputBufferModule>();

        const int BUFFER_ITERATIONS = 50_000;
        sw.Restart();
        for (int i = 0; i < BUFFER_ITERATIONS; i++)
        {
            inputBuffer.BufferAction("bench_action", 1.0);
            inputBuffer.ConsumeAction("bench_action");
        }
        sw.Stop();

        var bufferThroughput = BUFFER_ITERATIONS / sw.Elapsed.TotalSeconds;
        GD.Print($"  {BUFFER_ITERATIONS:N0} buffer/consume cycles in {sw.ElapsedMilliseconds}ms");
        GD.Print($"  Throughput: {bufferThroughput:N0} cycles/sec");
        Assert(bufferThroughput > 10_000, "InputBuffer >10K cycles/sec");

        // ═══════════════════════════════════════
        // Benchmark 5: ResourceService 缓存命中
        // ═══════════════════════════════════════
        GD.Print("");
        GD.Print("── Benchmark 5: ResourceService Cache ──");
        var resourceService = fw.GetModule<ResourceService>();

        // 首次加载（冷启动）
        sw.Restart();
        var res = resourceService.LoadCached<Texture2D>("res://icon.svg");
        sw.Stop();
        var coldMs = sw.Elapsed.TotalMilliseconds;
        GD.Print($"  Cold load: {coldMs:F3}ms");

        // 缓存命中
        sw.Restart();
        const int CACHE_ITERATIONS = 10_000;
        for (int i = 0; i < CACHE_ITERATIONS; i++)
            resourceService.LoadCached<Texture2D>("res://icon.svg");
        sw.Stop();

        var cacheThroughput = CACHE_ITERATIONS / sw.Elapsed.TotalSeconds;
        GD.Print($"  {CACHE_ITERATIONS:N0} cache hits in {sw.ElapsedMilliseconds}ms");
        GD.Print($"  Throughput: {cacheThroughput:N0} lookups/sec");
        Assert(cacheThroughput > 100_000, "Cache >100K lookups/sec");

        // ═══════════════════════════════════════
        // Benchmark 6: 内存基准
        // ═══════════════════════════════════════
        GD.Print("");
        GD.Print("── Benchmark 6: Memory Baseline ──");
        var objectCount = Performance.GetMonitor(Performance.Monitor.ObjectCount);
        var memoryStatic = Performance.GetMonitor(Performance.Monitor.MemoryStatic);
        GD.Print($"  Object count: {objectCount:N0}");
        GD.Print($"  Static memory: {memoryStatic / 1024 / 1024:F2} MB");
        Assert(memoryStatic < 100 * 1024 * 1024, "Static memory <100MB");

        // ═══════════════════════════════════════
        // 最终结果
        // ═══════════════════════════════════════
        GD.Print("");
        PrintResult();
    }

    private void Assert(bool condition, string testName)
    {
        if (condition)
        {
            GD.Print($"  ✓ {testName}");
            _passCount++;
        }
        else
        {
            GD.PushError($"  ✗ FAIL: {testName}");
            _failCount++;
        }
    }

    private void PrintResult()
    {
        GD.Print("╔══════════════════════════════════════════╗");
        if (_failCount == 0)
            GD.Print($"║  RESULT: PASS  ({_passCount}/{_passCount} benchmarks passed)  ║");
        else
            GD.Print($"║  RESULT: FAIL  ({_failCount} failed, {_passCount} passed)    ║");
        GD.Print("╚══════════════════════════════════════════╝");
    }
}

public partial class BenchPayload : EventArg { public int Value; }
public class BenchPoolable : IPoolableObject
{
    public void OnGet() { }
    public void OnRecycle() { }
}
