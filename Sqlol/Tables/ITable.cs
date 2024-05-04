using Sqlol.Expressions;
using Sqlol.Tables.Memory;

namespace Sqlol.Tables;

public interface ITable
{
    public string Name { get; }
    public ITableMemory Memory { get; }
    
    public bool Insert(ICollection<KeyValuePair<string, string>> data);
    public ITableData Get(IExpression expression);
    public int Update(ICollection<KeyValuePair<string, string>> changes, IExpression expression);
    public bool Remove(IExpression expression);
}