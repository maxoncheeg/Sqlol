using Sqlol.Expressions;

namespace Sqlol.Tests;

[TestClass]
public class CheckTest
{
    [TestMethod]
    public void PrimaryTest()
    {
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

        List<string> variables =
            ["z", "t", "x", "q", "k"];
        List<List<int>> records =
        [
            [2, 0, 0, 0, 0],
            [1, 6, 0, 0, 4],
            [1, 6, 0, 0, 2],
            [1, 2, 2, 2, 4],
            [1, 2, 2, 2, 2],
            [1, 2, 5, 2, 2],
        ];
        List<bool> results =
        [
            false, true, false, true, false, false
        ];

        //Print(result, records[0]);
        Console.WriteLine("z != 2 and ((t > 5) or (x<3 and q==2)) and (k == 4)");
        Check(result, variables, records, results);
    }
    
    [TestMethod]
    public void OrTest()
    {
        #region expression

        Filter f1 = new Filter("a", "3", "=", "or");
        Filter f2 = new Filter("b", "2", "=", "or");
        Filter f3 = new Filter("c", "1", "=", "and");
        Filter f4 = new Filter("d", "0", "=", "or");
        Filter f5 = new Filter("f", "105", "=", string.Empty);

        Expression result = new();
        result.Add(f1);
        result.Add(f2);
        result.Add(f3);
        result.Add(f4);
        result.Add(f5);
        #endregion

        List<string> variables =
            ["a", "b", "c", "d", "f"];
        List<List<int>> records =
        [
            [-1, 2, 1, 0, 105],
            [3, -1, 1, 0, 105],
            [3, 2, 1, 0, 103],
            [3, 2, 8, 0, 105],
            [3, 2, 1, 20, 105],
            [-1, -1, 1, 0, -1],
            [-1, -1, 2, 0, -1],
            [-1, -1, 1, -1, 105],
            [-1, 2, -1, 0, 6],
        ];
        List<bool> results =
        [
            true, true, true, true, true, true,
            false, true, true
        ];

        //Print(result, records[0]);
        Console.WriteLine("a == 3 or b == 2 or c == 1 and d == 0 or f == 105");
        Check(result, variables, records, results);
    }
    
    [TestMethod]
    public void OrAndTest()
    {
        #region expression

        Filter f1 = new Filter("a", "3", "=", "or");
        Filter f2 = new Filter("b", "2", "=", "or");
        Filter f3 = new Filter("c", "1", "=", "and");
        Filter f4 = new Filter("d", "0", "=", string.Empty);
        
        Expression result = new();
        result.Add(f1);
        result.Add(f2);
        result.Add(f3);
        result.Add(f4);
        #endregion

        List<string> variables =
            ["a", "b", "c", "d"];
        List<List<int>> records =
        [
            [-1, 2, 1, 0],
            [3, -1, 1, 0],
            [3, 2, 1, 0],
            [3, 2, 8, 0],
            [3, 2, 1, 20],
            [-1, -1, 1, 0],
            [-1, -1, 2, 0],
            [-1, -1, 1, -1],
            [-1, 2, -1, 0],
        ];
        List<bool> results =
        [
            true, 
            true, 
            true, 
            true, 
            true, 
            true,
            false, 
            false, 
            true
        ];

        //Print(result, records[0]);
        Console.WriteLine("a == 3 or b == 2 or c == 1 and d == 0");
        Check(result, variables, records, results);
    }
    
    [TestMethod]
    public void DenixTest()
    {
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

        List<string> variables =
            ["x", "y", "z", "w"];
        List<List<int>> records =
        [
            [1, 1, 1, 1],
            [1, 0, 1, 1],
            [1, 0, 0, 1],
            [0, 0, 1, 1],
            [0, 0, 0, 0],
        ];
        List<bool> results =
        [
            true, 
            true, 
            false,
            true,
            false
        ];

        //Print(result, records[0]);
        Console.WriteLine("x == 1 and (y == 1 or (z == 1 and x == 1)) or (z == 1 and w == 1)");
        Check(result, variables, records, results);
    }
    
    [TestMethod]
    public void XorTest()
    {
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

        List<string> variables =
            ["a", "b", "c", "d"];
        List<List<int>> records =
        [
            [1, 1, 1, 1],
            [0, 1, 1, 0],
            [1, 1, 0, 1],
            [0, 1, 0, 0],
            [0, 0, 1, 1],
            [0, 0, 0, 0],
        ];
        List<bool> results =
        [
            true, 
            true, 
            true, 
            true,
            true,
            false
        ];

        //Print(result, records[0]);
        Console.WriteLine("a == 1 or b == 1 xor c == 1 and d == 1");
        Check(result, variables, records, results);
    }
    
    [TestMethod]
    public void XorMegaTest()
    {
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

        List<string> variables =
            ["a", "b", "c", "d"];
        List<List<int>> records =
        [
            [1, 1, 1, 1],
            [0, 1, 1, 0],
            [1, 1, 0, 1],
            [0, 1, 0, 0],
            [0, 0, 1, 1],
            [0, 0, 0, 0],
        ];
        List<bool> results =
        [
            true, 
            true, 
            true, 
            true,
            true,
            false
        ];

        Print(result, records[0]);
        Console.WriteLine("(a == 1) or (b == 1) xor (c == 1) and (d == 1)");
        Check(result, variables, records, results);
    }
    
    private bool Print(Expression expression, List<int> record)
    {
        for (int i = 0; i < expression.Entities.Count; i++)
        {
            if (expression.Entities[i] is Filter f)
            {
                Console.WriteLine(f.Field + " " + f.Operation + " " + f.Value);
                if (f.Next != string.Empty) Console.WriteLine(f.Next);
            }
            else if (expression.Entities[i] is Expression e)
            {
                Print(e, record);
                Console.WriteLine(e.Next);
            }
        }

        return true;
    }

    private void Check(Sqlol.Expressions.Expression ex, List<string> variables, List<List<int>> records,
        List<bool> results)
    {
        global::Expressions e = new();
        variables.ForEach(x => Console.Write(x.ToString() + " "));
        Console.WriteLine();
        for (int i = 0; i < records.Count; i++)
        {
            bool res = e.Check(ex, variables, records[i]);
            records[i].ForEach(x => Console.Write(x.ToString() + " "));
            Console.Write("\t" + res);
            Assert.AreEqual(res, results[i]);
            Console.WriteLine();
        }
    }
}