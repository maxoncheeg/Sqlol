namespace Sqlol.Tables.Memory;

public interface ITableMemory
{
    public void SaveHeader(ITable table, Stream stream);
}