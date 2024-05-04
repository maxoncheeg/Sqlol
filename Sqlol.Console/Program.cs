// See https://aka.ms/new-console-template for more information

using Sqlol.Configurations.Factories;
using Sqlol.Expressions;

public class Expressions
{
    public bool Check(Expression expression, List<string> variables, List<int> record)
    {
        bool prevResult = false;
        bool prevOr = false;
        bool orWait = false;
        bool prevXor = false;
        bool xorWait = false;
        string next = string.Empty;
        for (int i = 0; i < expression.Entities.Count; i++)
        {
            if (expression.Entities[i] is Filter f)
            {
                var index = variables.IndexOf(f.Field);
                bool result = false;

                Console.Write(record[index]);
                result = new OperationFactory().GetOperation(f.Operation).GetResult(record[index].ToString(), f.Value);
                bool r = result;

                Console.Write(" - " + f.Field + " " + result);
                if (next != string.Empty)
                {
                    result = next switch
                    {
                        "and" => result && prevResult,
                        "or" => result || prevResult,
                        _ => result
                        //SqlolLogicalOperation.Xor => (result && !prevResult) || (!result && prevResult)
                    };
                }
                
                
                Console.WriteLine(" " + next + " " + result);
                

                if (f.Next == "or")
                {
                    if(orWait)
                    {
                        result = prevOr || result;
                        orWait = false;
                    }
                    else
                    {
                        prevOr = result;
                    }

                    if (xorWait)
                    {
                        result = (result && !prevXor) || (!result && prevXor);
                        xorWait = false;
                    }
                }
                else if (f.Next == "and" || f.Next == "xor") orWait = true;
                
                if (f.Next == "xor")
                {
                    prevXor = r;
                    xorWait = true;
                }

               // if (next == SqlolLogicalOperation.Xor) prevXor = result;

                next = f.Next;
                prevResult = result;
            }
            else if (expression.Entities[i] is Expression e)
            {
                bool result = Check(e, variables, record);
                if (next != String.Empty)
                {
                    result = next switch
                    {
                        "and" => result && prevResult,
                        "or" => result || prevResult,
                        _ => result
                    };
                }

                bool r = result;
                
                if (e.Next == "or")
                {
                    if(orWait)
                    {
                        result = prevOr || result;
                        orWait = false;
                    }
                    else
                    {
                        prevOr = result;
                    }

                    if (xorWait)
                    {
                        result = (result && !prevXor) || (!result && prevXor);
                        xorWait = false;
                    }
                }
                else if (e.Next == "and" || e.Next == "xor") orWait = true;
                
                if (e.Next == "xor")
                {
                    prevXor = r;
                    xorWait = true;
                }

                next = e.Next;
                prevResult = result;
            }
        }

        if (xorWait)
        {
            Console.WriteLine(prevResult + " " + prevXor);
            prevResult = (prevResult && !prevXor) || (!prevResult && prevXor);
            Console.WriteLine(prevResult + " " + prevXor);
        }
        if (orWait)
        {
            prevResult = prevOr || prevResult;
        }
        return prevResult;
    }
}

internal class Program
{
     public static void Main(string[] args)
     {
//         Filter f1 = new Filter("x", "3", SqlolElementaryOperation.Less, SqlolLogicalOperation.And);
//         Filter f2 = new("q", "2", SqlolElementaryOperation.Equals);
//         Expression expression1 = new();
//
//         expression1.Entities.Add(f1);
//         expression1.Entities.Add(f2);
//
//         Filter f3 = new("t", "5", SqlolElementaryOperation.Greater);
//         Expression expression2 = new();
//
//         expression2.Entities.Add(f3);
//         expression2.Next = SqlolLogicalOperation.Or;
//
//
//         Expression expression3 = new();
//         expression3.Entities.Add(expression2);
//         expression3.Entities.Add(expression1);
//
//
//         Expression result = new();
//         result.Entities.Add(new Filter("z", "2", SqlolElementaryOperation.NotEquals, SqlolLogicalOperation.And));
//         expression3.Next = SqlolLogicalOperation.And;
//         result.Entities.Add(expression3);
//
//         Expression expression5 = new();
//         expression5.Entities.Add(new Filter("k", "4", SqlolElementaryOperation.Equals, SqlolLogicalOperation.None));
//
//         result.Entities.Add(expression5);
//
//         Console.WriteLine(result);
//
//
// //z != 2 and ((t > 5) or (x<3 and q==2)) and (k == 4)
//         List<string> variables =
//             ["z", "t", "x", "q", "k"];
//         List<List<int>> records =
//         [
//             [2, 0, 0, 0, 0],
//             [1, 6, 0, 0, 4],
//             [1, 6, 0, 0, 2],
//             [1, 2, 2, 2, 4],
//             [1, 2, 2, 2, 2],
//             [1, 2, 5, 2, 2],
//         ];
//
//         Expressions e = new();
//         Print(result, records[0]);
//         variables.ForEach(x => Console.Write(x.ToString() + " "));
//         Console.WriteLine();
//         Console.WriteLine("z != 2 and ((t > 5) or (x<3 and q==2)) and (k == 4)");
//         for (int i = 0; i < records.Count; i++)
//         {
//             records[i].ForEach(x => Console.Write(x.ToString() + " "));
//             Console.Write("\t" + e.Check(result, variables, records[i]));
//             Console.WriteLine();
//         }
    }

    // static bool Print(Expression expression, List<int> record)
    // {
    //     for (int i = 0; i < expression.Entities.Count; i++)
    //     {
    //         if (expression.Entities[i] is Filter f)
    //         {
    //             Console.WriteLine(f.Field + " " + f.Operation + " " + f.Value);
    //             if (f.Next != SqlolLogicalOperation.None) Console.WriteLine(f.Next);
    //         }
    //         else if (expression.Entities[i] is Expression e)
    //         {
    //             Print(e, record);
    //             Console.WriteLine(e.Next);
    //         }
    //     }
    //
    //     return true;
    // }
}