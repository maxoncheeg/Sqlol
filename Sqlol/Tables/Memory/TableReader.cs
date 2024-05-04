namespace Sqlol.Tables.Memory;

public class TableReader : ITableReader
{
    public ITable? ReadTable(string fileName)
    {
        try
        {
            Stream stream = File.Open(fileName, FileMode.Open)
        }
    }
}