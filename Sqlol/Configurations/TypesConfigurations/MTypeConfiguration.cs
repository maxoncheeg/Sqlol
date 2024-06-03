using System.Text.RegularExpressions;

namespace Sqlol.Configurations.TypesConfigurations;

public class MTypeConfiguration(byte width, byte precision, byte sizeOffset)
    : AbstractTypeConfiguration(width, precision, sizeOffset)
{
    public override bool Validate(string v, byte width = 0, byte precision = 0)
    {
        string value = (string)v.Clone();
        if (value[0] != '"' || value[^1] != '"') return false;
        value = value[1..^1];

        string pattern = @"^.{0,512}";

        return Regex.Match(value, pattern).Success;
    }
}