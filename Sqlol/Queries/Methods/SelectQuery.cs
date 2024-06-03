using System.Text.RegularExpressions;
using Sqlol.Expressions;
using Sqlol.Expressions.Builders;
using Sqlol.Loggers;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class SelectQuery(IExpressionBuilder builder, ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null)
        {
            logger.SendMessage("Ошибка", "Таблица не открыта");
            return new QueryResult(0, table);
        }

        string where = "";
        IExpression? expression = null;
        if (textQuery.Contains("where", StringComparison.InvariantCultureIgnoreCase))
        {
            where = textQuery[textQuery.IndexOf("where", StringComparison.InvariantCultureIgnoreCase)..];
            where = where[where.IndexOf(' ')..];
            where = where.Trim();
            try
            {
                expression = builder.TranslateToExpression(where);
            }
            catch (Exception e)
            {
                logger.SendMessage("Ошибка where-запроса", e.Message);
                return new QueryResult(0, table);
            }
        }

        textQuery = textQuery.Trim();
        textQuery = textQuery[textQuery.IndexOf(' ')..];
        textQuery = textQuery[..textQuery.IndexOf("from", StringComparison.InvariantCultureIgnoreCase)];
        textQuery = textQuery.Trim();

        if (textQuery[0] == '*')
            return new QueryResult(1, table, table.Select(expression));
        else
        {
            var columns = Regex.Matches(textQuery, @"\w{1,11}").Select(m => m.Value).ToList();
            try
            {
                return new QueryResult(1, table, table.Select(columns, expression));
            }
            catch (Exception e)
            {
                logger.SendMessage("Ошибка", "Не удалось прочитать данные");
                
                return new QueryResult(0, null);
            }
        }

        return new QueryResult(0, null);
    }
}