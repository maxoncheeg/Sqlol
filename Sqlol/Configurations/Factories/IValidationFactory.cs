namespace Sqlol.Configurations.Factories;

public interface IValidationFactory
{
    public bool Validate(string keyWord, string query);
    public string GetTableName(string keyWord, string query);
}