using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace Plainion.GraphViz.Pioneer.Packaging
{
    [ContentProperty("Patterns")]
    public class Package : PackageBase
    {
        public Package()
        {
            Clusters = new List<Cluster>();
        }

        public List<Cluster> Clusters { get; private set; }
    }
}
