using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public interface IQuery
{
    public int Execute(ITable table, string textQuery);
}