using Sqlol.Tables;

namespace Sqlol.Queries;

public interface IQueryResult
{
    public int Result { get; }
    public ITableData? Data { get; }
    public ITable? Table { get; }
}