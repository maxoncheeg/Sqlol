using System.Text.RegularExpressions;
using Sqlol.Loggers;
using Sqlol.Tables;
using Sqlol.Tables.Properties;

namespace Sqlol.Queries.Methods;

public class AddColumnQuery(ITablePropertyConverter converter, ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null)
        {
            logger.SendMessage("Ошибка", $"Таблица не открыта");
            return new QueryResult(0, null);
        }

        //var column = Regex.Match(textQuery, @"add\s+\w{1,11}").Value.Replace("add", "").Trim();
        // if (table.Properties.FirstOrDefault(p => p.Name.Equals(column, StringComparison.InvariantCultureIgnoreCase)) ==
        //     null)
        // {
        //     logger.SendMessage("Ошибка", $"В таблице нет такого поля");
        //     return new QueryResult(0, null);
        // }

        IList<ITableProperty> properties = converter.Convert(Regex.Match(textQuery, @"add\s+.+", RegexOptions.IgnoreCase).Value
            .Replace("add", "", StringComparison.InvariantCultureIgnoreCase).Trim());
        if (properties.Count != 1)
        {
            logger.SendMessage("Ошибка", $"Неверно задан тип нового столбца");
            return new QueryResult(0, null);
        }

        ITableProperty property = properties[0];
        property.Index = (byte)(table.Properties.Last().Index + 1);

        return new QueryResult(table.AddColumn(properties[0]) ? 1 : 0, table);
    }
}