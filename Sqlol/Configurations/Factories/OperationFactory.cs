using Sqlol.Configurations.Factories.Operations;
using Sqlol.Configurations.Factories.Operations.Numbers;

namespace Sqlol.Configurations.Factories;

public class OperationFactory : IOperationFactory
{
    private Dictionary<string, IOperation> _operations = new Dictionary<string, IOperation>()
    {
        { "=", new EqualsOperation() },
        { "<>", new NotEqualsOperation() },
        { ">", new GreaterOperation() },
        { ">=", new GreaterEqualsOperation() },
        { "<", new LessOperation() },
        { "<=", new LessEqualsOperation() },
    };

    public IOperation GetOperation(string operation)
    {
        if (_operations.TryGetValue(operation, out var method))
            return method;
        throw new ApplicationException("command can't be found");
    }
}