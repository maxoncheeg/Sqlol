using Sqlol.Configurations;

namespace Sqlol.Expressions.Builders;

public class ExpressionBuilder : IExpressionBuilder
{
    private IKeyWordsConfiguration _configuration;
    
    public ExpressionBuilder(IKeyWordsConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    // todo: sergio speedster work
    public IExpression TranslateToExpression(string condition)
    {
        //_configuration.LogicalOperations;
        //_configuration.NumberOperations;

        return new Expression();
    }
}