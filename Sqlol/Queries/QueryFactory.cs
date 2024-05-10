using Sqlol.Configurations.Factories;
using Sqlol.Loggers;
using Sqlol.Queries.Methods;
using Sqlol.Tables.Properties;

namespace Sqlol.Queries;

public class QueryFactory : IQueryFactory
{
    private Dictionary<string, IQuery> _queries;

    public QueryFactory(ITablePropertyConverter converter, IValidationFactory validation, ILogger logger)
    {
        _queries = new Dictionary<string, IQuery>()
        {
            { "create", new CreateQuery(converter, logger, validation) }
        };
    }
    
    public IQuery GetQuery(string keyWord)
    {
        if (_queries.TryGetValue(keyWord, out var query))
            return query;
        throw new ApplicationException("query doesn't exist");
    }
}