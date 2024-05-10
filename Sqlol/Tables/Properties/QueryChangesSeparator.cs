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
                    var value = match.Value.Substring(match.Value.IndexOf('=') + 1).Trim().Trim('"');

                    changes.Add(new Tuple<string, string>(variable, value));
                }
            }
                break;
            case "insert":
            {
                var brackets = Regex.Matches(query, @"\(.*\)").Select(m => m.Value).ToList();
                var firstBracket = brackets.First();
                var secondBracket = brackets.Last();

                var variables = Regex.Matches(firstBracket, @"\w{1,11}");
                var values = Regex.Matches(firstBracket, @"(\w+|\d+)");
                
                
            }
                break;
        }

        return changes;
    }
}