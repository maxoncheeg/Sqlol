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
                result = Regex.IsMatch(query, @"^create\stable\s\w{1,11}\s?(.+)$");
                break;
        }

        return result;
    }
}