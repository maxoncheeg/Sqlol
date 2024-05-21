using Sqlol.Configurations.Factories;
using Sqlol.Loggers;
using Sqlol.Queries.Methods;
using Sqlol.Tables;

namespace Sqlol.Queries;

public class QueryManager(IValidationFactory validationFactory, IQueryFactory queryFactory, ILogger logger) : IQueryManager
{
    private List<ITable> _openedTables = [];
    
    public IQueryResult Execute(string textQuery)
    {
        if (!textQuery.Contains(' ')) return new QueryResult(0, null);
        
        string command = textQuery[..textQuery.IndexOf(' ')];
        string? queryKey = validationFactory.Validate(command, textQuery);
        if (queryKey == null)
        {
            logger.SendMessage("Ошибка", "Запрос не прошел валидацию");
            return new QueryResult(0, null);
        }

        string tableName = validationFactory.GetTableName(command, textQuery);

        if (string.IsNullOrEmpty(tableName))
        {
            logger.SendMessage("Ошибка", "Не удалось распознать имя таблицы");
            return new QueryResult(0, null);
        }
        
        ITable? table = _openedTables.FirstOrDefault(t => string.Equals(t.Name, tableName, StringComparison.InvariantCultureIgnoreCase));
        IQuery? query = queryFactory.GetQuery(queryKey);
        
        if (query == null)
        {
            logger.SendMessage("Ошибка", "Запроса не существует");
            return new QueryResult(0, null);
        }
        
        IQueryResult result = query.Execute(textQuery, table);
        
        if (result.Result == 1 && table == null && result.Table != null)
            _openedTables.Add(result.Table);
        else if (result.Result == 1 && table != null && result.Table == null)
            _openedTables.Remove(table);
        
        return result;
    }


    public void Dispose()
    {
        foreach (var table in _openedTables)
        {
            table.Dispose();
        }
    }
}