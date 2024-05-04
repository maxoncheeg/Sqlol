namespace Sqlol.Configurations.Factories.Operations;

public interface IOperation
{
    public bool GetResult(string actual, string expected);
}