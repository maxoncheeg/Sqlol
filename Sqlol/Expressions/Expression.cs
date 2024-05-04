namespace Sqlol.Expressions;

public class Expression : IExpression
{
    private readonly List<ILogicalful> _entities;

    public string Next { get; }
    public IReadOnlyList<ILogicalful> Entities => _entities;

    public Expression(IEnumerable<ILogicalful> entities, string next)
    {
        Next = next;
        _entities = [..entities];
    }

    public Expression(IEnumerable<ILogicalful> entities)
    {
        Next = string.Empty;
        _entities = [..entities];
    }

    public Expression(string next)
    {
        Next = next;
        _entities = [];
    }

    public Expression()
    {
        Next = string.Empty;
        _entities = [];
    }

    public void Add(ILogicalful entity)
    {
        _entities.Add(entity);
    }

    public void Add(IEnumerable<ILogicalful> entities)
    {
        _entities.AddRange(entities);
    }
};