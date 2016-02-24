using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace Plainion.GraphViz.Pioneer.Spec
{
    [ContentProperty("Patterns")]
    public class PackageBase
    {
        public PackageBase()
        {
            Patterns = new List<Wildcard>();
        }

        public string Name { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<Wildcard> Patterns { get; private set; }

        public IEnumerable<Include> Includes
        {
            get { return Patterns.OfType<Include>(); }
        }

        public IEnumerable<Exclude> Excludes
        {
            get { return Patterns.OfType<Exclude>(); }
        }
    }
}
