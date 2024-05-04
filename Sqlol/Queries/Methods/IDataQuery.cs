using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public interface IDataQuery
{
    public ITableData Execute(ITable table, string textQuery);
}