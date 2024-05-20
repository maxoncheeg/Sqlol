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

        // todo: при добавлении записи, ты должна сравнивать ее значения с уже самой длинной на данный момент переменной(для ускорения расчетов)

        _values.Add([..record]);
        return true;
    }

    public string GetHeader()
    {
        // todo: благодоря AddRecord ты уже будешь знать длинну столбцов, по ней и выстроить таблицу
        throw new NotImplementedException();
    }

    public IEnumerator<string> GetRecords()
    {
        // todo: благодоря AddRecord ты уже будешь знать длинну столбцов, по ней и выстроить таблицу
        throw new NotImplementedException();
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