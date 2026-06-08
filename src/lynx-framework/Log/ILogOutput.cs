namespace LynxFramework.Log;

/// <summary>
/// 日志输出接口。实现此接口可添加自定义日志输出目标（文件、远程等）。
/// </summary>
public interface ILogOutput
{
    void Write(LogLevel level, string message);
}
