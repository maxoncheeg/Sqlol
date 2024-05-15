using System.Text.RegularExpressions;

namespace Sqlol.Configurations.Factories;

public class ValidationFactory : IValidationFactory
{
    private string _types = "";
    private string _typePattern = "";

    public ValidationFactory(IKeyWordsConfiguration configuration)
    {
        _types = @"[";
        foreach (var type in configuration.Types)
            _types += type.Key;
        _types += @"]";

        _typePattern = @"\w{1,11}\s+";
        _typePattern += _types;
        _typePattern += @"\s*(\(\s*\d+\s*,\s*\d+\s*\)|\(\s*\d+\s*\)|)";
    }

    public bool Validate(string keyWord, string query)
    {
        bool result = false;

        switch (keyWord)
        {
            case "create":
                result = Regex.IsMatch(query, @"^create\stable\s\w+\s?\(.+\);?$", RegexOptions.IgnoreCase);
                if (!result) break;
                string temp = Regex.Match(query, @"\(.+\)").Value;
                result = Regex.IsMatch(temp, @"^\(\s*" + _typePattern + @"\s*(|(,\s*" + _typePattern + @"\s*)+)\)$", RegexOptions.IgnoreCase);
                break;
            case "select":
                result = Regex.IsMatch(query, @"^select\s+(\*|\w{1,11}\s*(|,\s*\w{1,11}\s*)*)\s+from\s+\w+(|\s+where\s+.+)$", RegexOptions.IgnoreCase);
                break;
            case "insert":
                result = Regex.IsMatch(query, @"^insert\s+into\s+\w+\s*\(.+\)\s*values\s*\(.+\)$",
                    RegexOptions.IgnoreCase);
                break;
            case "open":
                result = Regex.IsMatch(query, @"^open\s+\w+$", RegexOptions.IgnoreCase);
                break;
            case "close":
                result = Regex.IsMatch(query, @"^close\s+\w+$", RegexOptions.IgnoreCase);
                break;
        }

        return result;
    }

    public string GetTableName(string keyWord, string query)
    {
        string result = string.Empty;

        switch (keyWord)
        {
            case "create":
            case "insert":
                result = Regex.Match(query, @"\s+\w+\s*\(").Value;
                result = Regex.Match(result, @"\w+").Value;
                result = result.Trim();
                break;
            case "open":
            case "close":
                result = Regex.Match(query, @"\s+\w+").Value;
                result = Regex.Match(result, @"\w+").Value;
                result = result.Trim();
                break;
            case "select":
                result = Regex.Match(query, @"from\s+\w+").Value;
                result = result.Replace("from", "").Trim();
                break;
        }

        return result;
    }
}