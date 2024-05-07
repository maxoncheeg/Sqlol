namespace Sqlol.Configurations;

public class KeyWordsConfiguration : IKeyWordsConfiguration
{
    public IList<string> Types { get; set; } = ["C", "D", "N", "L", "M"];
    public IDictionary<char, Tuple<byte, byte, byte>> Types { get; set; } = new Dictionary<char, Tuple<byte, byte, byte>>()
    {
        {'C', new(255,0,0)},
        {'D', new(8,0,0)},
        {'N', new(8,8,1)}, // или 2 если оставлять для +-
        {'L', new(1,0,0)},
        {'M', new(0,0,1)},
    };
    public IList<string> LogicalOperations { get; set; } = ["or", "and", "xor"];
    public IList<string> NumberOperations { get; set; } = ["=", "<>", ">", "<", ">=", "<="];
    public IList<string> StringOperations { get; set; } = ["=", "<>"];
    public IList<string> PrimaryKeyWords { get; set; } = ["select", "where", "delete", "update", "set", "from", "*"]; //...
}