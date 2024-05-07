using System.Text;

namespace Sqlol.Tables.Memory;

public class TableMemory : ITableMemory
{
    public void SaveHeader(ITable table, Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);

        stream.Write(table.HasMemoFile ? [131] : [3]);

        byte year = (byte)(table.LastUpdateDate.Year % 100);
        byte month = (byte)(table.LastUpdateDate.Month % 100);
        byte day = (byte)(table.LastUpdateDate.Day % 100);
        byte[] dateBuffer = [year, month, day];

        stream.Write(dateBuffer);
        
        byte[] recordsAmount = BitConverter.GetBytes(table.RecordsAmount);
        stream.Write(recordsAmount);
        
        byte[] lengthInBytes = BitConverter.GetBytes(table.HeaderLength);
        stream.Write(lengthInBytes);
        
        byte[] recordLength = BitConverter.GetBytes(table.RecordLength);
        stream.Write(recordLength);

        for (int i = 0; i < 20; i++)
            stream.Write([0]);

        foreach (var property in table.Properties)
        {
            string name = property.Name;
            while (name.Length < 11) name += '\0';
            
            stream.Write(Encoding.Unicode.GetBytes(name));
            stream.Write([(byte)property.Type]);
            stream.Write([(byte)property.Size]);
            stream.Write([(byte)property.Index]);
        }
    }
}