using System.Text.RegularExpressions;

namespace Sqlol.Configurations.TypesConfigurations;

public class LTypeConfiguration(byte width, byte precision, byte sizeOffset)
    : AbstractTypeConfiguration(width, precision, sizeOffset)
{
    public override bool Validate(string value, byte width = 0, byte precision = 0)
    {
        if (value.Length > Width + Precision + SizeOffset) return false;
        if (width > Width) return false;
        if (precision > 0) throw new ArgumentException("D type has not precision");
        if (width <= 0) throw new ArgumentException("width cannot be less than zero");

        string pattern = @"^[tfyn\?]$";

        return Regex.Match(value, pattern, RegexOptions.IgnoreCase).Success;
    }
}