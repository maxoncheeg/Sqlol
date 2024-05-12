using Sqlol.Configurations.Factories;
using Sqlol.Loggers;
using Sqlol.Queries.Methods;
using Sqlol.Tables.Properties;

namespace Sqlol.Queries;

public class QueryFactory : IQueryFactory
{
    private Dictionary<string, IQuery> _queries = [];

    public QueryFactory(Dictionary<string, IQuery> queries)
    {
        _queries = queries;
    }
    
    public IQuery GetQuery(string keyWord)
    {
        if (_queries.TryGetValue(keyWord.ToLowerInvariant(), out var query))
            return query;
        throw new ApplicationException("запроса не существует");
    }
}