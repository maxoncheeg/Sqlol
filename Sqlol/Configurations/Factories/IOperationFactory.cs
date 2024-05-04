using Sqlol.Configurations.Factories.Operations;

namespace Sqlol.Configurations.Factories;

public interface IOperationFactory
{
    public IOperation GetOperation(string operation);
}