namespace Sqlol.Configurations.Factories.Operations.Numbers;

public class LessEqualsOperation : IOperation
{
    public bool GetResult(string actual, string expected)
    {
        if (actual[0] == '+' && expected[0] == '-') return false;
        if (actual[0] == '-' && expected[0] == '+') return true;
        actual = actual[1..];
        expected = expected[1..];
        
        bool isActualLonger = actual.Length > expected.Length;
        int length = isActualLonger ? expected.Length : actual.Length;
        
        for (int i = 0; i < length; i++)
        {
            if(actual[i] == expected[i] && actual[i] == '.')continue;
            if (actual[i] == '.') return true;
            if (expected[i] == '.') return false;
            if (actual[i] > expected[i]) return false;
            if (actual[i] < expected[i]) return true;
        }

        return true;
    }
}