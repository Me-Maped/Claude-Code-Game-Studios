using System.Collections.Generic;
using Godot;

namespace LynxFramework.Data;

/// <summary>
/// 数据表。存储从 JSON/TRES 加载的表格数据。
/// </summary>
public class DataTable
{
    public string TableName { get; }
    private readonly Dictionary<string, Godot.Collections.Dictionary> _rows = new();

    public DataTable(string name) => TableName = name;

    /// <summary>添加数据行。</summary>
    public void AddRow(string id, Godot.Collections.Dictionary row) => _rows[id] = row;

    /// <summary>根据 ID 获取数据行。</summary>
    public Godot.Collections.Dictionary GetRow(string id)
        => _rows.TryGetValue(id, out var row) ? row : null;

    /// <summary>获取所有数据行。</summary>
    public IReadOnlyDictionary<string, Godot.Collections.Dictionary> GetAll() => _rows;

    /// <summary>根据字段值查询数据行。</summary>
    public List<Godot.Collections.Dictionary> GetRowsByField(string field, Variant value)
    {
        var result = new List<Godot.Collections.Dictionary>();
        foreach (var row in _rows.Values)
        {
            if (row.TryGetValue(field, out var v) && v.VariantType == value.VariantType && v.Equals(value))
                result.Add(row);
        }
        return result;
    }

    /// <summary>获取数据行数量。</summary>
    public int RowCount => _rows.Count;
}
