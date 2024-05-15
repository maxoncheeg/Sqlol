namespace Sqlol.Configurations.Factories.Operations.Numbers;

public class GreaterEqualsOperation : IOperation
{
    public bool GetResult(string actual, string expected)
    {
        if (actual[0] == '+' && expected[0] == '-') return true;
        if (actual[0] == '-' && expected[0] == '+') return false;
        actual = actual[1..];
        expected = expected[1..];
        
        bool isActualLonger = actual.Length > expected.Length;
        int length = isActualLonger ? expected.Length : actual.Length;
        
        for (int i = 0; i < length; i++)
        {
            if(actual[i] == expected[i] && actual[i] == '.')continue;
            if (actual[i] == '.') return false;
            if (expected[i] == '.') return true;
            if (actual[i] > expected[i]) return true;
            if (actual[i] < expected[i]) return false;
        }

        return true;
    }
}