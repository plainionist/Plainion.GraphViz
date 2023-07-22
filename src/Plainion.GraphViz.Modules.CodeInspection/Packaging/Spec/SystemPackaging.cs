using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec
{
    [ContentProperty("Packages")]
    public class SystemPackaging
    {
        public SystemPackaging()
        {
            Packages = new List<Package>();
        }
        
        public bool NetFramework { get; set; }
        
        public bool UsedTypesOnly { get; set; }

        public string AssemblyRoot { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<Package> Packages { get; private set; }
    }
}
