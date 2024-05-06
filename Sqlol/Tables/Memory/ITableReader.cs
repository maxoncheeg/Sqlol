namespace Sqlol.Tables.Memory;

/// <summary>
/// Определяет объекты для нахождения или создания файла таблицы в памяти компьютера и последующего открытия
/// </summary>
public interface ITableReader
{
    /// <summary>
    /// Создание файла таблицы
    /// </summary>
    /// <param name="properties">Столбцы таблицы</param>
    /// <returns>Объект для взаимодействия с таблицей</returns>
    public ITable? CreateTable(IList<ITableProperty> properties);
    
    /// <summary>
    /// Прочесть заголовок таблицы в объект ITable для последующих взаимодействий с таблицей
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <returns>Объект для взаимодействия с таблицей</returns>
    public ITable? ReadTable(string fileName);
}