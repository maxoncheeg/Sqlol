namespace Sqlol.Configurations.Validators;

public interface IValidator
{
    /// <summary>
    /// Провести валидацию строки
    /// </summary>
    /// <param name="text">Строка</param>
    /// <returns>-1 - если валидация прошла успешно, ИНАЧЕ индекс места ошибки в строке</returns>
    public int Validate(string text);
}