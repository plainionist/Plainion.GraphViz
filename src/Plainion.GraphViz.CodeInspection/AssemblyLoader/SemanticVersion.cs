using System;
using System.Text.RegularExpressions;

namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    public record SemanticVersion : IComparable<SemanticVersion>
    {
        private static readonly string myPreReleasePatternText = "(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*";
        private static readonly Regex myPreReleasePattern = new Regex($"^{myPreReleasePatternText}$", RegexOptions.Compiled);
        private static readonly Regex myPattern = new Regex($"^(?<major>0|[1-9]\\d*)\\.(?<minor>0|[1-9]\\d*)\\.(?<patch>0|[1-9]\\d*)(?:-(?<pre>{myPreReleasePatternText}))?", RegexOptions.Compiled);

        private SemanticVersion(int major, int minor, int patch, string preRelease)
        {
            Major = major;
            Minor = minor;
            Patch = patch;

            if (preRelease != null && !myPreReleasePattern.IsMatch(preRelease))
            {
                throw new ArgumentException($"Invalid format for pre-release: '{preRelease}'", nameof(preRelease));
            }

            PreRelease = preRelease;
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string PreRelease { get; }

        internal static SemanticVersion TryParse(string input)
        {
            Contract.RequiresNotNull(input, nameof(input));

            var match = myPattern.Match(input);

            if (!match.Success)
            {
                return null;
            }

            return new SemanticVersion(
                int.Parse(match.Groups["major"].Value),
                int.Parse(match.Groups["minor"].Value),
                int.Parse(match.Groups["patch"].Value),
                string.IsNullOrEmpty(match.Groups["pre"].Value) ? null : match.Groups["pre"].Value);
        }

        public override string ToString() =>
            $"{Major}.{Minor}.{Patch}" + (PreRelease != null ? "-" + PreRelease : "");

        public int CompareTo(SemanticVersion other)
        {
            if (other == null)
            {
                return 1;
            }

            var result = Major.CompareTo(other.Major);
            if (result != 0)
            {
                return result;
            }

            result = Minor.CompareTo(other.Minor);
            if (result != 0)
            {
                return result;
            }

            result = Patch.CompareTo(other.Patch);
            if (result != 0)
            {
                return result;
            }

            return StringComparer.Ordinal.Compare(PreRelease, other.PreRelease);
        }
    }
}
