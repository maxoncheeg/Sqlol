using Sqlol.Expressions;
using Sqlol.Tables.Memory;
using Sqlol.Tables.Properties;

namespace Sqlol.Tables;

public class StreamTable : ITable
{
    private ITableMemory _memory;

    private List<ITableProperty> _properties;
    
    public string Name { get; }
    public bool HasMemoFile { get; }
    public DateTime LastUpdateDate { get; }
    public int RecordsAmount { get; }
    public short HeaderLength { get; }
    public short RecordLength { get; }
    public IReadOnlyList<ITableProperty> Properties => _properties;

    public StreamTable(ITableMemory memory, string name, Stream tableStream, IList<ITableProperty> properties)
    {
        _memory = memory;
        
        Name = name;
        LastUpdateDate = DateTime.Now;
        HasMemoFile = false;
        RecordsAmount = 0;
        HeaderLength = 32;
        RecordLength = 5;
        _properties = properties.ToList();

        _memory.SaveHeader(this, tableStream);
        tableStream.Dispose();
    }

    public bool Insert(IList<Tuple<string, string>> data)
    {
        throw new NotImplementedException();
    }

    public ITableData Select(IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public ITableData Select(IList<string> columns, IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public int Update(IList<Tuple<string, string>> changes, IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public int Truncate()
    {
        throw new NotImplementedException();
    }

    public int Delete(IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public int Restore(IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public bool AddColumn(ITableProperty property)
    {
        throw new NotImplementedException();
    }

    public bool RemoveColumn(string columnName)
    {
        throw new NotImplementedException();
    }

    public bool RenameColumn(string currentName, string newName)
    {
        throw new NotImplementedException();
    }

    public bool UpdateColumn(string columnName, ITableProperty property)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}