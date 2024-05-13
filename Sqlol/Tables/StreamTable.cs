using System.Text;
using Sqlol.Expressions;
using Sqlol.Tables.Properties;

namespace Sqlol.Tables;

public class StreamTable : ITable
{
    private readonly Stream _tableStream;

    private List<ITableProperty> _properties;

    public string Name { get; }
    public bool HasMemoFile { get; }
    public DateTime LastUpdateDate { get; private set; }
    public int RecordsAmount { get; private set; }
    public short HeaderLength { get; }
    public short RecordLength { get; }
    public IReadOnlyList<ITableProperty> Properties => _properties;

    public StreamTable(string name, Stream tableStream, IList<ITableProperty> properties)
    {
        Name = name;
        LastUpdateDate = DateTime.Now;

        HeaderLength = (short)(32 + 16 * properties.Count);
        RecordLength = (short)(properties.Sum(p => (short)p.Size) + 1);
        _properties = properties.ToList();

        // ??
        RecordsAmount = 0;
        HasMemoFile = File.Exists($"{name}.dbt");

        _tableStream = tableStream;

        CreateTable();

        //tableStream.Dispose();
    }
    
    public StreamTable(string name, Stream tableStream)
    {
        Name = name;
        _tableStream = tableStream;
        
        _tableStream.Seek(0, SeekOrigin.Begin);
        byte[] buffer = new byte[32];
        
        _tableStream.Read(buffer, 0, 1);
        HasMemoFile = buffer[0] == 131;

        _tableStream.Read(buffer, 0, 3);
        LastUpdateDate = new(2000 + buffer[0], buffer[1], buffer[2]);

        _tableStream.Read(buffer, 0, sizeof(int));
        RecordsAmount = BitConverter.ToInt32(buffer, 0);
        
        _tableStream.Read(buffer, 0, sizeof(short));
        HeaderLength = BitConverter.ToInt16(buffer, 0);
        
        _tableStream.Read(buffer, 0, sizeof(short));
        RecordLength = BitConverter.ToInt16(buffer, 0);

        _tableStream.Seek(20, SeekOrigin.Current);
        int propertiesLength = HeaderLength - 32;

        _properties = [];
        while (propertiesLength > 0)
        {
            ITableProperty property;

            _tableStream.Read(buffer, 0, 11);
            var propertyName = Encoding.ASCII.GetString(buffer, 0, 11);
            propertyName = propertyName.Trim('\0').Trim();

            _tableStream.Read(buffer, 0, 5);
            

            property = new TableProperty(propertyName, (char)buffer[0], buffer[1], buffer[2], buffer[4], buffer[3]);
            _properties.Add(property);
            propertiesLength -= 16;
        }
    }

    #region commands

    public bool Insert(IList<Tuple<string, string>> data)
    {
        try
        {
            _tableStream.Seek(HeaderLength + RecordLength * RecordsAmount, SeekOrigin.Begin);

            List<byte> buffer = new();
            // Пометка удаления.
            //_tableStream.Write([20]);
            buffer.Add(32);

            foreach (var property in Properties)
            {
                var tuple = data.FirstOrDefault(t => t.Item1.ToLowerInvariant() == property.Name.ToLowerInvariant());

                if (tuple != null)
                {
                    string value = tuple.Item2;
                    value = value.Trim('"');
                    if (value.Length > property.Size) throw new ArgumentException("Поле больше позволяемой длины");

                    switch (property.Type)
                    {
                        case 'C':
                            if (value.Length < property.Size)
                                value += new string('\0', property.Size - value.Length);
                            break;
                        case 'N':
                            if (value.Length < property.Size)
                            {
                                char sign = '+';
                                if (value[0] == '-' || value[0] == '+')
                                {
                                    sign = value[0];
                                    value = value[^1..];
                                }

                                string left, right = "";
                                if (value.Contains('.'))
                                {
                                    left = value[..(value.IndexOf('.'))];
                                    right = value[(value.IndexOf('.') + 1)..];
                                }
                                else left = value;

                                if (left.Length < property.Width)
                                    left = new string('0', property.Width - left.Length) + left;
                                if (right.Length < property.Precision)
                                    right = right + new string('0', property.Width - right.Length);

                                value = sign + left + '.' + right;
                            }

                            break;
                    }

                    //_tableStream.Write(Encoding.ASCII.GetBytes(value));
                    buffer.AddRange(Encoding.ASCII.GetBytes(value));
                }
                else
                {
                    // todo: значения по умолчанию для каждого типа
                    //_tableStream.Write(Encoding.ASCII.GetBytes(new string('\0', property.Width)));
                    buffer.AddRange(Encoding.ASCII.GetBytes(new string('\0', property.Size)));
                }
            }

            _tableStream.Write(buffer.ToArray());
        }
        catch
        {
            throw;
            return false;
        }

        RecordsAmount++;
        LastUpdateDate = DateTime.Now;
        UpdateHeader();
        
        return true;
    }

