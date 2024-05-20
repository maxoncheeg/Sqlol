namespace Sqlol.Tables.Properties;

public interface ITableProperty
{
    public string Name { get; set; }
    public char Type { get; }
    public byte Size { get; }
    public byte Index { get; set; }
    public byte Width { get; }
    public byte Precision { get; }
}