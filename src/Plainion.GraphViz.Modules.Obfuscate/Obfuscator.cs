namespace Plainion.GraphViz.Modules.Obfuscate;

using System.Collections.Generic;

internal class Obfuscator
{
    private readonly Dictionary<string, string> myMap = [];
    private int myCounter = 0;

    internal string Obfuscate(string value)
    {
        if (myMap.TryGetValue(value, out var obfuscated))
        {
            return obfuscated;
        }

        obfuscated = GetNextObfuscation();
        myMap[value] = obfuscated;

        return obfuscated;
    }

    private string GetNextObfuscation()
    {
        int value = myCounter++;
        string result = "";

        while (true)
        {
            result = (char)('A' + (value % 26)) + result;
            value = value / 26 - 1;

            if (value < 0) break;
        }

        return result;
    }
}