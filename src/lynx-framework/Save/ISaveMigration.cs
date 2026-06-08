using Godot;

namespace LynxFramework.Save;

/// <summary>
/// 存档迁移接口。实现此接口定义版本间的数据迁移逻辑。
/// </summary>
public interface ISaveMigration
{
    /// <summary>源版本号。</summary>
    int FromVersion { get; }

    /// <summary>目标版本号。</summary>
    int ToVersion { get; }

    /// <summary>执行数据迁移。</summary>
    Godot.Collections.Dictionary Migrate(Godot.Collections.Dictionary data);
}
