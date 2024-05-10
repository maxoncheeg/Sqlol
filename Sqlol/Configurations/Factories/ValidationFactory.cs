using System.Text.RegularExpressions;

namespace Sqlol.Configurations.Factories;

public class ValidationFactory : IValidationFactory
{
    public bool Validate(string keyWord, string query)
    {
        bool result = false;
        
        switch (keyWord)
        {
            case "create":
                result = Regex.IsMatch(query, @"^create\stable\s\w{1,11}\s?(.+);?$");
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
                result = Regex.Match(query, @"\s\w{1,11}\s?\(").Value;
                result = result[..^1];
                result = result.Trim();
                break;
        }

        return result;
    }
}