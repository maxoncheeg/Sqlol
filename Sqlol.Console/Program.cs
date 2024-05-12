// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.RegularExpressions;
using Sqlol.Configurations;
using Sqlol.Configurations.Factories;
using Sqlol.Configurations.TypesConfigurations;
using Sqlol.Expressions;
using Sqlol.Expressions.Builders;
using Sqlol.Loggers;
using Sqlol.Queries;
using Sqlol.Queries.Methods;
using Sqlol.Tables.Properties;

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

public class B
{
    public int X { get; set; }
}

public class A
{
    public void B(B b)
    {
        b = new B() { X = 10 };
    }
}

internal class Program
{
    public static void Print(IExpression e, int c = 0) {
        foreach (var x in e.Entities) {
            if (x is IFilter f) {
                Console.WriteLine($"{new string('\t', c)}{f.Field} {f.Operation} {f.Value} {f.Next}");
            }
            else if (x is IExpression a) {
                Print(a, c + 1);
                Console.WriteLine(x.Next);
            }
        }
    }
    
    public static void Main(string[] args)
    {
        // IQueryManager manager = new QueryManager(reader, validationFactory);
        // Console.WriteLine(manager.CreateTable("create sqlol_primary_table"));

        IQueryChangesSeparator separator = new QueryChangesSeparator();
        IKeyWordsConfiguration configuration = new KeyWordsConfiguration();
        var condition = "z<>2 and((t>5)or(x<3 and q=2))and(k=4)";
        var b = new ExpressionBuilder(configuration);
        
        try {
            var x = b.TranslateToExpression(condition);
            // Print(x);
        }
        catch (Exception e) {
            Console.WriteLine("Error: " + e.Message);
        }
        

        ITablePropertyConverter converter = new TablePropertyConverter(configuration);
        IQueryFactory queryFactory = new QueryFactory(new()
        {
            { "create", new CreateQuery(converter, logger, validationFactory) },
            { "insert", new InsertQuery(configuration, separator, logger) },
            
        });

        // IValidationFactory validationFactory = new ValidationFactory();
        // ILogger logger = new SimpleLogger((t, m) => { Console.WriteLine($"{t}: {m}"); });
        //
        // ITablePropertyConverter converter = new TablePropertyConverter(configuration);
        // IQueryFactory queryFactory = new QueryFactory(converter, validationFactory, logger);
        //
        // IQueryManager manager = new QueryManager(validationFactory, queryFactory);
        //
        // //запрос
        // string query = "create table russia (x C (20), y N (3,5), z L)";
        // Console.WriteLine(query);
        // //проверяем валидацию
        // Console.WriteLine(validationFactory.Validate("create", query) + "\nСвойства:\n");
        //
        // // //разбираем на поля
        // string[] properties = converter.GetStringProperties(query);
        // foreach (var property in properties)
        //     Console.WriteLine(property);
        //
        // Console.WriteLine("Имя таблицы " + validationFactory.GetTableName("create", query));
        //
        // Console.WriteLine(manager.Execute(query).Result);

        //запрос
        string createQuery = "create table test1 (name C (8), age N (2,2), isDead L, birth D)";
        Console.WriteLine(createQuery);
        //проверяем валидацию
        Console.WriteLine(validationFactory.Validate("create", createQuery) + "\nСвойства:\n");

        // //разбираем на поля
        string[] properties = converter.GetStringProperties(createQuery);
        foreach (var property in properties)
            Console.WriteLine(property);

        Console.WriteLine("Имя таблицы " + validationFactory.GetTableName("create", createQuery));

        Console.WriteLine(manager.Execute(createQuery).Result);

        string insertQuery1 = "insert into test1 (name, age, isDead, birth) valUes(\"john\", 24.5, T, 10050606)";
        string insertQuery2 = "insert into test1 (age, isDead) valUes(-7, f)";
        string insertQuery3 = "insert into test1 (name) valUes(\"american\")";
        
        Console.WriteLine(manager.Execute(insertQuery1).Result);
        Console.WriteLine(manager.Execute(insertQuery2).Result);
        Console.WriteLine(manager.Execute(insertQuery3).Result);
        manager.Dispose();
    }
}