using Sqlol.Configurations.TypesConfigurations;

namespace Sqlol.Configurations;

public interface IKeyWordsConfiguration
{
    public string ValuePattern { get; }
    
    public IDictionary<char, ITypeConfiguration> Types { get; set; }
    public IList<string> LogicalOperations { get; set; }
    public IList<string> NumberOperations { get; set; }
    public IList<string> StringOperations { get; set; }
    public IList<string> PrimaryKeyWords { get; set; }
}