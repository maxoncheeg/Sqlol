using System.Text;
using Sqlol.Configurations;
using Sqlol.Configurations.Factories;
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

internal class Program
{
    public static void Print(IExpression e, int c = 0)
    {
        foreach (var x in e.Entities)
        {
            if (x is IFilter f)
            {
                Console.WriteLine($"{new string('\t', c)}{f.Field} {f.Operation} {f.Value} {f.Next}");
            }
            else if (x is IExpression a)
            {
                Print(a, c + 1);
                Console.WriteLine(x.Next);
            }
        }
    }

    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        IKeyWordsConfiguration configuration = new KeyWordsConfiguration();
        IValidationFactory validation = new ValidationFactory(configuration);
        IQueryChangesSeparator separator = new QueryChangesSeparator(configuration);
        IOperationFactory operationFactory = new OperationFactory();
        IExpressionBuilder builder = new ExpressionBuilder(configuration);
        ITablePropertyConverter converter = new TablePropertyConverter(configuration);
        ILogger logger = new SimpleLogger((t, m) =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(t + ": " + m);
            Console.ForegroundColor = ConsoleColor.White;
        });

        IQueryFactory queryFactory = new QueryFactory(new()
        {
            { "create", new CreateQuery(converter, logger, validation, operationFactory) },
            { "open", new OpenQuery(validation, logger, operationFactory) },
            { "close", new CloseQuery(logger) },
            { "insert", new InsertQuery(configuration, separator, logger) },
            { "select", new SelectQuery(builder, logger) },
            { "delete", new DeleteQuery(builder, logger) },
            { "restore", new RestoreQuery(logger) },
            { "truncate", new TruncateQuery(logger) },
            { "columnRename", new ColumnRenameQuery(logger) },
            { "columnAdd", new AddColumnQuery(converter, logger) },
            { "columnRemove", new RemoveColumnQuery(logger) },
            { "columnUpdate", new UpdateColumnQuery(converter, logger) },
            { "update", new UpdateQuery(configuration, separator, builder, logger) }
        });

        IQueryManager manager = new QueryManager(validation, queryFactory, logger);

        string? query = "";
        while (query.ToLowerInvariant() != "exit")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("SQL> ");
            Console.ForegroundColor = ConsoleColor.White;

            var queries = ReadQueries();

            foreach (var q in queries)
            {
                if (q.ToLowerInvariant() == "exit") break;
                
                if(queries.Count > 1)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Запрос: " + q);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                var result = manager.Execute(q);
                

                if (result.Data != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(result.Data.GetHeader());
                    Console.ForegroundColor = ConsoleColor.White;
                    using IEnumerator<string> enumerator = result.Data.GetRecords();

                    while (enumerator.MoveNext())
                        Console.WriteLine(enumerator.Current);
                }

                if (result.Result > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Команда выполнена успешно.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Команда не выполнена.");
                }
                Console.ForegroundColor = ConsoleColor.White;
                query = q;
            }
        }


        manager.Dispose();
    }

    private static List<string> ReadQueries()
    {
        string? line = "";
        List<string> queries = [];
        string input = "";
        int lineIndex = -1;
        do
        {
            if (line.Contains(';')) lineIndex = 0;
            else lineIndex++;

            if(line != "") Console.Write(new string(' ', 5));
            Console.Write(new string(' ', lineIndex));
            input += line + ' ';
            line = Console.ReadLine();
        } while (!string.IsNullOrWhiteSpace(line));

        var lines = input.Split(';');
        foreach (var l in lines)
        {
            if (string.IsNullOrWhiteSpace(l)) continue;
            queries.Add(l.Trim());
        }

        return queries;
    }
}