namespace Sqlol.Tables.Memory;

public class TableReader : ITableReader
{
    public ITable? CreateTable(IList<ITableProperty> properties)
    {
        throw new NotImplementedException();
    }

    public ITable? ReadTable(string fileName)
    {
        // try
        // {
        //     Stream stream = File.Open(fileName, FileMode.Open)
        // }
        return null;
    }
}