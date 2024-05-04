namespace Sqlol.Tables;

public class TableHeader
{
    public bool HasMemoFile { get; private set; }
    public DateTime LastUpdateDate { get; private set; }
    public int RecordsAmount { get; private set; }
    public short HeaderLength { get; private set; }
    public short RecordLength { get; private set; }
    //резервы 3 13 4 байт
    public IReadOnlyList<TableProperty> Properties { get; }
    
}

