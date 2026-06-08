using System;
using System.Collections.Generic;

namespace LynxFramework.Log;

/// <summary>
/// 日志服务。支持多输出目标、日志级别过滤。
/// </summary>
public class LogService : IModule
{
    private LogLevel _minLevel = LogLevel.Debug;
    private readonly List<ILogOutput> _outputs = new();

    public int Priority => 10;

    public void OnInit()
    {
        _outputs.Add(new ConsoleLogOutput());
    }

    public void OnShutdown()
    {
        foreach (var output in _outputs)
            (output as IDisposable)?.Dispose();
        _outputs.Clear();
    }

    /// <summary>设置最低日志级别。低于此级别的日志将被过滤。</summary>
    public void SetMinLevel(LogLevel level) => _minLevel = level;

    /// <summary>添加日志输出目标。</summary>
    public void AddOutput(ILogOutput output) => _outputs.Add(output);

    /// <summary>移除日志输出目标。</summary>
    public void RemoveOutput(ILogOutput output) => _outputs.Remove(output);

    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Info(string message) => Log(LogLevel.Info, message);
    public void Warning(string message) => Log(LogLevel.Warning, message);
    public void Error(string message) => Log(LogLevel.Error, message);

    /// <summary>记录日志。自动添加时间戳和级别标记。</summary>
    public void Log(LogLevel level, string message)
    {
        if (level < _minLevel) return;
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var formatted = $"[{timestamp}][{level}] {message}";
        foreach (var output in _outputs)
            output.Write(level, formatted);
    }
}
