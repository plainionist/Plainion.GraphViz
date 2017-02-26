using System.Linq;
using System.Windows.Markup;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec
{
    [ContentProperty( "Patterns" )]
    public class Cluster : PackageBase
    {
        public string Id { get; set; }

        internal bool Matches( string str )
        {
            return Includes.Any(i => i.Matches(str)) && !Excludes.Any(e => e.Matches(str));
        }
    }
}
