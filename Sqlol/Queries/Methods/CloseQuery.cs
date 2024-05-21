using Sqlol.Loggers;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class CloseQuery(ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        try
        {
            if (table == null)
            {
                logger.SendMessage("Ошибка", "Таблица не открыта");
                return new QueryResult(0, table);
            }
            
            table.Dispose();
            return new QueryResult(1, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new QueryResult(0, null);
        }
    }
}