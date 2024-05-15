namespace Sqlol.Tables;

public class TableData : ITableData
{
    private List<string> _columns;
    private List<List<string>> _values = [];

    public IReadOnlyList<string> Columns => _columns;
    public IReadOnlyList<IReadOnlyList<string>> Values => _values;

    public TableData(List<string> columns) => _columns = columns;

    public bool AddRecord(IList<string> record)
    {
        if (record.Count != _columns.Count) return false;

        _values.Add([..record]);
        return true;
    }

    public string GetStringTable()
    {
        string result = "";
        foreach (var column in Columns)
            result += " |\t" + column;
        result += Environment.NewLine;
        int index = 0;
        foreach (var record in Values)
        {
            result += index++;
            foreach (var variable in record)
                result += $"|\t" + variable;
            result += Environment.NewLine;
        }
        result += Environment.NewLine;

        return result;
    }
}