using System.Text.RegularExpressions;
using Sqlol.Loggers;
using Sqlol.Tables;
using Sqlol.Tables.Properties;

namespace Sqlol.Queries.Methods;

public class UpdateColumnQuery(ITablePropertyConverter converter, ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null)
        {
            logger.SendMessage("Ошибка", $"Таблица не открыта");
            return new QueryResult(0, null);
        }

        var text = Regex.Match(textQuery, @"update\s+\w{1,11}\s+.+$").Value;
        text = text[text.IndexOf(' ')..].Trim();
        var columnName = text[..text.IndexOf(' ')].Trim();
        var newColumn = text[text.IndexOf(' ')..].Trim();
        
        // if (table.Properties.FirstOrDefault(p => p.Name.Equals(column, StringComparison.InvariantCultureIgnoreCase)) ==
        //     null)
        // {
        //     logger.SendMessage("Ошибка", $"В таблице нет такого поля");
        //     return new QueryResult(0, null);
        // }

        IList<ITableProperty> properties = converter.Convert(newColumn);
        if (properties.Count != 1)
        {
            logger.SendMessage("Ошибка", $"Неверно задан тип нового столбца");
            return new QueryResult(0, null);
        }

        ITableProperty property = properties[0];
        if (table.Properties.FirstOrDefault(p => p.Name == columnName) == null)
        {
            logger.SendMessage("Ошибка", "В таблице нет такого поля");
            return new QueryResult(0, table);
        }

        return new QueryResult(table.UpdateColumn(columnName, property) ? 1 : 0, table);
    }
}