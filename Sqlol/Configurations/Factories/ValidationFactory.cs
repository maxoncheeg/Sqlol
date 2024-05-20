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

    public string? Validate(string keyWord, string query)
    {
        bool result = false;
        string? queryKey = keyWord.ToLowerInvariant();

        switch (queryKey)
        {
            case "create":
                result = Regex.IsMatch(query, @"^create\stable\s\w+\s?\(.+\);?$", RegexOptions.IgnoreCase);
                if (!result) break;
                string temp = Regex.Match(query, @"\(.+\)").Value;
                result = Regex.IsMatch(temp, @"^\(\s*" + _typePattern + @"\s*(|(,\s*" + _typePattern + @"\s*)+)\)$",
                    RegexOptions.IgnoreCase);
                break;
            case "select":
                result = Regex.IsMatch(query,
                    @"^select\s+(\*|\w{1,11}\s*(|,\s*\w{1,11}\s*)*)\s+from\s+\w+(|\s+where\s+.+)$",
                    RegexOptions.IgnoreCase);
                break;
            case "delete":
                result = Regex.IsMatch(query, @"^delete\s+from\s+\w+(|\s+where\s+.+)$", RegexOptions.IgnoreCase);
                break;
            case "insert":
                result = Regex.IsMatch(query, @"^insert\s+into\s+\w+\s*\(.+\)\s*values\s*\(.+\)$",
                    RegexOptions.IgnoreCase);
                break;
            case "open":
                result = Regex.IsMatch(query, @"^open\s+\w+$", RegexOptions.IgnoreCase);
                break;
            case "restore":
                result = Regex.IsMatch(query, @"^restore\s+\w+$", RegexOptions.IgnoreCase);
                break;
            case "truncate":
                result = Regex.IsMatch(query, @"^truncate\s+\w+$", RegexOptions.IgnoreCase);
                break;
            case "close":
                result = Regex.IsMatch(query, @"^close\s+\w+$", RegexOptions.IgnoreCase);
                break;
            case "update":
                result = Regex.IsMatch(query, @"^update\s+\w+\s+set\s+(.+)(|\s+where\s+.+)$", RegexOptions.IgnoreCase);
                break;
            case "alter":
                result = Regex.IsMatch(query, @"^alter\s+table\s+\w+\s+column\s+.+$", RegexOptions.IgnoreCase);
                if (result)
                {
                    query = query.Substring(query.IndexOf("column", StringComparison.Ordinal));
                    if (Regex.IsMatch(query, $@"^column\s+add\s+{_typePattern}$", RegexOptions.IgnoreCase))
                        queryKey = "columnAdd";
                    else if (Regex.IsMatch(query, @"^column\s+remove\s+\w{1,11}$", RegexOptions.IgnoreCase))
                        queryKey = "columnRemove";
                    else if (Regex.IsMatch(query, @"^column\s+rename\s+\w{1,11}\s+\w{1,11}$", RegexOptions.IgnoreCase))
                        queryKey = "columnRename";
                    else if (Regex.IsMatch(query, @"^column\s+update\s+\w{1,11}\s+" + $@"{_typePattern}$",
                                 RegexOptions.IgnoreCase)) queryKey = "columnUpdate";
                    else result = false;
                }

                break;
        }

        if (result == false) queryKey = null;

        return queryKey;
    }

    public string GetTableName(string keyWord, string query)
    {
        string result = string.Empty;

        switch (keyWord.ToLowerInvariant())
        {
            case "create":
            case "insert":
                result = Regex.Match(query, @"\s+\w+\s*\(").Value;
                result = Regex.Match(result, @"\w+").Value;
                result = result.Trim();
                break;
            case "open":
            case "restore":
            case "truncate":
            case "update":
            case "close":
                result = Regex.Match(query, @"\s+\w+").Value;
                result = Regex.Match(result, @"\w+").Value;
                result = result.Trim();
                break;
            case "select":
            case "delete":
                result = Regex.Match(query, @"from\s+\w+").Value;
                result = result.Replace("from", "").Trim();
                break;
            case "alter":
                result = Regex.Match(query, @"^alter\s+table\s+\w+").Value;
                result = Regex.Match(result, @"\w+$").Value;
                break;
        }

        return result;
    }
}