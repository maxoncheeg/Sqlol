using System.ComponentModel.Design;
using System.Text;
using Sqlol.Configurations.Factories;
using Sqlol.Configurations.Factories.Operations;
using Sqlol.Expressions;
using Sqlol.Tables.Properties;

namespace Sqlol.Tables;

public class StreamTable : ITable
{
    private readonly IOperationFactory _operationFactory;
    private readonly Stream _tableStream;

    private List<ITableProperty> _properties;

    public string Name { get; }
    public bool HasMemoFile { get; }
    public DateTime LastUpdateDate { get; private set; }
    public int RecordsAmount { get; private set; }
    public short HeaderLength { get; }
    public short RecordLength { get; }
    public IReadOnlyList<ITableProperty> Properties => _properties;

    public StreamTable(string name, Stream tableStream, IList<ITableProperty> properties, IOperationFactory factory)
    {
        _operationFactory = factory;
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

    public StreamTable(string name, Stream tableStream, IOperationFactory factory)
    {
        _operationFactory = factory;
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
            var propertyName = Encoding.GetEncoding(1251).GetString(buffer, 0, 11);
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
                    if (value.Length > property.Size) throw new ArgumentException("Поле больше позволяемой длины");
                    
                    if (value.Length != property.Size)
                        switch (property.Type)
                        {
                            case 'C': value = ConvertToC(value, property);
                                break;
                            case 'N': value = ConvertToN(value, property);
                                break;
                        }

                    //_tableStream.Write(Encoding.GetEncoding(1251).GetBytes(value));
                    buffer.AddRange(Encoding.GetEncoding(1251).GetBytes(value));
                }
                else
                {
                    // todo: значения по умолчанию для каждого типа
                    //_tableStream.Write(Encoding.GetEncoding(1251).GetBytes(new string('\0', property.Width)));
                    buffer.AddRange(Encoding.GetEncoding(1251).GetBytes(new string('\0', property.Size)));
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
        return Select(Properties.Select(p => p.Name).ToList(), expression);
    }

    public ITableData Select(IList<string> columns, IExpression? expression = null)
    {
        ITableData data = new TableData([..columns]);
        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);
        string[] result = new string[columns.Count];
        List<string> variables = [];


        byte[] buffer = new byte[RecordLength];
        int count = RecordsAmount;
        while (count > 0)
        {
            _tableStream.Read(buffer, 0, RecordLength);
            int offset = 1;
            variables = [];
            foreach (var property in Properties)
            {
                string value = Encoding.GetEncoding(1251).GetString(buffer, offset, property.Size);
                variables.Add(value);
                
                offset += property.Size;
                
                int index = columns.IndexOf(property.Name);
                if (index >= 0)
                    result[index] = value;
            }

            List<string> record = [..result];

            if (expression != null && CheckExpression(expression, Properties.Select(p => p.Name).ToList(), variables))
                data.AddRecord(record);
            else if (expression == null) data.AddRecord(record);
            count--;
        }

        return data;
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

            _tableStream.Write(Encoding.GetEncoding(1251).GetBytes(name));
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

                _tableStream.Write(Encoding.GetEncoding(1251).GetBytes(name));
                _tableStream.Write([(byte)property.Type]);
                _tableStream.Write([(byte)property.Size]);
                _tableStream.Write([(byte)property.Index]);
            }
    }

    private bool CheckExpression(IExpression expression, List<string> variables, List<string> record)
    {
        bool prevResult = false;
        bool prevOr = false;
        bool orWait = false;
        bool prevXor = false;
        bool xorWait = false;
        string next = string.Empty;
        for (int i = 0; i < expression.Entities.Count; i++)
        {
            if (expression.Entities[i] is Filter f)
            {
                var index = variables.IndexOf(f.Field);
                bool result = false;

                var property = Properties.FirstOrDefault(p => p.Name == f.Field);
                //Console.Write(record[index]);

                if (property == null) return false;
                string expectedValue = f.Value;
                switch (property.Type)
                {
                    case 'C':
                        expectedValue.Trim('\0');
                        expectedValue = '"' + expectedValue + '"';
                        break;
                    case 'N': expectedValue = ConvertToN(expectedValue, property);
                        break;
                }

                result = _operationFactory.GetOperation(f.Operation).GetResult(record[index], expectedValue);
                bool r = result;

                // Console.Write(" - " + f.Field + " " + result);
                if (next != string.Empty)
                {
                    result = next switch
                    {
                        "and" => result && prevResult,
                        "or" => result || prevResult,
                        _ => result
                        //SqlolLogicalOperation.Xor => (result && !prevResult) || (!result && prevResult)
                    };
                }


                // Console.WriteLine(" " + next + " " + result);


                if (f.Next == "or")
                {
                    if (orWait)
                    {
                        result = prevOr || result;
                        orWait = false;
                    }
                    else
                    {
                        prevOr = result;
                    }

                    if (xorWait)
                    {
                        result = (result && !prevXor) || (!result && prevXor);
                        xorWait = false;
                    }
                }
                else if (f.Next == "and" || f.Next == "xor") orWait = true;

                if (f.Next == "xor")
                {
                    prevXor = r;
                    xorWait = true;
                }

                // if (next == SqlolLogicalOperation.Xor) prevXor = result;

                next = f.Next;
                prevResult = result;
            }
            else if (expression.Entities[i] is Expression e)
            {
                bool result = CheckExpression(e, variables, record);
                if (next != String.Empty)
                {
                    result = next switch
                    {
                        "and" => result && prevResult,
                        "or" => result || prevResult,
                        _ => result
                    };
                }

                bool r = result;

                if (e.Next == "or")
                {
                    if (orWait)
                    {
                        result = prevOr || result;
                        orWait = false;
                    }
                    else
                    {
                        prevOr = result;
                    }

                    if (xorWait)
                    {
                        result = (result && !prevXor) || (!result && prevXor);
                        xorWait = false;
                    }
                }
                else if (e.Next == "and" || e.Next == "xor") orWait = true;

                if (e.Next == "xor")
                {
                    prevXor = r;
                    xorWait = true;
                }

                next = e.Next;
                prevResult = result;
            }
        }

        if (xorWait)
        {
            //Console.WriteLine(prevResult + " " + prevXor);
            prevResult = (prevResult && !prevXor) || (!prevResult && prevXor);
            //Console.WriteLine(prevResult + " " + prevXor);
        }

        if (orWait)
        {
            prevResult = prevOr || prevResult;
        }

        return prevResult;
    }

    private string ConvertToC(string value, ITableProperty property)
    {
        value = value[1..^1];
        if (value.Length < property.Size)
            value += new string('\0', property.Size - value.Length);
        return value;
    }

    private string ConvertToN(string value, ITableProperty property)
    {
        if (value.Length < property.Size)
        {
            char sign = '+';
            if (value[0] == '-' || value[0] == '+')
            {
                sign = value[0];
                value = value[1..];
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
                right = right + new string('0', property.Precision - right.Length);

            if (left.All(c => c == '0') && right.All(c => c == '0'))
                sign = '+';
                    
            value = sign + left + '.' + right;
        }

        return value;
    }
}