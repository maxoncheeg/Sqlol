namespace Sqlol.Configurations;

public class KeyWordsConfiguration : IKeyWordsConfiguration
{
    public IList<string> LogicalOperations { get; set; } = ["or", "and", "xor"];
    public IList<string> NumberOperations { get; set; } = ["=", "<>", ">", "<", ">=", "<="];
    public IList<string> StringOperations { get; set; } = ["=", "<>"];
    public IList<string> PrimaryKeyWords { get; set; } = ["select", "where", "delete", "update", "set", "from", "*"]; //...
}