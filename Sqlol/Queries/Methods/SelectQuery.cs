using System.Text.RegularExpressions;
using Sqlol.Expressions;
using Sqlol.Expressions.Builders;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class SelectQuery(IExpressionBuilder builder) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null) return new QueryResult(0, table);

        string where = "";
        IExpression? expression = null;
        if(textQuery.Contains("where"))
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

        if (textQuery[0] == '*')
            return new QueryResult(1, table, table.Select(expression));
        else
        {
            var columns = Regex.Matches(textQuery, @"\w{1,11}").Select(m => m.Value).ToList();
            return new QueryResult(1, table, table.Select(columns, expression));
        }

        return new QueryResult(0, null);
    }
}