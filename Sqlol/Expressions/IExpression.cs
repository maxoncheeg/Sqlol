namespace Sqlol.Expressions;

public interface IExpression : ILogicalful
{
    public IReadOnlyList<ILogicalful> Entities { get; }

    public void Add(ILogicalful entity);
    public void Add(IEnumerable<ILogicalful> entities);
}