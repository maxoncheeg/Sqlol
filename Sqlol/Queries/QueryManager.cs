using Sqlol.Configurations.Factories;
using Sqlol.Queries.Methods;
using Sqlol.Tables;
using Sqlol.Tables.Memory;

namespace Sqlol.Queries;

public class QueryManager : IQueryManager
{
    private List<ITable> _openedTables = [];
    private IValidationFactory _validationFactory;
    private IQueryFactory _queryFactory;

    public event Action<string, string>? ErrorReceived;

    public QueryManager(IValidationFactory validationFactory, IQueryFactory queryFactory)
    {
        _validationFactory = validationFactory;
        _queryFactory = queryFactory;
    }
    
    public IQueryResult Execute(string textQuery)
    {
        string command = textQuery[..textQuery.IndexOf(' ')];
        if (!_validationFactory.Validate(command, textQuery))
        {
            return new QueryResult(0, null);
        }

        string tableName = _validationFactory.GetTableName(command, textQuery);
        ITable? table = _openedTables.FirstOrDefault(t => t.Name == tableName);

        IQuery query = _queryFactory.GetQuery(command);
        return query.Execute(textQuery, table);
    }


    public void Dispose()
    {
        foreach (var table in _openedTables)
        {
            table.Dispose();
        }
    }
}