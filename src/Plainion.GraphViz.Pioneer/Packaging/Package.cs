using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace Plainion.GraphViz.Pioneer.Packaging
{
    [ContentProperty("Patterns")]
    public class Package
    {
        public Package()
        {
            Patterns = new List<FilePattern>();
        }

        public string Name { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<FilePattern> Patterns { get; private set; }

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
