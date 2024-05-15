namespace Sqlol.Expressions.Builders;

public interface IExpressionValidator
{
    public bool Validate(IExpression expression);
}