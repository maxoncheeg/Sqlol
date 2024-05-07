namespace Sqlol.Configurations.Factories;

public interface IValidationFactory
{
    public bool Validate(string keyWord, string query);
}