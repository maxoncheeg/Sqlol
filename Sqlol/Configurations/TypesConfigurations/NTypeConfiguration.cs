using System.Text.RegularExpressions;

namespace Sqlol.Configurations.TypesConfigurations;

public class NTypeConfiguration(byte width, byte precision, byte sizeOffset)
    : AbstractTypeConfiguration(width, precision, sizeOffset)
{
    public override bool Validate(string value, byte width = 1, byte precision = 0)
    {
        if (value.Length > Width + Precision + SizeOffset) return false;
        if (width > Width) return false;
        if (precision > Precision) return false;
        if (width <= 0) throw new ArgumentException("width cannot be less than zero");

        string pattern = @"^[\+\-]?\d{1,";
        pattern += width;

        if (precision > 0)
        {
            pattern += @"}(|.\d{1,";
            pattern += precision;
            pattern += @"})$";
        }
        else
            pattern += @"}$";

        return Regex.Match(value, pattern).Success;
    }
}