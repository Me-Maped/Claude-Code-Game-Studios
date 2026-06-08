using Godot;

namespace LynxFramework.Log;

/// <summary>
/// 控制台日志输出。使用 GD.Print / GD.PushWarning / GD.PushError。
/// </summary>
public class ConsoleLogOutput : ILogOutput
{
    public void Write(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Debug:
            case LogLevel.Info:
                GD.Print(message);
                break;
            case LogLevel.Warning:
                GD.PushWarning(message);
                break;
            case LogLevel.Error:
                GD.PushError(message);
                break;
        }
    }
}
