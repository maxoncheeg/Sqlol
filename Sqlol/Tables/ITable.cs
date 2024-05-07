using Sqlol.Expressions;
using Sqlol.Tables.Memory;
using Sqlol.Tables.Properties;

namespace Sqlol.Tables;

public interface ITable : IDisposable
{
    public string Name { get; }
    public bool HasMemoFile { get; }
    public DateTime LastUpdateDate { get; }
    public int RecordsAmount { get; }
    public short HeaderLength { get; }
    public short RecordLength { get; }
    //резервы 3 13 4 байт
    public IReadOnlyList<ITableProperty> Properties { get; }
    
    public bool Insert(IList<Tuple<string, string>> data);
    public ITableData Select(IExpression? expression = null);
    public ITableData Select(IList<string> columns, IExpression? expression = null);
    public int Update(IList<Tuple<string, string>> changes, IExpression? expression = null);
    public int Truncate();
    public int Delete(IExpression? expression = null);
    public int Restore(IExpression? expression = null);

    public bool AddColumn(ITableProperty property);
    public bool RemoveColumn(string columnName);
    public bool RenameColumn(string currentName, string newName);
    public bool UpdateColumn(string columnName, ITableProperty property);
}