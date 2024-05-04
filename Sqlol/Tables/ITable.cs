namespace Sqlol.Tables;

public interface ITable
{
    public string Name { get; }
    
    // public bool Insert(ICollection<KeyValuePair<string, string>> data);
    // public TableData get(Expression* expression);
    // public long Update(ICollection<KeyValuePair<string, string>> changes, Expression* expression);
    // public bool Remove(Expression* expression);
}