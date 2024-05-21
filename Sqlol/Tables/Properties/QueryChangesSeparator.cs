using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Sqlol.Configurations;

namespace Sqlol.Tables.Properties;

public class QueryChangesSeparator(IKeyWordsConfiguration configuration) : IQueryChangesSeparator
{
    public IList<Tuple<string, string>> GetChangesFromQuery(string command, string query)
    {
        List<Tuple<string, string>> changes = [];
        switch (command.ToLowerInvariant())
        {
            case "update":
            {
                query = Regex.Match(query, @"set\s+.+\s+(where)?", RegexOptions.IgnoreCase).Value;
                var variables = Regex.Matches(query, @"\w{1,11}\s*=", RegexOptions.IgnoreCase).Select(m => m.Value.TrimEnd('=').Trim()).ToList();
                var values = Regex.Matches(query, $@"=\s*{configuration.ValuePattern}", RegexOptions.IgnoreCase).Select(m => m.Value.TrimStart('=').Trim()).ToList();
                
                if (variables.Count != values.Count)
                    throw new Exception("Количество столбцов не соотвествует количеству значений");
                
                for (int i = 0; i < variables.Count; i++)
                    changes.Add(new Tuple<string, string>(variables[i], values[i]));
            }
                break;
            case "insert":
            {
                var brackets = Regex.Split(query, "values", RegexOptions.IgnoreCase).ToList();
                brackets = brackets.Select(s => Regex.Match(s, @"\(.+\)").Value).ToList();
                var firstBracket = brackets.First();
                var secondBracketText = brackets.Last().Trim('(').Trim(')');
                
                List<string> values = [];
                
                var m = Regex.Matches(secondBracketText, $@"{configuration.ValuePattern}\,\s*");
                foreach (Match t in m)
                    values.Add(t.Value.TrimEnd(' ').TrimEnd(','));
                values.Add(Regex.Match(secondBracketText, $@"(\,|^\s*)\s*{configuration.ValuePattern}$").Value
                    .TrimStart(',').TrimStart(' '));

                var variables = Regex.Matches(firstBracket, @"\w{1,11}").ToList();

                if (variables.Count != values.Count)
                    throw new Exception("Количество столбцов не соотвествует количеству значений");

                for (int i = 0; i < variables.Count; i++)
                    changes.Add(new Tuple<string, string>(variables[i].Value, values[i]));
            }
                break;
        }

        return changes;
    }
}