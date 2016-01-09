using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace Plainion.GraphViz.Modules.Editor
{
    [ModuleExport( typeof( EditorModule ) )]
    public class EditorModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion( Infrastructure.RegionNames.AddIns, typeof( DotLangEditorMenuItem ) );
            RegionManager.RegisterViewWithRegion( RegionNames.DotLangEditor, typeof( DotLangEditorView ) );
        }
    }
}
