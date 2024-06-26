﻿using System.Text.RegularExpressions;

namespace Sqlol.Configurations.TypesConfigurations;

public class CTypeConfiguration(byte width, byte precision, byte sizeOffset)
    : AbstractTypeConfiguration(width, precision, sizeOffset)
{
    public override bool Validate(string v, byte width = 1, byte precision = 0)
    {
        string value = (string)v.Clone();
        if (value[0] != '"' || value[^1] != '"') return false;
        value = value[1..^1];
        if (value.Length > Width + Precision + SizeOffset) return false;
        if (width > Width) return false;
        if (precision > 0) throw new ArgumentException("C type has not precision");
        if (width <= 0) throw new ArgumentException("width cannot be less than zero");

        string pattern = @"^.{0,";
        pattern += width;
        pattern += @"}$";

        return Regex.Match(value, pattern).Success;
    }
}