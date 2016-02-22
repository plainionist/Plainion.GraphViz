using System.Diagnostics;
using System.Text.RegularExpressions;

using Plainion.Text;
namespace Plainion.GraphViz.Pioneer.Packaging
{
    public class Wildcard
    {
        public string Pattern { get; set; }

        internal bool Matches(string file)
        {
            return new Plainion.Text.Wildcard(Pattern, RegexOptions.IgnoreCase).IsMatch(file);
        }
    }
}
