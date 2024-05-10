namespace Sqlol.Configurations.TypesConfigurations;

public abstract class AbstractTypeConfiguration(byte width, byte precision, byte sizeOffset) : ITypeConfiguration
{
    public byte Width { get; } = width;
    public byte Precision { get; } = precision;
    public byte SizeOffset { get; } = sizeOffset;

    public abstract bool Validate(string value, byte width = 0, byte precision = 0);
}