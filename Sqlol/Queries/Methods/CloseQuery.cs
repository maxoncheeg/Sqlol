using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class CloseQuery : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        try
        {
            if (table == null) throw new Exception("Таблица не открыта");
            
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