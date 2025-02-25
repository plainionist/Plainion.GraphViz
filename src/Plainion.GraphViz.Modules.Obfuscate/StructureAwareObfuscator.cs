using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Modules.Obfuscate;

internal class StructureAwareObfuscator : IObfuscator
{
    private readonly Dictionary<string, string> myMap = [];
    private int myCounter = 0;

    public string Obfuscate(string value)
    {
        var parts = value.Split('.');
        var obfuscatedParts = parts.Select(ObfuscatePart).ToArray();
        return string.Join(".", obfuscatedParts);
    }

    private string ObfuscatePart(string part)
    {
        if (myMap.TryGetValue(part, out var obfuscated))
        {
            return obfuscated;
        }

        obfuscated = GetNextObfuscation();
        myMap[part] = obfuscated;

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
