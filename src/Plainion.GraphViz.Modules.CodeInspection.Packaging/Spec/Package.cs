using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec
{
    [ContentProperty( "Patterns" )]
    public class Package : PackageBase
    {
        public Package()
        {
            Clusters = new List<Cluster>();
        }

        [DefaultValue(null)]
        public string AutoClusters { get; set; }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
        public List<Cluster> Clusters { get; private set; }
    }
}
