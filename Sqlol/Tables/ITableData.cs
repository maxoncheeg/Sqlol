namespace Sqlol.Tables;

public interface ITableData
{
    public IReadOnlyList<string> Columns { get; }
    public IReadOnlyList<IReadOnlyList<string>> Values { get; }

    public bool AddRecord(IList<string> record);
    public string GetStringTable();
}