    public ITableData Select(IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public ITableData Select(IList<string> columns, IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public int Update(IList<Tuple<string, string>> changes, IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public int Truncate()
    {
        throw new NotImplementedException();
    }

    public int Delete(IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public int Restore(IExpression? expression = null)
    {
        throw new NotImplementedException();
    }

    public bool AddColumn(ITableProperty property)
    {
        throw new NotImplementedException();
    }

    public bool RemoveColumn(string columnName)
    {
        throw new NotImplementedException();
    }

    public bool RenameColumn(string currentName, string newName)
    {
        throw new NotImplementedException();
    }

    public bool UpdateColumn(string columnName, ITableProperty property)
    {
        throw new NotImplementedException();
    }

    #endregion

    public void Dispose()
    {
        _tableStream.Close();
        _tableStream.Dispose();
    }

    private void CreateTable()
    {
        _tableStream.Seek(0, SeekOrigin.Begin);

        _tableStream.Write(HasMemoFile ? [131] : [3]);

        byte year = (byte)(LastUpdateDate.Year % 100);
        byte month = (byte)(LastUpdateDate.Month % 100);
        byte day = (byte)(LastUpdateDate.Day % 100);
        byte[] dateBuffer = [year, month, day];

        _tableStream.Write(dateBuffer);

        byte[] recordsAmount = BitConverter.GetBytes(RecordsAmount);
        _tableStream.Write(recordsAmount);

        byte[] lengthInBytes = BitConverter.GetBytes(HeaderLength);
        _tableStream.Write(lengthInBytes);

        byte[] recordLength = BitConverter.GetBytes(RecordLength);
        _tableStream.Write(recordLength);

        for (int i = 0; i < 20; i++)
            _tableStream.Write([0]);

        foreach (var property in Properties)
        {
            string name = property.Name;
            while (name.Length < 11) name += '\0';

            _tableStream.Write(Encoding.ASCII.GetBytes(name));
            _tableStream.Write([(byte)property.Type]);
            _tableStream.Write([(byte)property.Width]);
            _tableStream.Write([(byte)property.Precision]);
            _tableStream.Write([(byte)property.Size]);
            _tableStream.Write([(byte)property.Index]);
        }
    }

    private void UpdateHeader(bool withProperties = false)
    {
        _tableStream.Seek(0, SeekOrigin.Begin);

        _tableStream.Write(HasMemoFile ? [131] : [3]);

        byte year = (byte)(LastUpdateDate.Year % 100);
        byte month = (byte)(LastUpdateDate.Month % 100);
        byte day = (byte)(LastUpdateDate.Day % 100);
        byte[] dateBuffer = [year, month, day];

        _tableStream.Write(dateBuffer);

        byte[] recordsAmount = BitConverter.GetBytes(RecordsAmount);
        _tableStream.Write(recordsAmount);

        byte[] lengthInBytes = BitConverter.GetBytes(HeaderLength);
        _tableStream.Write(lengthInBytes);

        byte[] recordLength = BitConverter.GetBytes(RecordLength);
        _tableStream.Write(recordLength);

        for (int i = 0; i < 20; i++)
            _tableStream.Write([0]);
        
        if (withProperties)
            foreach (var property in Properties)
            {
                string name = property.Name;
                while (name.Length < 11) name += '\0';

                _tableStream.Write(Encoding.ASCII.GetBytes(name));
                _tableStream.Write([(byte)property.Type]);
                _tableStream.Write([(byte)property.Size]);
                _tableStream.Write([(byte)property.Index]);
            }
    }
}