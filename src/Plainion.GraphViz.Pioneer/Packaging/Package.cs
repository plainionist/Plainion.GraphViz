using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;

namespace Plainion.GraphViz.Pioneer.Packaging
{
    [ContentProperty("Includes")]
    public class Package
    {
        public Package()
        {
            Includes = new List<Include>();
        }
        
        public string Name { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<Include> Includes { get; private set; }
    }
}
