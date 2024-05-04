namespace Sqlol.Expressions;

public class Filter(string field, string value, string operation, string next)
    : IFilter
{
    public string Field { get; } = field;
    public string Value { get; } = value;
    public  string Operation { get; } = operation;
    public string Next { get; } = next;

    public Filter(string field, string value, string operation) : this(field, value, operation, string.Empty) 
    { }
}