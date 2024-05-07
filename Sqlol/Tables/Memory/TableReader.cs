using Sqlol.Tables.Properties;

namespace Sqlol.Tables.Memory;

public class TableReader : ITableReader
{
    public ITable? CreateTable(IList<ITableProperty> properties, string tableName)
    {
        if (File.Exists(tableName + ".dbf"))
        {
            //error
            return null;
        }

        try
        {
            Stream stream = File.Create(tableName + ".dbf");
            ITable table = new StreamTable(new TableMemory(), tableName, stream, []);
            return table;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }

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