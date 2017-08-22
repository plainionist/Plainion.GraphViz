using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.Documents
{
    [ModuleExport( typeof( DocumentsModule ) )]
    public class DocumentsModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.OpenDocuments, typeof(OpenDocumentsView));
            RegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.SaveDocuments, typeof(SaveDocumentsView));
        }
    }
}
