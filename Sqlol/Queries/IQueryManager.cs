using Sqlol.Tables;

namespace Sqlol.Queries;

public interface IQueryManager : IDisposable
{
   public IQueryResult Execute(string query);
}