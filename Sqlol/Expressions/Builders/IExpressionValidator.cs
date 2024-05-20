using Sqlol.Tables.Properties;

namespace Sqlol.Expressions.Builders;

public interface IExpressionValidator
{
    public bool Validate(IExpression expression, IList<ITableProperty> properties);
}