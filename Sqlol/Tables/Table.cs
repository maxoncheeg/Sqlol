using Sqlol.Expressions;

namespace Sqlol.Tables;

public class Table : ITable
{
    private Func<string, string, bool> _applyRequired;
    
    public string Name { get; }
    public bool HasMemoFile { get; }
    public DateTime LastUpdateDate { get; }
    public int RecordsAmount { get; }
    public short HeaderLength { get; }
    public short RecordLength { get; }
    public IReadOnlyList<ITableProperty> Properties { get; }

    public Table(string name, Func<string, string, bool> applyRequired, Stream tableStream)
    {
        Name = name;
        _applyRequired = applyRequired;
        
        
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