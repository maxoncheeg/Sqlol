using Sqlol.Loggers;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class TruncateQuery(ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null) logger.SendMessage("Ошибка", $"Таблица не открыта");
        int result = table.Truncate();
        logger.SendMessage("Truncate-запрос", $"Затронуто {result} строк");
        return new QueryResult(result, table);
    }
}