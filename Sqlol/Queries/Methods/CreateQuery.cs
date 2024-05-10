using Sqlol.Configurations.Factories;
using Sqlol.Loggers;
using Sqlol.Tables;
using Sqlol.Tables.Memory;
using Sqlol.Tables.Properties;

namespace Sqlol.Queries.Methods;

public class CreateQuery(ITablePropertyConverter converter, ILogger logger, IValidationFactory validation) : IQuery
{
    private readonly ITablePropertyConverter _converter = converter;
    private readonly ILogger _logger = logger;
    private readonly IValidationFactory _validation = validation;
    
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        IList<ITableProperty> properties = _converter.Convert(textQuery);
        string tableName = validation.GetTableName(textQuery[..textQuery.IndexOf(' ')],textQuery);
        
        if (File.Exists(tableName + ".dbf"))
        {
            _logger.SendMessage("Ошибка", $"Таблица с именем {tableName} уже существует");
            return new QueryResult(0, null);
        }

        try
        {
            Stream stream = File.Create(tableName + ".dbf");
            ITable newTable = new StreamTable(new TableMemory(), tableName, stream, properties);
            return new QueryResult(1, newTable);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new QueryResult(0, null);
        }
    }
}