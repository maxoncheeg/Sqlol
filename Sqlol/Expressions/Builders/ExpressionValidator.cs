using Sqlol.Configurations;
using Sqlol.Loggers;
using Sqlol.Tables.Properties;

namespace Sqlol.Expressions.Builders;

public class ExpressionValidator(IKeyWordsConfiguration configuration, ILogger logger) : IExpressionValidator
{
    public bool Validate(IExpression expression, IList<ITableProperty> properties)
    {
        // todo: спизженный метод Print из Programm.cs Sqlol.Console ИСПРАВЬ! ЧИСТЫЙ КОД ОЖИДАЮ 
        foreach (var x in expression.Entities)
        {
            if (x is IFilter f)
            {
                //Console.WriteLine($"{new string('\t', c)}{f.Field} {f.Operation} {f.Value} {f.Next}");
                var property = properties.FirstOrDefault(property =>
                    property.Name.Equals(f.Field, StringComparison.InvariantCultureIgnoreCase));

                if (property == null)
                {
                    ThrowErrorMessage(null);
                    return false;
                }
                //else if()
                
                // todo: фрагмент из класса InsertQuery, вот так ваолидировать будешь
                // if (!_configuration.Types[property.Type].Validate(tuple.Item2, property.Width, property.Precision))
                // {
                //     _logger.SendMessage("Ошибка", $"Значение поля {tuple.Item1} не соотвествует типу {property.Type}");
                //     return new QueryResult(0, table);
                // }
                // todo: если валидируется плохо, то вызывай ThrowErrorMessage
            }
            else if (x is IExpression a)
            {
                return Validate(a, properties);
            }
        }

        return true;
    }

    private void ThrowErrorMessage(ITableProperty? property)
    {
        if(property == null)
            logger.SendMessage("Ошибка", "Нет такого поля");
        
        // todo: чтоб все по красоте было ПО ТИПУ ПОЛЯ выводи соотвественное смс, ну может по типу будет излишне, просто какую то инфу выводить, можно даже изменить аргументы. СДЕЛАЙ ЧТО НИБУДЬ
        
        // todo: когда допишешь этот класс и будешь в себе уверен, именно тогда изза того что мне лень писать тесты тебе надо будет встроить этот класс в Select- Delete- Update- Query классы,
        // todo: чтоб работали они - в Programm.cs тебе надобно передать будет через Dependency Injection этот интерфейс в них, да прибудет с тобой Пайсон
    }
}