using System.Text.RegularExpressions;
using Sqlol.Expressions;
using Sqlol.Expressions.Builders;
using Sqlol.Loggers;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class DeleteQuery(IExpressionBuilder builder, ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null) logger.SendMessage("Ошибка", $"Таблица не открыта");

        string where = "";
        IExpression? expression = null;
        if (textQuery.Contains("where"))
        {
            where = textQuery[textQuery.IndexOf("where", StringComparison.Ordinal)..];
            where = where[where.IndexOf(' ')..];
            where = where.Trim();
            expression = builder.TranslateToExpression(where);
        }

        textQuery = textQuery.Trim();
        textQuery = textQuery[textQuery.IndexOf(' ')..];
        textQuery = textQuery[..textQuery.IndexOf("from", StringComparison.Ordinal)];
        textQuery = textQuery.Trim();

        int result = table.Delete(expression);
        logger.SendMessage("Delete-запрос", $"Затронуто {result} строк");
        return new QueryResult(result, table);
    }
}