using Sqlol.Configurations;
using Sqlol.Loggers;
using Sqlol.Tables;
using Sqlol.Tables.Properties;

namespace Sqlol.Queries.Methods;

public class InsertQuery : IQuery
{
    private readonly IKeyWordsConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IQueryChangesSeparator _separator;
    
    public InsertQuery(IKeyWordsConfiguration configuration, IQueryChangesSeparator separator, ILogger logger)
    {
        _configuration = configuration;
        _separator = separator;
        _logger = logger;
    }
    
    public IQueryResult Execute(string textQuery, ITable? table = null)
    {
        if (table == null) return new QueryResult(0, null);

        string keyWord = textQuery[..textQuery.IndexOf(' ')];
        var changes = _separator.GetChangesFromQuery(keyWord, textQuery);

        foreach (var tuple in changes)
        {
            var property = table.Properties.FirstOrDefault(prop => prop.Name == tuple.Item1);
            
            if (property == null)
            {
                _logger.SendMessage("Ошибка", $"Таблица {table.Name} не имеет поля {tuple.Item1}");
                return new QueryResult(0, null);
            }
            if (!_configuration.Types[property.Type].Validate(tuple.Item2, property.Width, property.Precision))
            {
                _logger.SendMessage("Ошибка", $"Значение поля {tuple.Item1} не соотвествует типу {property.Type}");
                return new QueryResult(0, null);
            }
        }

        return new QueryResult(table.Insert(changes) ? 1 : 0, table);
    }
}