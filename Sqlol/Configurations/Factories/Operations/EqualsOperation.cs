namespace Sqlol.Configurations.Factories.Operations;

public class EqualsOperation : IOperation
{
    public bool GetResult(string actual, string expected)
    {
        return actual.Equals(expected);
    }
}