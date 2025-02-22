using System.Text.RegularExpressions;

namespace Plainion.GraphViz.CodeInspection;

/// <summary>
/// Represents a wildcard running on the <see cref="System.Text.RegularExpressions"/> engine.
/// Supported wildcard characters are "*" (any sequence of any character) and "?" (any but single character).
/// The pattern must match the whole input.
/// </summary>
public class Wildcard : Regex
{
    public Wildcard(string pattern)
        : base(WildcardToRegex(pattern))
    {
    }

    /// <summary>
    /// Initializes a wildcard with the given search pattern and options.
    /// </summary>
    public Wildcard(string pattern, RegexOptions options)
        : base(WildcardToRegex(pattern), options)
    {
    }

    public static string WildcardToRegex(string pattern)
    {
        return "^" + Escape(pattern).
            Replace("\\*", ".*").
            Replace("\\?", ".") + "$";
    }

    public new static bool IsMatch(string input, string pattern)
    {
        return new Wildcard(pattern).IsMatch(input);
    }
}
