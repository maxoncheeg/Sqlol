namespace Sqlol.Configurations.TypesConfigurations;

public interface ITypeConfiguration
{
    public byte Width { get; }
    public byte Precision { get; }
    public byte SizeOffset { get; }

    public bool Validate(string value, byte width = 0, byte precision = 0);
}