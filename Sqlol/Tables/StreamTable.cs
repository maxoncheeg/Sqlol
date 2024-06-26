﻿using System.Text;
using Sqlol.Configurations.Factories;
using Sqlol.Expressions;
using Sqlol.Tables.Properties;

namespace Sqlol.Tables;

public class StreamTable : ITable
{
    private readonly IOperationFactory _operationFactory;
    private Stream _tableStream;

    private readonly List<ITableProperty> _properties;
    private int _readData;

    public string Name { get; }
    public bool HasMemoFile { get; private set; }
    public DateTime LastUpdateDate { get; private set; }
    public int RecordsAmount { get; private set; }
    public short HeaderLength { get; private set; }
    public short RecordLength { get; private set; }
    public IReadOnlyList<ITableProperty> Properties => _properties;

    public StreamTable(string name, Stream tableStream, IList<ITableProperty> properties, IOperationFactory factory)
    {
        _operationFactory = factory;
        Name = name;
        LastUpdateDate = DateTime.Now;

        HeaderLength = (short)(32 + 16 * properties.Count);
        RecordLength = (short)(properties.Sum(p => p.Size) + 1);
        _properties = properties.ToList();

        RecordsAmount = 0;

        if (Properties.FirstOrDefault(p =>
                p.Type.ToString().Equals("M", StringComparison.InvariantCultureIgnoreCase)) != null)
        {
            UpdateMemo();
            HasMemoFile = true;
        }

        _tableStream = tableStream;

        UpdateHeader(true);
    }

    public StreamTable(string name, Stream tableStream, IOperationFactory factory)
    {
        _operationFactory = factory;
        Name = name;
        _tableStream = tableStream;

        _tableStream.Seek(0, SeekOrigin.Begin);
        byte[] buffer = new byte[32];

        _readData = _readData = _tableStream.Read(buffer, 0, 1);
        HasMemoFile = File.Exists($"{name}.dbt");

        _readData = _tableStream.Read(buffer, 0, 3);
        LastUpdateDate = new(2000 + buffer[0], buffer[1], buffer[2]);

        _readData = _tableStream.Read(buffer, 0, sizeof(int));
        RecordsAmount = BitConverter.ToInt32(buffer, 0);

        _readData = _tableStream.Read(buffer, 0, sizeof(short));
        HeaderLength = BitConverter.ToInt16(buffer, 0);

        _readData = _tableStream.Read(buffer, 0, sizeof(short));
        RecordLength = BitConverter.ToInt16(buffer, 0);

        _tableStream.Seek(20, SeekOrigin.Current);
        int propertiesLength = HeaderLength - 32;

        _properties = [];
        while (propertiesLength > 0)
        {
            ITableProperty property;

            _readData = _tableStream.Read(buffer, 0, 11);
            var propertyName = Encoding.GetEncoding(1251).GetString(buffer, 0, 11);
            propertyName = propertyName.Trim('\0').Trim();

            _readData = _tableStream.Read(buffer, 0, 5);

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

            WriteRecord(data);
            UpdateHeader();
        }
        catch (Exception e)
        {
            return false;
        }

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

        byte[] buffer = new byte[RecordLength];
        int count = RecordsAmount;
        while (count-- > 0)
        {
            _readData = _tableStream.Read(buffer, 0, RecordLength);

            bool onDelete = buffer[0] == '*';
            if (onDelete) continue;

            int offset = 1;
            List<string> variables = [];
            foreach (var property in Properties)
            {
                string value = Encoding.GetEncoding(1251).GetString(buffer, offset, property.Size);
                if (property.Type == 'C')
                    value = '"' + value.Trim('\0') + '"';
                if (property.Type == 'M')
                {
                    short index = short.Parse(value);
                    value = ReadMemo(index);
                    value = '"' + value.Trim('\0') + '"';
                }

                variables.Add(value);

                offset += property.Size;

                List<int> indexes = [];
                for (int i = 0; i < columns.Count; i++)
                {
                    if (columns[i].Equals(property.Name, StringComparison.InvariantCultureIgnoreCase))
                        indexes.Add(i);
                }

                foreach (var index in indexes)
                    result[index] = value;
            }

            List<string> record = [..result];

            if (expression != null && CheckExpression(expression, Properties.Select(p => p.Name).ToList(), variables))
                data.AddRecord(record);
            else if (expression == null) data.AddRecord(record);
        }

        return data;
    }

