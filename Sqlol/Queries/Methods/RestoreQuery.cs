using Sqlol.Loggers;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class RestoreQuery(ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null)
        {
            logger.SendMessage("Ошибка", $"Таблица не открыта");
            return new QueryResult(0, null);
        }
        
        int result = table.Restore();
        logger.SendMessage("Restore-запрос", $"Затронуто {result} строк");
        return new QueryResult(result, table);
    }
}