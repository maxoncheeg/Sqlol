using Sqlol.Configurations.Factories;
using Sqlol.Tables;
using Sqlol.Tables.Memory;

namespace Sqlol.Queries;

public class QueryManager : IQueryManager
{
    private List<ITable> _openedTables = [];
    private readonly ITableReader _reader;
    private IValidationFactory _validationFactory;

    public event Action<string, string>? ErrorReceived;

    public QueryManager(ITableReader reader, IValidationFactory validationFactory)
    {
        _reader = reader;
        _validationFactory = validationFactory;
    }
    
    public bool OpenTable(string query)
    {
        int index = query.IndexOf(';');
        if (index != -1)
            query = query[..index];
        string[] words = query.Split(' ');

        if (words.Length == 2 && words[0].ToLowerInvariant().Equals("open"))
        {
            string tableName = words.Last();
            if(_reader.CreateTable([], tableName) != null)
                return true;
        }
    
        ErrorReceived?.Invoke("Неверный запрос", "Неверная структура");
        return false; // ошибочный запрос
    }

    public bool CreateTable(string query)
    {
        int index = query.IndexOf(';');
        if (index != -1)
            query = query[..index];
        string[] words = query.Split(' ');

        if (words.Length == 2 && words[0].ToLowerInvariant().Equals("create"))
        {
            string tableName = words.Last();
            _reader.CreateTable([], tableName);
        }
    
        ErrorReceived?.Invoke("Неверный запрос", "Неверная структура");
        return false; // ошибочный запрос
    }

    public bool CloseTable(string query)
    {
        throw new NotImplementedException();
    }

    public bool DropTable(string query)
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


    public void Dispose()
    {
        foreach (var table in _openedTables)
        {
            table.Dispose();
        }
    }
}