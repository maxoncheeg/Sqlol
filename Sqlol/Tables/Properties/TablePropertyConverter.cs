using System.Text.RegularExpressions;
using Sqlol.Configurations;

namespace Sqlol.Tables.Properties;

public class TablePropertyConverter : ITablePropertyConverter
{
    private readonly string _types;
    private readonly IKeyWordsConfiguration _configuration;

    public TablePropertyConverter(IKeyWordsConfiguration configuration)
    {
        _configuration = configuration;
        _types = @"[";
        foreach (var type in configuration.Types)
            _types += type.Key;
        _types += @"]";
    }

    public string[] GetStringProperties(string query)
    {
        //query = Regex.Match(query, @"\(.+\)").Value;
        string pattern = @"\w{1,11}\s+";
        pattern += _types;
        pattern += @"\s*(\(\s*\d+\s*,\s*\d+\s*\)|\(\s*\d+\s*\)|)";
        var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        return matches.Select(x => x.Value).ToArray();
    }

    public IList<ITableProperty> Convert(string properties)
    {
        return Convert(GetStringProperties(properties));
    }

    public IList<ITableProperty> Convert(string[] properties)
    {
        List<ITableProperty> result = [];
        byte index = 0;
        foreach (var item in properties)
        {
            string name = item.Substring(0, item.IndexOf(' '));
            char type = item.Substring(item.IndexOf(' ') + 1, 1).ToUpperInvariant()[0];
            string width = string.Empty, precision = string.Empty;
            if (item.Contains('('))
            {
                int openBracket = item.IndexOf('(');
                int closeBracket = item.IndexOf(')');
                int comma = item.IndexOf(',');
                
                width = item.Substring(openBracket + 1,
                    comma > 0 ? comma - openBracket - 1 : closeBracket - openBracket - 1);
                if (comma > 0)
                    precision = item.Substring(comma + 1,  closeBracket - comma - 1);
            }
             
            bool wRes = byte.TryParse(width, out var w);
            bool pRes = byte.TryParse(precision, out var p);
            byte size = (byte)((wRes ? w : 0) + (pRes ? p : 0) + _configuration.Types[type].SizeOffset);
           // Console.Write(type + " " + w + " " + p + " " + size);
            
            ITableProperty property = new TableProperty(name, type, (wRes ? w : (byte)0), (pRes ? p : (byte)0), index++, size);
            result.Add(property);
        }

        return result;
    }
}