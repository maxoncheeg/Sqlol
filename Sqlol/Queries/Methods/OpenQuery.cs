using Sqlol.Configurations.Factories;
using Sqlol.Loggers;
using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public class OpenQuery(IValidationFactory validation, ILogger logger) : IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        string tableName = validation.GetTableName(textQuery[..textQuery.IndexOf(' ')],textQuery);
        
        if (!File.Exists(tableName + ".dbf"))
        {
            logger.SendMessage("Ошибка", $"Таблица с именем {tableName} не существует");
            return new QueryResult(0, null);
        }

        try 
        {
            Stream stream = File.Open(tableName + ".dbf", FileMode.Open);
            ITable newTable = new StreamTable( tableName, stream);
            return new QueryResult(1, newTable);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new QueryResult(0, null);
        }
    }
}