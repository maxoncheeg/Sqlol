using Sqlol.Configurations.TypesConfigurations;

namespace Sqlol.Configurations;

public class KeyWordsConfiguration : IKeyWordsConfiguration
{
    private const string DoubleQuote = "\"";
    public string ValuePattern => $@"([\w\d\.\+\-\?]+|{DoubleQuote}[^{DoubleQuote}]*{DoubleQuote})";
    
    public IDictionary<char, ITypeConfiguration> Types { get; set; } = new Dictionary<char, ITypeConfiguration>()
    {
        { 'C', new CTypeConfiguration(255, 0, 0) },
        { 'D', new DTypeConfiguration(8, 0, 8) },
        { 'N', new NTypeConfiguration(127, 126, 2) },
        { 'L', new LTypeConfiguration(1, 0, 1) },
        { 'M', new CTypeConfiguration(255, 0, 0) },
    };

    public IList<string> LogicalOperations { get; set; } = ["or", "and", "xor"];
    public IList<string> NumberOperations { get; set; } = ["=", "<>", ">=", "<=", ">", "<"];
    public IList<string> StringOperations { get; set; } = ["=", "<>"];

    public IList<string> PrimaryKeyWords { get; set; } =
        ["select", "where", "delete", "update", "set", "from", "*"]; //...
}