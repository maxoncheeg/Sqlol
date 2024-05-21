using System.Text.RegularExpressions;
using Sqlol.Configurations;
using Sqlol.Expressions;
using Sqlol.Expressions.Builders;
using Sqlol.Loggers;
using Sqlol.Tables;
using Sqlol.Tables.Properties;

namespace Sqlol.Queries.Methods;

public class UpdateQuery(IKeyWordsConfiguration configuration, IQueryChangesSeparator separator, IExpressionBuilder builder, ILogger logger) : IQuery
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
        if(textQuery.Contains("where", StringComparison.InvariantCultureIgnoreCase))
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

            textQuery = textQuery[..textQuery.IndexOf("where", StringComparison.InvariantCultureIgnoreCase)];
        }

        var changes = separator.GetChangesFromQuery(textQuery[..textQuery.IndexOf(' ')], textQuery);
 
        foreach (var tuple in changes)
        {
            var property =
                table.Properties.FirstOrDefault(prop => prop.Name.ToLowerInvariant() == tuple.Item1.ToLowerInvariant());

            if (property == null)
            {
                logger.SendMessage("Ошибка", $"Таблица {table.Name} не имеет поля {tuple.Item1}");
                return new QueryResult(0, table);
            }

            if (!configuration.Types[property.Type].Validate(tuple.Item2, property.Width, property.Precision))
            {
                logger.SendMessage("Ошибка", $"Значение поля {tuple.Item1} не соотвествует типу {property.Type}");
                return new QueryResult(0, table);
            }
        }
        
        int result = table.Update(changes, expression);
        logger.SendMessage("Update-запрос", $"Затронуто {result} строк");
        return new QueryResult(result, table);
    }
}