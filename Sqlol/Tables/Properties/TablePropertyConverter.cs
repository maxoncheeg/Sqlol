using System.Text.RegularExpressions;
using Sqlol.Configurations;

namespace Sqlol.Tables.Properties;

public class TablePropertyConverter : ITablePropertyConverter
{
    private readonly string _types;
    
    public TablePropertyConverter(IKeyWordsConfiguration configuration)
    {
        _types = @"[";
        foreach (var type in configuration.Types)
            _types += type.ToUpper() + type.ToLower();
        _types += @"]";
    }
    
    public string[] GetStringProperties(string query)
    {
        string pattern = @"\w{1,11}\s";
        pattern += _types;
        pattern += @"\s?(\(\d+,\d+\)|\(\d+\)|)";
            //Console.WriteLine(pattern);
        var matches = Regex.Matches(query, pattern);
        return matches.Select(x => x.Value).ToArray();
    }

    public IList<ITableProperty> Convert(string properties)
    {
        throw new NotImplementedException();
    }

    public IList<ITableProperty> Convert(string[] properties)
    {
        List<ITableProperty> result = [];
        foreach (var item in properties)
        {
            string name = item.Substring(0, item.IndexOf(' '));
            char type = item.Substring(1, item.IndexOf(' '))[0];
            
        }
    }
}