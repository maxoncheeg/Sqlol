using System.Text.RegularExpressions;
using Sqlol.Loggers;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class ColumnRenameQuery(ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null) logger.SendMessage("Ошибка", $"Таблица не открыта");

        string columns = Regex.Match(textQuery, @"\w+\s+\w+$").Value;
        string name = Regex.Match(columns, @"\w+\s+").Value.Trim();
        string newName = Regex.Match(columns, @"\w+$").Value;
        
        int result = table.RenameColumn(name, newName) ? 1 : 0;
        return new QueryResult(result, table);
    }
}