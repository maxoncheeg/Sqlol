using System.Text.RegularExpressions;
using Sqlol.Loggers;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class RemoveColumnQuery(ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null)
        {
            logger.SendMessage("Ошибка", $"Таблица не открыта");
            return new QueryResult(0, null);
        }

        var column = Regex.Match(textQuery, @"remove\s+\w{1,11}", RegexOptions.IgnoreCase).Value
            .Replace("remove", "", StringComparison.InvariantCultureIgnoreCase).Trim();
        

        return new QueryResult(table.RemoveColumn(column) ? 1 : 0, table);
    }
}