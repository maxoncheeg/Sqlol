// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.RegularExpressions;
using Sqlol.Configurations;
using Sqlol.Configurations.Factories;
using Sqlol.Expressions;
using Sqlol.Expressions.Builders;

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
                    if (orWait)
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
                    if (orWait)
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
        // Console.WriteLine("Допустим задали число с 5 цифрами в целой части и 4 после запятой");
        //
        // int a = 5, b = 4;
        //
        //
        // Regex regex = new(@"\d{" + a + @"}[.]\d{" + b + @"}", RegexOptions.Compiled);
        // //Console.WriteLine(regex.Match("055878345.128884")); // 78345.1288
        //
        string query = "gsdf select * from table where something asd";

        Console.WriteLine(Regex.Match(query, @"select.+from.+where").Success);
        Console.WriteLine(Regex.Match(query, @"^select.+from.+where.+$").Success);

        // "select * from table";
        // "select dasfads, fasf from table";
        // "select dasfads, fasf from table where dasdas = dsa or sda and";

        // IKeyWordsConfiguration configuration = new KeyWordsConfiguration();
        // IOperationFactory operationFactory = new OperationFactory();
        // IExpressionBuilder builder = new ExpressionBuilder();
        //

    }
}