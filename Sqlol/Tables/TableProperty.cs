namespace Sqlol.Tables;

public class TableProperty
{
    public string Name { get; private set; }
    public string Type { get; private set; }
    public byte Size { get; private set; }
    public byte Index { get; private set; }
}