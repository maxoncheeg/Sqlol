﻿namespace Sqlol.Tables.Properties;

public interface ITablePropertyConverter
{
    public string[] GetStringProperties(string query);
    public IList<ITableProperty> Convert(string properties);
    public IList<ITableProperty> Convert(string[] properties);
}