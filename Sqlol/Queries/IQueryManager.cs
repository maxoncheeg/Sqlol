using Sqlol.Tables;

namespace Sqlol.Queries;

public interface IQueryManager
{
   public event Action<string, string> ErrorReceived;
   
   public bool OpenTable(string query);
   public bool CloseTable(string query);
   public int Execute(string query);
   public ITableData ExecuteNoneQuery(string query);
}