namespace Sqlol.Tables.Properties;

public class TableProperty : ITableProperty
{
    public string Name { get; set; }
    public char Type { get; }
    public byte Size { get; }
    public byte Index { get; set; }
    public byte Width { get; }
    public byte Precision { get; }

    public TableProperty(string name, char type, byte width, byte precision, byte index, byte size)
    {
        if (size < width + precision) throw new ArgumentException("size < width + precision");
        
        Name = name;
        Type = type;
        Width = width;
        Precision = precision;
        Index = index;
        Size = size;
    }
}