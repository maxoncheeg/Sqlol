using Sqlol.Tables;

namespace Sqlol.Queries.Methods;

public interface IQuery
{
    public IQueryResult Execute(string textQuery, ITable? table = null);
}