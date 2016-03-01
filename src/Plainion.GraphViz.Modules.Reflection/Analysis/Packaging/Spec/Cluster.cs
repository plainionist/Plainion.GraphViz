using System.Linq;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec
{
    public class Cluster : PackageBase
    {
        internal bool Matches(string str)
        {
            return Includes.Any(i => i.Matches(str)) && !Excludes.Any(e => e.Matches(str));
        }
    }
}
