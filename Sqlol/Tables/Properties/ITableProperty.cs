namespace Sqlol.Tables.Properties;

public interface ITableProperty
{
    public string Name { get; }
    public char Type { get; }
    public byte Size { get; }
    public byte Index { get; }
    public byte Width { get; }
    public byte Precision { get; }
}