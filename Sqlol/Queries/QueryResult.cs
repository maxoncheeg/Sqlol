using Sqlol.Tables;

namespace Sqlol.Queries;

public class QueryResult(int result, ITable? table, ITableData? data = null) : IQueryResult
{
    public int Result { get; } = result;
    public ITableData? Data { get; } = data;
    public ITable? Table { get; } = table;
}