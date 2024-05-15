using Sqlol.Configurations.Factories;
using Sqlol.Queries.Methods;
using Sqlol.Tables;

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
        ITable? table = _openedTables.FirstOrDefault(t => string.Equals(t.Name, tableName, StringComparison.InvariantCultureIgnoreCase));

        IQuery query = _queryFactory.GetQuery(command);
        IQueryResult result = query.Execute(textQuery, table);
        
        if (result.Result == 1 && table == null && result.Table != null)
            _openedTables.Add(result.Table);
        else if (result.Result == 1 && table != null && result.Table == null)
            _openedTables.Remove(table);
        
        return result;
    }


    public void Dispose()
    {
        foreach (var table in _openedTables)
        {
            table.Dispose();
        }
    }
}