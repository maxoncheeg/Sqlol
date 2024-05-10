namespace Sqlol.Tables.Properties;

public interface IQueryChangesSeparator
{
    public IList<Tuple<string, string>> GetChangesFromQuery(string command, string query);
}