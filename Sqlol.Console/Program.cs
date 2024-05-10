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
using Sqlol.Tables.Memory;
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
    public static void Main(string[] args)
    {

        // IQueryManager manager = new QueryManager(reader, validationFactory);
        // Console.WriteLine(manager.CreateTable("create sqlol_primary_table"));


        IKeyWordsConfiguration configuration = new KeyWordsConfiguration();
        IValidationFactory validationFactory = new ValidationFactory();
        ILogger logger = new SimpleLogger((t, m) => { Console.WriteLine($"{t}: {m}"); });

        ITablePropertyConverter converter = new TablePropertyConverter(configuration);
        IQueryFactory queryFactory = new QueryFactory(converter, validationFactory, logger);

        IQueryManager manager = new QueryManager(validationFactory, queryFactory);

        //запрос
        string query = "create table russia (x C (20), y N (3,5), z L)";
        Console.WriteLine(query);
        //проверяем валидацию
        Console.WriteLine(validationFactory.Validate("create", query) + "\nСвойства:\n");

        // //разбираем на поля
        string[] properties = converter.GetStringProperties(query);
        foreach (var property in properties)
            Console.WriteLine(property);

        Console.WriteLine("Имя таблицы " + validationFactory.GetTableName("create", query));

        Console.WriteLine(manager.Execute(query).Result);

        // ITypeConfiguration configuration = new NTypeConfiguration(127, 126, 2);
        //
        // Console.WriteLine(configuration.Validate("1235,023", 4, 3));
        // Console.WriteLine(configuration.Validate("1235,023", 3, 3));
        // Console.WriteLine(configuration.Validate("+1235,023", 3, 3));
        // Console.WriteLine(configuration.Validate("-1235,023", 3, 3));
        // Console.WriteLine(configuration.Validate("1235", 10, 3));
        // Console.WriteLine(configuration.Validate("-1235", 10, 3));
        // Console.WriteLine(configuration.Validate("+1235", 10, 3));
        // Console.WriteLine(configuration.Validate("0", 10, 0));
        // Console.WriteLine(configuration.Validate("0,2", 10, 2));
        // Console.WriteLine(configuration.Validate("-0,2", 10, 2));


        //конвертируем в классы
        //List<ITableProperty> result = converter.Convert(properties).ToList();
        //List<ITableProperty> result = converter.Convert(query).ToList();
    }
}