namespace Sqlol.Configurations.Factories.Operations;

public class NotEqualsOperation : IOperation
{
    public bool GetResult(string actual, string expected)
    {
        return !actual.Equals(expected);
    }
}