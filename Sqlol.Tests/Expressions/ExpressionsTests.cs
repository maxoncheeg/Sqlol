using Sqlol.Configurations;
using Sqlol.Expressions;
using Sqlol.Expressions.Builders;

namespace Sqlol.Tests.Expressions;

[TestClass]
public class ExpressionsTests
{
    [TestMethod]
    public void ExpressionBuilder_First_Test()
    {
        string condition = "(a = 1) or (b = 1) xor (c = 1) and (d = 1)";
        Console.WriteLine(condition);

        IExpressionBuilder builder = new ExpressionBuilder(new KeyWordsConfiguration());
        
        #region expression
        Filter f1 = new Filter("a", "1", "=", string.Empty);
        Filter f2 = new Filter("b", "1", "=", string.Empty);
        Filter f3 = new Filter("c", "1", "=", string.Empty);
        Filter f4 = new Filter("d", "1", "=", string.Empty);
        
        Expression e1 = new("or");
        e1.Add(f1);

        Expression e2 = new("xor");
        e2.Add(f2);

        Expression e3 = new("and");
        e3.Add(f3);

        Expression e4 = new();
        e4.Add(f4);

        Expression result = new();
        result.Add(e1);
        result.Add(e2);
        result.Add(e3);
        result.Add(e4);
        #endregion
        
        Assert.IsTrue(CheckExpression(builder.TranslateToExpression(condition), result));
    }
    
    [TestMethod]
    public void ExpressionBuilder_Second_Test()
    {
        string condition = "a = 1 or b = 1 xor c = 1 and d = 1";
        Console.WriteLine(condition);

        IExpressionBuilder builder = new ExpressionBuilder(new KeyWordsConfiguration());
        
        #region expression
        Filter f1 = new Filter("a", "1", "=", "or");
        Filter f2 = new Filter("b", "1", "=", "xor");
        Filter f3 = new Filter("c", "1", "=", "and");
        Filter f4 = new Filter("d", "1", "=", string.Empty);
        
        Expression result = new();
        result.Add(f1);
        result.Add(f2);
        result.Add(f3);
        result.Add(f4);
        #endregion
        
        Assert.IsTrue(CheckExpression(builder.TranslateToExpression(condition), result));
    }
    
    [TestMethod]
    public void ExpressionBuilder_Third_Test()
    {
        string condition = "x = 1 and (y = 1 or (z = 1 and x = 1)) or (z = 1 and w = 1)";
        Console.WriteLine(condition);

        IExpressionBuilder builder = new ExpressionBuilder(new KeyWordsConfiguration());

        #region expression

        Filter f1 = new Filter("x", "1", "=", "and");
        
        
        Filter f2 = new Filter("y", "1", "=", "or");
        
        Filter f3 = new Filter("z", "1", "=", "and");
        Filter f4 = new Filter("x", "1", "=");


        Expression e1 = new();
        e1.Add(f3);
        e1.Add(f4);

        Expression e2 = new("or");
        e2.Add(f2);
        e2.Add(e1);

        Expression result = new();
        result.Add(f1);
        result.Add(e2);
        
        Expression e4 = new();
        Filter f5 = new Filter("z", "1", "=", "and");
        Filter f6 = new Filter("w", "1", "=", string.Empty);
        e4.Add(f5);
        e4.Add(f6);
        
        result.Add(e4);

        #endregion
        
        Assert.IsTrue(CheckExpression(builder.TranslateToExpression(condition), result));
    }
    
        
    [TestMethod]
    public void ExpressionBuilder_Fourth_Test()
    {
        string condition = "z<>2 and((t>5)or(x<3 and q=2))and(k=4)";
        Console.WriteLine(condition);

        IExpressionBuilder builder = new ExpressionBuilder(new KeyWordsConfiguration());

        #region expression

        Filter f1 = new Filter("x", "3","<", "and");
        Filter f2 = new("q", "2", "=");
        Expression expression1 = new();

        expression1.Add(f1);
        expression1.Add(f2);

        Filter f3 = new("t", "5",">");
        Expression expression2 = new("or");

        expression2.Add(f3);


        Expression expression3 = new("and");
        expression3.Add(expression2);
        expression3.Add(expression1);


        Expression result = new();
        result.Add(new Filter("z", "2", "<>", "and"));
        result.Add(expression3);

        Expression expression5 = new();
        expression5.Add(new Filter("k", "4", "=", string.Empty));

        result.Add(expression5);

        #endregion
        
        Assert.IsTrue(CheckExpression(builder.TranslateToExpression(condition), result));
    }
    
    [TestMethod]
    public void ExpressionBuilder_Fifth_Test()
    {
        string condition = "amerikanecCounter > 15 and russianName = \"полный  сосач\" or (americLol = true)";
        Console.WriteLine(condition);

        IExpressionBuilder builder = new ExpressionBuilder(new KeyWordsConfiguration());

        #region expression

        Filter f1 = new Filter("amerikanecCounter", "15",">", "and");
        Filter f2 = new("russianName", "\"полный  сосач\"", "=", "or");


        Filter f3 = new("americLol", "true","=");
        Expression expression1 = new([f3]);


        Expression result = new([f1,f2,expression1]);

        #endregion
        
        Assert.IsTrue(CheckExpression(builder.TranslateToExpression(condition), result));
    }

    private bool CheckExpression(IExpression actual, IExpression expected) {
        var res = true;
        for (var i = 0; i < expected.Entities.Count; i++) {
            if (i >= actual.Entities.Count)
                return false;
            
            if (expected.Entities[i] is IFilter expectedFilter && actual.Entities[i] is IFilter actualFilter)
            {
                res = expectedFilter.Field == actualFilter.Field &&
                       expectedFilter.Operation == actualFilter.Operation &&
                       expectedFilter.Value == actualFilter.Value && 
                       expectedFilter.Next == actualFilter.Next;
            }
            else if (expected.Entities[i] is IExpression expectedExpression &&
                     actual.Entities[i] is IExpression actualExpression)
            {
                if (expectedExpression.Next != actualExpression.Next) return false;
                res = CheckExpression(expectedExpression, actualExpression);
                //Console.WriteLine(e.Next);
            }
            
            if (!res)
                return false;
        }

        return true;
    }
}