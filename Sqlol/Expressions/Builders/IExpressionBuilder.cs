namespace Sqlol.Expressions.Builders;

public interface IExpressionBuilder
{
    public IExpression TranslateToExpression(string condition);
}