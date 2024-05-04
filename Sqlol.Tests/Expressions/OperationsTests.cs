using Sqlol.Configurations.Factories.Operations;
using Sqlol.Configurations.Factories.Operations.Numbers;

namespace Sqlol.Tests.Expressions;

[TestClass]
public class OperationsTests
{

    [TestMethod]
    public void GreaterOperation_Equals_Test()
    {
        string actual = "1234.4321";
        string expected = "1234.4321";

        IOperation operation = new GreaterOperation();
        
        Assert.IsFalse(operation.GetResult(actual, expected));
    }
    
    [TestMethod]
    public void GreaterOperation_Greater_Test()
    {
        string actual = "1254.4321";
        string expected = "1234.4321";

        IOperation operation = new GreaterOperation();
        
        Assert.IsTrue(operation.GetResult(actual, expected));
    }
    
    [TestMethod]
    public void GreaterOperation_Less_Test()
    {
        string actual = "1234.4321";
        string expected = "1234.4521";

        IOperation operation = new GreaterOperation();
        
        Assert.IsFalse(operation.GetResult(actual, expected));
    }
    
    [TestMethod]
    public void GreaterOperation_Float_Test()
    {
        string actual = "1234.4321";
        string expected = "123.4321";

        IOperation operation = new GreaterOperation();
        
        Assert.IsTrue(operation.GetResult(actual, expected));
    }
}