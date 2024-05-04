using Sqlol.Queries.Methods;

namespace Sqlol.Queries;

public interface IQueryFactory
{
    public IQuery GetQuery(string keyWord);
}