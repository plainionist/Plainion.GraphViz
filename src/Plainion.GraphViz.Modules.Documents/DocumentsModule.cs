using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.Documents
{
    [ModuleExport( typeof( DocumentsModule ) )]
    public class DocumentsModule : IModule
    {
        private IRegionManager myRegionManager;

        [ImportingConstructor]
        public DocumentsModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void Initialize()
        {
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.OpenDocuments, typeof(OpenDocumentsView));
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.SaveDocuments, typeof(SaveDocumentsView));
        }
    }
}
