using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Resource;

namespace LynxFramework.Data;

/// <summary>
/// 数据表管理模块。
/// 从 JSON/TRES 文件加载表格数据，提供查询接口。
/// </summary>
public class DataTableModule : IModule
{
    private ResourceService _resourceService;
    private LogService _logService;
    private readonly Dictionary<string, DataTable> _tables = new();

    public int Priority => 60;

    public void OnInit()
    {
        _resourceService = FrameworkEntry.Instance.GetModule<ResourceService>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
    }

    public void OnShutdown() => _tables.Clear();

    /// <summary>加载数据表。</summary>
    public DataTable LoadTable(string tableName)
    {
        if (_tables.TryGetValue(tableName, out var existing)) return existing;

        var path = $"res://data/tables/{tableName}.json";
        var jsonText = _resourceService.LoadCached<Json>(path);
        if (jsonText == null)
        {
            _logService?.Warning($"DataTable not found: {path}");
            return null;
        }

        var table = new DataTable(tableName);
        var data = jsonText.Data;

        if (data.VariantType == Variant.Type.Array)
        {
            var arr = data.AsGodotArray();
            foreach (var item in arr)
            {
                if (item.VariantType == Variant.Type.Dictionary)
                {
                    var row = item.AsGodotDictionary();
                    var id = row.ContainsKey("id") ? row["id"].AsString() : System.Guid.NewGuid().ToString();
                    table.AddRow(id, row);
                }
            }
        }

        _tables[tableName] = table;
        _logService?.Info($"DataTable loaded: {tableName} ({table.RowCount} rows)");
        return table;
    }

    /// <summary>获取指定表的指定行。</summary>
    public Godot.Collections.Dictionary GetRow(string tableName, string id)
        => _tables.TryGetValue(tableName, out var table) ? table.GetRow(id) : null;

    /// <summary>获取指定表的所有数据。</summary>
    public IReadOnlyDictionary<string, Godot.Collections.Dictionary> GetAll(string tableName)
        => _tables.TryGetValue(tableName, out var table) ? table.GetAll() : null;

    /// <summary>卸载数据表。</summary>
    public void UnloadTable(string tableName)
    {
        _tables.Remove(tableName);
        _logService?.Info($"DataTable unloaded: {tableName}");
    }

    /// <summary>检查数据表是否已加载。</summary>
    public bool HasTable(string tableName) => _tables.ContainsKey(tableName);
}
