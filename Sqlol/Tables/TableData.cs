namespace Sqlol.Tables;

public class TableData : ITableData
{
    private List<string> _columns;
    private List<List<string>> _values = [];

    public int MaxLenRows { get; set; }
    public int MaxLengthOfRecord { get; set; }
    
    public IReadOnlyList<string> Columns => _columns;
    public IReadOnlyList<IReadOnlyList<string>> Values => _values;

    public TableData(List<string> columns)
    {
        _columns = columns;
        MaxLenRows = 1;

        MaxLengthOfRecord = 0;
        foreach (string value in columns)
            if (value.Length > MaxLengthOfRecord)
                MaxLengthOfRecord = value.Length;
    }



    public bool AddRecord(IList<string> record)
    {
        if (record.Count != Columns.Count) return false;

        foreach(string value in record)
            if(value.Length > MaxLengthOfRecord) 
                MaxLengthOfRecord = value.Length;

        // todo: при добавлении записи, ты должна сравнивать ее значения с уже самой длинной на данный момент переменной(для ускорения расчетов)

        _values.Add([..record]);
        return true;
    }

    public string GetHeader()
    {
        MaxLenRows = Values.Count.ToString().Length;

        string result = " ".PadLeft(MaxLenRows);
        string emptyLine = "";

        for (int i = 0; i < Columns.Count; i++)
        {
            if((MaxLengthOfRecord - Columns[i].Length)% 2 == 0)
                result += "| " + Columns[i].PadLeft((MaxLengthOfRecord - Columns[i].Length) / 2 + Columns[i].Length)
                    + emptyLine.PadRight((MaxLengthOfRecord - Columns[i].Length) / 2) + " ";
            else
                result += "| " + Columns[i].PadLeft((MaxLengthOfRecord - Columns[i].Length) / 2 + Columns[i].Length)
                    + emptyLine.PadRight((MaxLengthOfRecord - Columns[i].Length) / 2 + 1) + " ";
        }
        return result;
        // todo: благодоря AddRecord ты уже будешь знать длинну столбцов, по ней и выстроить таблицу
        
    }

    public IEnumerator<string> GetRecords()
    {
        string resultValues = string.Empty;
        string emptyLine = "";


        for (int i = 0; i < Values.Count; i++)
        {
            resultValues = i.ToString().PadLeft(MaxLenRows);
            for (int j = 0; j < Values[i].Count; j++)
            {
                if ((MaxLengthOfRecord - Values[i][j].Length) % 2 == 0)
                    resultValues += "| " + Values[i][j].PadLeft((MaxLengthOfRecord - Values[i][j].Length) / 2 + Values[i][j].Length)
                    + emptyLine.PadRight((MaxLengthOfRecord - Values[i][j].Length) / 2) + " ";
                else
                    resultValues += "| " + Values[i][j].PadLeft((MaxLengthOfRecord - Values[i][j].Length) / 2 + Values[i][j].Length)
                    + emptyLine.PadRight((MaxLengthOfRecord - Values[i][j].Length) / 2 + 1) + " ";
            }
            
            yield return resultValues;
        }

        // todo: благодоря AddRecord ты уже будешь знать длинну столбцов, по ней и выстроить таблицу
        
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