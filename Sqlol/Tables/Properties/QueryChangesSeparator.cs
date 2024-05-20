using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Sqlol.Configurations;

namespace Sqlol.Tables.Properties;

public class QueryChangesSeparator(IKeyWordsConfiguration configuration) : IQueryChangesSeparator
{
    public IList<Tuple<string, string>> GetChangesFromQuery(string command, string query)
    {
        List<Tuple<string, string>> changes = [];
        switch (command)
        {
            case "update":
            {
                query = Regex.Match(query, @"set\s+.+\s+where").Value;
                var variables = Regex.Matches(query, @"\w{1,11}\s*=").Select(m => m.Value.TrimEnd('=').Trim()).ToList();
                var values = Regex.Matches(query, $@"=\s*{configuration.ValuePattern}").Select(m => m.Value.TrimStart('=').Trim()).ToList();
                
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

                // List<string> secondBracket = GetStringValues(secondBracketText);
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

    private List<string> GetStringValues(string bracketText)
    {
        bool isC = false;
        List<string> secondBracket = [];
        List<int> commaIndexes = new List<int>();

        int firstIndex = 0, cEndIndex = -1;
        for (int i = 0; i < bracketText.Length; i++)
        {
            switch (bracketText[i])
            {
                case '"' when cEndIndex < i:
                    isC = true;
                    int comma = bracketText.IndexOf(',', i);
                    cEndIndex = bracketText.LastIndexOf('"', comma >= 0 ? comma : bracketText.Length - 1);
                    break;
                case '"' when cEndIndex == i:
                    isC = false;
                    break;
                case ',' when !isC:
                    secondBracket.Add(bracketText.Substring(firstIndex, i - firstIndex));
                    firstIndex = i + 1;
                    i++;
                    break;
            }
        }

        if (secondBracket.Count == 0) secondBracket.Add(bracketText);

        return secondBracket;
    }
}