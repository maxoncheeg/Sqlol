using Sqlol.Tables;

namespace Sqlol.Queries;

public interface IQueryManager : IDisposable
{
   public bool OpenTable(string query);
   public bool CreateTable(string query);
   public bool CloseTable(string query);
   public bool DropTable(string query);
   public int Execute(string query);
   public ITableData ExecuteNoneQuery(string query);
}