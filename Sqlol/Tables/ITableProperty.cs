namespace Sqlol.Tables;

public interface ITableProperty
{
    public string Name { get; }
    public string Type { get; }
    public byte Size { get; }
    public byte Index { get; }
    public byte Width { get; }
    public byte Precision { get; }

    public bool Validate(string value);
}