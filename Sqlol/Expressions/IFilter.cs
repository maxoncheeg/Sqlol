namespace Sqlol.Expressions;

public interface IFilter : ILogicalful
{
    public string Field { get; }
    public string Value { get; }
    public string Operation { get; }
}