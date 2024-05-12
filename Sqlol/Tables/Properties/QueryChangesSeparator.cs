using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Sqlol.Tables.Properties;

public class QueryChangesSeparator : IQueryChangesSeparator
{
    public IList<Tuple<string, string>> GetChangesFromQuery(string command, string query)
    {
        List<Tuple<string, string>> changes = [];
        switch (command)
        {
            case "update":
            {
                var matches = Regex.Matches(query, @"\w{1,11}\s?=\s?" + "\"?" + @"(\w+|\d+)" + "\"?").ToList();
                foreach (var match in matches)
                {
                    var variable = match.Value.Substring(0, match.Value.IndexOf('=')).Trim();
                    var value = match.Value.Substring(match.Value.IndexOf('=') + 1).Trim();

                    changes.Add(new Tuple<string, string>(variable, value));
                }
            }
                break;
            case "insert":
            {
                var brackets = Regex.Split(query, "values", RegexOptions.IgnoreCase).ToList();
                brackets = brackets.Select(s => Regex.Match(s, @"\(.+\)").Value).ToList();
                var firstBracket = brackets.First();
                var secondBracket = brackets.Last().Trim('(').Trim(')').Split(',');

                var variables = Regex.Matches(firstBracket, @"\w{1,11}").ToList();
                var values = secondBracket.Select(v => v.Trim()).ToList();

                if (variables.Count != values.Count)
                    throw new Exception("Количество столбцов не соотвествует количеству значений");

                for (int i = 0; i < variables.Count; i++)
                    changes.Add(new(variables[i].Value, values[i]));
            }
                break;
        }

        return changes;
    }
}