    public int Update(IList<Tuple<string, string>> changes, IExpression? expression = null)
    {
        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);
        int amount = 0;

        byte[] buffer = new byte[RecordLength];
        int count = RecordsAmount;
        while (count-- > 0)
        {
            _readData = _tableStream.Read(buffer, 0, RecordLength);
            bool onDelete = buffer[0] == '*';
            if (onDelete) continue;

            int offset = 1;
            List<string> variables = [];
            foreach (var property in Properties)
            {
                string value = Encoding.GetEncoding(1251).GetString(buffer, offset, property.Size);
                variables.Add(value);
                offset += property.Size;
            }

            if (expression != null && CheckExpression(expression, Properties.Select(p => p.Name).ToList(), variables) ||
                expression == null)
            {
                _tableStream.Seek(-RecordLength, SeekOrigin.Current);

                List<Tuple<string, string>> data = [];

                for (int i = 0; i < Properties.Count; i++)
                    if (changes.FirstOrDefault(t =>
                            string.Equals(t.Item1, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase)) is
                        { } tuple)
                        data.Add(new(tuple.Item1, tuple.Item2));
                    else
                        data.Add(new(Properties[i].Name, variables[i]));

                RecordsAmount--;
                WriteRecord(data);

                amount++;
            }
        }

