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

        configuration.StringOperations.Add("like");
        configuration.StringOperations.Add("not like");
        IExpressionBuilder builder = new ExpressionBuilder(configuration);
        //
        // string x = "x like \"%amerika\" or y not like \"%russia%\"";
        // Print(builder.TranslateToExpression(x));

        ITablePropertyConverter converter = new TablePropertyConverter(configuration);
        ILogger logger = new SimpleLogger((t, m) => Console.WriteLine(t + ": " + m));

        IQueryFactory queryFactory = new QueryFactory(new()
        {
            { "create", new CreateQuery(converter, logger, validation, operationFactory) },
            { "open", new OpenQuery(validation, logger, operationFactory) },
            { "close", new CloseQuery() },
            { "insert", new InsertQuery(configuration, separator, logger) },
            { "select", new SelectQuery(builder) },
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
        while (query != "exit")
        {
            Console.Write("SQL> ");
            query = Console.ReadLine();
            if (query == null) continue;

            query = query.Trim().Trim(';').Trim();

            if (query.ToLowerInvariant() == "exit") break;

            var result = manager.Execute(query);

            if (result.Data != null)
                Console.WriteLine(result.Data.GetStringTable());

            if (result.Result > 0)
                Console.WriteLine("Команда выполнена успешно.");
            else
                Console.WriteLine("Команда не выполнена.");
        }


        manager.Dispose();
    }
}