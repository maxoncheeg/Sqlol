using System.Text.RegularExpressions;
using Sqlol.Configurations;

namespace Sqlol.Expressions.Builders;

public partial class ExpressionBuilder : IExpressionBuilder {
    private const string DoubleQuote = "\"";
    private const string ValuePattern = $@"([\w\d\.\+\-]+|{DoubleQuote}.+{DoubleQuote})";
    private const string FieldPattern = @"[\w\d_]+";
    private readonly string _comparisonGroup;
    private readonly string _logicalGroup;
    
    [GeneratedRegex(@"\|$")]
    private static partial Regex GroupRegex();
    
    [GeneratedRegex("\".+\"")]
    private static partial Regex StringValueRegex();
    
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
    
    [GeneratedRegex(@"(\(|\))")]
    private static partial Regex BracketsRegex();
    
    [GeneratedRegex(@"^\([\w\d_]+")]
    private static partial Regex FieldRegex();
    
    [GeneratedRegex(@$"([\w\d\.]+|{DoubleQuote}.+{DoubleQuote})\)")]
    private static partial Regex ValueRegex();
    
    [GeneratedRegex(@"\)$")]
    private static partial Regex EolBracketRegex();
    
    [GeneratedRegex(@"^\(")]
    private static partial Regex SolBracketRegex();
    
    [GeneratedRegex(@"\s\)")]
    private static partial Regex CloseBracketWhitespaceRegex();
    
    [GeneratedRegex(@"\(\s")]
    private static partial Regex OpenBracketWhitespaceRegex();
    
    [GeneratedRegex(@"^\s")]
    private static partial Regex SolWhitespaceRegex();
    
    [GeneratedRegex(@"\s$")]
    private static partial Regex EolWhitespaceRegex();
    
    public ExpressionBuilder(IKeyWordsConfiguration configuration)
    {
        var operations = new HashSet<string>(configuration.NumberOperations.Concat(configuration.StringOperations));
        _comparisonGroup = GroupRegex().Replace(operations.Aggregate("(", (g, o) => g + o + "|"), ")");
        
        _logicalGroup =  GroupRegex().Replace(configuration.LogicalOperations.Aggregate("(", (g, o) => g + o + "|"), ")");
    }

    private static readonly Dictionary<char, char> Pairs = new() {
        { '(', ')' },
        { '[', ']' },
        { '{', '}' },
    };

    private static bool IsPaired(string input)
    {
        if (input.Length == 0) {
            return true;
        }

        var brackets = new Stack<char>();

        foreach (var i in input)
        {
            if(Pairs.ContainsKey(i)) {
                brackets.Push(i);
            }
            
            else if(Pairs.ContainsValue(i)) {
                if(brackets.Count == 0) 
                    return false;

                var openingBracket = brackets.Pop();
                if(Pairs[openingBracket] != i) {
                    return false;
                }
            }
        }
        
        return brackets.Count == 0;
    }

    private static string RemoveWhitespaces(string condition) {
        var values = StringValueRegex().Matches(condition);
        var c = 0;
        condition = WhitespaceRegex().Replace(condition, " ");
        return StringValueRegex().Replace(condition, _ => values[c++].Value);
    }

    private bool CheckConditions(string condition) {
        var c = BracketsRegex().Replace(condition, " ").ToLower();
        c = WhitespaceRegex().Replace(c, " ");
        return Regex.Match(c.Trim(), $@"^({FieldPattern}\s?{_comparisonGroup}\s?{ValuePattern}\s{_logicalGroup}\s)*{FieldPattern}\s?{_comparisonGroup}\s?{ValuePattern}$", RegexOptions.IgnoreCase).Success;
    }
    
    private IExpression ParseCondition(string condition) {
        var regex = new Regex(@$"\(([^()]+|(?<Level>\()|(?<-Level>\)))+(?(Level)(?!))\)\s?{_logicalGroup}?", RegexOptions.IgnorePatternWhitespace & RegexOptions.IgnoreCase);
        var conditions = regex.Matches(condition);
        var expressionNext = Regex.Match(condition, $"{_logicalGroup}$", RegexOptions.IgnoreCase).Value.ToLower(); 
        var res = new Expression(expressionNext);

        for (var i = 0; i < conditions.Count; i++) {
            var c = conditions[i].Value;
            if (i != conditions.Count - 1 && !Regex.Match(c, $"{_logicalGroup}$", RegexOptions.IgnoreCase).Success) {
                throw new ArgumentException("Некорректный формат");
            }
            
            if (Regex.Match(c, $@"^\({FieldPattern}\s?{_comparisonGroup}\s?{ValuePattern}\)\s?{_logicalGroup}?$").Success) {
                var next = Regex.Match(c, $"{_logicalGroup}$", RegexOptions.IgnoreCase).Value.ToLower();
                var field = FieldRegex().Match(c).Value.Replace("(", "");
                var value = ValueRegex().Match(c).Value.Replace(")", "");
                var operation = Regex.Match(c.Replace("(" + field, "").Replace(value + ")", ""), _comparisonGroup).Value;
                
                res.Add(new Filter(field, value, operation, next));
            }
            else {
                res.Add(ParseCondition(EolBracketRegex().Replace(SolBracketRegex().Replace(c, ""), "")));
            }
        }
        
        return res;
    }
    
    public IExpression TranslateToExpression(string condition) {
        if (!IsPaired(condition))
            throw new ArgumentException("Некорректные скобки");
        
        var x = condition.Trim();

        x = RemoveWhitespaces(x);
        
        if (!CheckConditions(condition))
            throw new ArgumentException("Некорректный формат");
        
        x = Regex.Replace(x, @$"{_logicalGroup}\(", m => m.Value.Replace("(", " ("), RegexOptions.IgnoreCase);
        x = Regex.Replace(x, @$"\){_logicalGroup}", m => m.Value.Replace(")", ") "), RegexOptions.IgnoreCase);
        x = CloseBracketWhitespaceRegex().Replace(x, ")");
        x = OpenBracketWhitespaceRegex().Replace(x, "(");
        
        x = Regex.Replace(x, @$"^{FieldPattern}\s?{_comparisonGroup}\s?{ValuePattern}", m => $"({m})");
        x = Regex.Replace(x, @$"\s{FieldPattern}\s?{_comparisonGroup}\s?{ValuePattern}", m => $" ({SolWhitespaceRegex().Replace(m.ToString(), "")})");
        x = Regex.Replace(x, @$"{FieldPattern}\s?{_comparisonGroup}\s?{ValuePattern}\s", m => $"({EolWhitespaceRegex().Replace(m.ToString(), "")}) ");
        
        return ParseCondition(x);
    }
}