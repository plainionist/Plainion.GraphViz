using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;


namespace Plainion.GraphViz.Pioneer.Packaging
{
    [ContentProperty("Packages")]
    public class Config
    {
        public Config()
        {
            Packages = new List<Package>();
        }
        
        public string AssemblyRoot { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<Package> Packages { get; private set; }
    }
}
