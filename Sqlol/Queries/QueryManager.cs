using Sqlol.Tables;

namespace Sqlol.Queries;

public class QueryManager : IQueryManager
{
    private List<ITable> _openedTables = [];

    public event Action<string, string>? ErrorReceived;
    
    public bool OpenTable(string query)
    {
        int index = query.IndexOf(';');
        if (index != -1)
            query = query[..index];
        string[] words = query.Split(' ');

        if (words.Length == 2 && words[0].ToLowerInvariant().Equals("open"))
        {
            string tableName = words.Last();
            
            
        }
    
        ErrorReceived?.Invoke("Неверный запрос", "Неверная структура");
        return false; // ошибочный запрос
    }

    public bool CloseTable(string query)
    {
        throw new NotImplementedException();
    }

    public int Execute(string query)
    {
        throw new NotImplementedException();
    }

    public ITableData ExecuteNoneQuery(string query)
    {
        throw new NotImplementedException();
    }
}