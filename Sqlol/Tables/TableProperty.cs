namespace Sqlol.Tables;

public struct TableProperty
{
    public string Name { get; private set; }
    public SqlolType Type { get; private set; }
    public byte Size { get; private set; }
    public byte Index { get; private set; }
}