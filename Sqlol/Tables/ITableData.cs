namespace Sqlol.Tables;

public interface ITableData
{
    public IReadOnlyList<string> Columns { get; }
    public IReadOnlyList<IReadOnlyList<string>> Values { get; }

    /// <summary>
    /// Добавляет новую запись в Values.
    /// </summary>
    /// <param name="record">Запись. Кол-во элементов должно соотвествовать Columns</param>
    /// <returns>Результат добавления</returns>
    public bool AddRecord(IList<string> record);

    /// <summary>
    /// Получение заголовка данных
    /// </summary>
    /// <returns>Columns в виде строки</returns>
    public string GetHeader();
    
    /// <summary>
    /// Получение записей данных
    /// </summary>
    /// <returns>Строки с записями</returns>
    public IEnumerator<string> GetRecords();
    
    /// <summary>
    /// Возвращает всю таблицу записанную в одну переменную
    /// </summary>
    /// <returns>Таблица в строке</returns>
    public string GetStringTable();
}