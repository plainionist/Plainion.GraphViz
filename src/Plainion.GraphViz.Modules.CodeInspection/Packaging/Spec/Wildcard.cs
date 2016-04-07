using System.Text.RegularExpressions;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec
{
    public abstract class Wildcard
    {
        public string Pattern { get; set; }

        internal bool Matches(string file)
        {
            return new Plainion.Text.Wildcard("*" + Pattern + "*", RegexOptions.IgnoreCase).IsMatch(file);
        }
    }
}
