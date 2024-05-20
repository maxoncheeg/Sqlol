namespace Sqlol.Configurations.Factories;

public interface IValidationFactory
{
    public string? Validate(string keyWord, string query);
    public string GetTableName(string keyWord, string query);
}