        return amount;
    }

    public int Truncate()
    {
        var data = Select();
        var truncateCount = RecordsAmount - data.Values.Count;
        if (truncateCount == 0) return truncateCount;
        RecordsAmount = 0;

        _tableStream.Close();

        File.WriteAllText(Name + ".dbf", string.Empty);
        UpdateMemo();
        _tableStream = File.Open(Name + ".dbf", FileMode.Open);
        UpdateHeaderVariables();
        UpdateHeader(true);

        foreach (var value in data.Values)
        {
            List<Tuple<string, string>> record = [];
            for (int i = 0; i < data.Columns.Count; i++)
                record.Add(new(data.Columns[i], value[i]));
            Insert(record);
        }

        UpdateHeader();

        return truncateCount;
    }

    public int Delete(IExpression? expression = null)
    {
        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);
        int amount = 0;

        byte[] buffer = new byte[RecordLength];
        int count = RecordsAmount;
        while (count-- > 0)
        {
            _readData = _tableStream.Read(buffer, 0, RecordLength);
            bool onDelete = buffer[0] == '*';
            if (onDelete) continue;

            int offset = 1;
            List<string> variables = [];
            foreach (var property in Properties)
            {
                string value = Encoding.GetEncoding(1251).GetString(buffer, offset, property.Size);
                if (property.Type == 'C')
                    value = '"' + value.Trim('\0') + '"';
                if (property.Type == 'M')
                {
                    short index = short.Parse(value);
                    value = ReadMemo(index);
                    value = '"' + value.Trim('\0') + '"';
                }

                variables.Add(value);
                offset += property.Size;
            }

            if (expression != null && CheckExpression(expression, Properties.Select(p => p.Name).ToList(), variables) ||
                expression == null)
            {
                _tableStream.Seek(-RecordLength, SeekOrigin.Current);
                _tableStream.Write([(byte)'*']);
                _tableStream.Seek(RecordLength - 1, SeekOrigin.Current);
                amount++;
            }
        }

        return amount;
    }

    public int Restore(IExpression? expression = null)
    {
        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);
        int amount = 0;

        byte[] buffer = new byte[RecordLength];
        int count = RecordsAmount;
        while (count-- > 0)
        {
            _readData = _tableStream.Read(buffer, 0, RecordLength);
            bool onDelete = buffer[0] == '*';
            if (!onDelete) continue;

            int offset = 1;
            List<string> variables = [];
            foreach (var property in Properties)
            {
                string value = Encoding.GetEncoding(1251).GetString(buffer, offset, property.Size);
                if (property.Type == 'C')
                    value = '"' + value.Trim('\0') + '"';
                if (property.Type == 'M')
                {
                    short index = short.Parse(value);
                    value = ReadMemo(index);
                    value = '"' + value.Trim('\0') + '"';
                }

                variables.Add(value);
                offset += property.Size;
            }

            if (expression != null && CheckExpression(expression, Properties.Select(p => p.Name).ToList(), variables) ||
                expression == null)
            {
                _tableStream.Seek(-RecordLength, SeekOrigin.Current);
                _tableStream.Write([32]);
                _tableStream.Seek(RecordLength - 1, SeekOrigin.Current);
                amount++;
            }
        }

        return amount;
    }

    public bool AddColumn(ITableProperty newProperty)
    {
        if (newProperty.Index != Properties.Last().Index + 1) return false;
        if (Properties.FirstOrDefault(p =>
                p.Name.Equals(newProperty.Name, StringComparison.InvariantCultureIgnoreCase)) != null) return false;

        List<Tuple<bool, List<string>>> records = [];
        byte[] buffer = new byte[RecordLength];
        int count = RecordsAmount;

        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);

        while (count-- > 0)
        {
            _readData = _tableStream.Read(buffer, 0, RecordLength);

            bool onDelete = buffer[0] == '*';
            int offset = 1;
            List<string> variables = [];

            foreach (var property in Properties)
            {
                string value = Encoding.GetEncoding(1251).GetString(buffer, offset, property.Size);
                if (property.Type == 'C')
                    value = '"' + value.Trim('\0') + '"';
                if (property.Type == 'M')
                {
                    short index = short.Parse(value);
                    value = ReadMemo(index);
                    value = '"' + value.Trim('\0') + '"';
                }

                variables.Add(value);
                offset += property.Size;
            }

            //variables.Add(new string('\0', newProperty.Size));
            records.Add(new(onDelete, variables));
        }

        _properties.Add(newProperty);

        RecordsAmount = 0;
        if (Properties.FirstOrDefault(p =>
                p.Type.ToString().Equals("M", StringComparison.InvariantCultureIgnoreCase)) != null)
        {
            UpdateMemo();
            HasMemoFile = true;
        }
        UpdateHeaderVariables();
        UpdateHeader(true);

        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);
        foreach (var tuple in records)
        {
            List<Tuple<string, string>> record = [];
            for (int i = 0; i < Properties.Count && i < tuple.Item2.Count; i++)
                record.Add(new(Properties[i].Name, tuple.Item2[i]));
            WriteRecord(record, tuple.Item1);
        }

        UpdateHeader();

        return true;
    }

    public bool RemoveColumn(string columnName)
    {
        if (Properties.FirstOrDefault(p =>
                p.Name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) is not
            { } propertyOnRemove) return false;

        List<Tuple<bool, List<string>>> records = [];
        byte[] buffer = new byte[RecordLength];
        int count = RecordsAmount;
        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);

        while (count-- > 0)
        {
            _readData = _tableStream.Read(buffer, 0, RecordLength);

            bool onDelete = buffer[0] == '*';
            int offset = 1;
            List<string> variables = [];

            foreach (var property in Properties)
            {
                if (property != propertyOnRemove)
                {
                    string value = Encoding.GetEncoding(1251).GetString(buffer, offset, property.Size);
                    if (property.Type == 'C')
                        value = '"' + value.Trim('\0') + '"';
                    if (property.Type == 'M')
                    {
                        short index = short.Parse(value);
                        value = ReadMemo(index);
                        value = '"' + value.Trim('\0') + '"';
                    }

                    variables.Add(value);
                }

                offset += property.Size;
            }

            records.Add(new(onDelete, variables));
        }

        _properties.Remove(propertyOnRemove);

        for (int i = 0; i < Properties.Count; i++)
            Properties[i].Index = (byte)i;

        RecordsAmount = 0;
        UpdateMemo();
        UpdateHeaderVariables();
        UpdateHeader(true);

        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);
        foreach (var tuple in records)
        {
            List<Tuple<string, string>> record = [];
            for (int i = 0; i < Properties.Count && i < tuple.Item2.Count; i++)
                record.Add(new(Properties[i].Name, tuple.Item2[i]));
            WriteRecord(record, tuple.Item1);
        }

        UpdateHeader();

        return true;
    }

    public bool RenameColumn(string currentName, string newName)
    {
        var property = Properties.FirstOrDefault(property => property.Name.Trim('\0') == currentName);
        if (property == null) return false;

        if (newName.Length > 11) return false;
        property.Name = newName;
        UpdateHeader(true);

        return true;
    }

    public bool UpdateColumn(string columnName, ITableProperty newProperty)
    {
        if (Properties.FirstOrDefault(p =>
                p.Name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) is not
            { } propertyOnRemove) return false;
        newProperty.Index = propertyOnRemove.Index;

        List<Tuple<bool, List<string>>> records = [];
        byte[] buffer = new byte[RecordLength];
        int count = RecordsAmount;

        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);
        while (count-- > 0)
        {
            _readData = _tableStream.Read(buffer, 0, RecordLength);

            bool onDelete = buffer[0] == '*';
            int offset = 1;
            List<string> variables = [];


            foreach (var property in Properties)
            {
                string value = Encoding.GetEncoding(1251).GetString(buffer, offset, property.Size);
                if (property.Type == 'C')
                    value = '"' + value.Trim('\0') + '"';
                if (property.Type == 'M')
                {
                    short index = short.Parse(value);
                    value = ReadMemo(index);
                    value = '"' + value.Trim('\0') + '"';
                }

                variables.Add(value);
                offset += property.Size;
            }

            records.Add(new(onDelete, variables));
        }

        _properties.Remove(propertyOnRemove);
        _properties.Insert(newProperty.Index, newProperty);

        RecordsAmount = 0;
        if (Properties.FirstOrDefault(p =>
                p.Type.ToString().Equals("M", StringComparison.InvariantCultureIgnoreCase)) != null)
        {
            UpdateMemo();
            HasMemoFile = true;
        }
        UpdateHeaderVariables();
        UpdateHeader(true);

        _tableStream.Seek(HeaderLength, SeekOrigin.Begin);
        foreach (var tuple in records)
        {
            List<Tuple<string, string>> record = [];
            for (int i = 0; i < Properties.Count && i < tuple.Item2.Count; i++)
                if (i != newProperty.Index)
                    record.Add(new(Properties[i].Name, tuple.Item2[i]));
                else
                {
                    char prev = propertyOnRemove.Type, now = Properties[i].Type;
                    if (prev == now && prev != 'C' && prev != 'N')
                        record.Add(new(Properties[i].Name, tuple.Item2[i]));
                    if (now == 'C')
                    {
                        if (Properties[i].Width >= propertyOnRemove.Width)
                            record.Add(new(Properties[i].Name, tuple.Item2[i]));
                        else
                        {
                            string newValue = tuple.Item2[i];
                            if (prev == 'M' || prev == 'C')
                                newValue = newValue[1..^1];
                            newValue = newValue[..Properties[i].Width];
                            newValue = '"' + newValue + '"';

                            record.Add(new(Properties[i].Name, tuple.Item2[i]));
                        }
                    }

                    if (now == 'N' && (prev == 'N' || prev == 'D'))
                    {
                        if (prev == 'N' && propertyOnRemove.Width <= Properties[i].Width &&
                            propertyOnRemove.Precision <= Properties[i].Precision)
                            record.Add(new(Properties[i].Name, tuple.Item2[i]));
                        else if (prev == 'D' && Properties[i].Width >= propertyOnRemove.Width)
                            record.Add(new(Properties[i].Name, tuple.Item2[i]));
                        else if (prev == 'N')
                        {
                            record.Add(new(Properties[i].Name, ConvertToN(tuple.Item2[i], Properties[i])));
                        }
                    }
                }

            WriteRecord(record, tuple.Item1);
        }

        UpdateHeader();

        return true;
    }

    #endregion

    public void Dispose()
    {
        _tableStream.Close();
        _tableStream.Dispose();
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
                _tableStream.Write([property.Width]);
                _tableStream.Write([property.Precision]);
                _tableStream.Write([property.Size]);
                _tableStream.Write([property.Index]);
            }
    }

    private void UpdateHeaderVariables()
    {
        HeaderLength = (short)(32 + 16 * Properties.Count);
        RecordLength = (short)(Properties.Sum(p => p.Size) + 1);
    }

    private void WriteRecord(IList<Tuple<string, string>> data, bool onDelete = false)
    {
        List<byte> buffer = new();

        buffer.Add(onDelete ? (byte)'*' : (byte)' ');

        foreach (var property in Properties)
        {
            var tuple = data.FirstOrDefault(t =>
                string.Equals(t.Item1, property.Name, StringComparison.InvariantCultureIgnoreCase));

            if (tuple != null)
            {
                string value = tuple.Item2;
                if (property.Type != 'M' && value.Length > property.Size)
                    throw new ArgumentException("Поле больше позволяемой длины");

                if (value.Length != property.Size)
                    switch (property.Type.ToString().ToUpperInvariant())
                    {
                        case "C":
                            value = ConvertToC(value, property);
                            break;
                        case "N":
                            value = ConvertToN(value, property);
                            break;
                        case "M":
                        {
                            value = WriteMemo(value).ToString();
                            if (value.Length < 5)
                                value = new string('0', 5 - value.Length) + value;
                        }
                            break;
                    }

                buffer.AddRange(Encoding.GetEncoding(1251).GetBytes(value));
            }
            else
            {
                // todo: значения по умолчанию для каждого типа
                string emptyValue = "";

                switch (property.Type.ToString().ToLowerInvariant())
                {
                    case "n":
                        emptyValue = ConvertToN(emptyValue, property);
                        break;
                    case "c":
                        emptyValue = ConvertToC(emptyValue, property);
                        break;
                    case "l":
                        emptyValue = "?";
                        break;
                    case "d":
                        emptyValue = "20040330";
                        break;
                    case "m":
                    {
                        emptyValue = WriteMemo(emptyValue).ToString();
                        if (emptyValue.Length < 5)
                            emptyValue = new string('0', 5 - emptyValue.Length);
                    }
                        break;
                }

                buffer.AddRange(Encoding.GetEncoding(1251).GetBytes(emptyValue));
            }
        }

        _tableStream.Write(buffer.ToArray());
        RecordsAmount++;
        LastUpdateDate = DateTime.Now;
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

                var property = Properties.FirstOrDefault(p => p.Name == f.Field);
                //Console.Write(record[index]);

                if (property == null) return false;
                string expectedValue = f.Value;
                string actualValue = record[index];
                switch (property.Type)
                {
                    case 'C':
                        actualValue = actualValue.Trim('\0');
                        actualValue = '"' + actualValue + '"';
                        break;
                    case 'N':
                        expectedValue = ConvertToN(expectedValue, property);
                        break;
                }

                var result = _operationFactory.GetOperation(f.Operation).GetResult(actualValue, expectedValue);
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
        if (value.Length >= 2)
            value = value[1..^1];
        if (value.Length < property.Size)
            value += new string('\0', property.Size - value.Length);
        return value;
    }

    private string ConvertToN(string value, ITableProperty property)
    {
        char sign = '+';

        if (value.Length > 0 && (value[0] == '-' || value[0] == '+'))
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
        if (left.Length > property.Width)
            left = left[^(property.Width)..];
        if (right.Length > property.Precision)
            right = right[..(property.Precision)];

        if (left.All(c => c == '0') && right.All(c => c == '0'))
            sign = '+';

        value = sign + left + '.' + right;


        return value;
    }

    private short WriteMemo(string value)
    {
        using Stream memo = File.Open($"{Name}.dbt", FileMode.Open);
        memo.Seek(0, SeekOrigin.Begin);
        byte[] shortNum = new byte[2];
        _readData = memo.Read(shortNum, 0, sizeof(short));
        short memoAmount = BitConverter.ToInt16(shortNum, 0);
        memo.Seek(0, SeekOrigin.Begin);
        memo.Write(BitConverter.GetBytes((short)(memoAmount + 1)));

        if (value.Length >= 2) value = value[1..^1];
        if (value.Length < 512) value = value + new string('\0', 512 - value.Length);

        memo.Seek(memoAmount * 512, SeekOrigin.Current);
        memo.Write(Encoding.GetEncoding(1251).GetBytes(value));

        return memoAmount;
    }

    private string ReadMemo(short index)
    {
        using Stream memo = File.Open($"{Name}.dbt", FileMode.Open);
        memo.Seek(2 + index * 512, SeekOrigin.Begin);
        byte[] buffer = new byte[512];
        _readData = memo.Read(buffer, 0, 512);
        return Encoding.GetEncoding(1251).GetString(buffer);
    }

    private void UpdateMemo()
    {
        File.WriteAllText(Name + ".dbt", string.Empty);
        using Stream memoFile = File.Create($"{Name}.dbt");
        memoFile.Write([0, 0]);
    }
}