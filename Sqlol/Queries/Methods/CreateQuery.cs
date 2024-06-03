using System.Text.RegularExpressions;
using Sqlol.Configurations.Factories;
using Sqlol.Loggers;
using Sqlol.Tables;
using Sqlol.Tables.Properties;

namespace Sqlol.Queries.Methods;

public class CreateQuery(ITablePropertyConverter converter, ILogger logger, IValidationFactory validation, IOperationFactory operationFactory) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        
        string tableName = validation.GetTableName(textQuery[..textQuery.IndexOf(' ')],textQuery);
        
        if (File.Exists(tableName + ".dbf"))
        {
            logger.SendMessage("Ошибка", $"Таблица с именем {tableName} уже существует");
            return new QueryResult(0, null);
        }
        
        try
        {
            IList<ITableProperty> properties = converter.Convert(Regex.Match(textQuery, @"\(.+\)").Value);
            Stream stream = File.Create(tableName + ".dbf");
            ITable newTable = new StreamTable( tableName, stream, properties, operationFactory);
            return new QueryResult(1, newTable);
        }
        catch (Exception e)
        {
            logger.SendMessage("Ошибка", e.Message);
            return new QueryResult(0, null);
        }
    }
}