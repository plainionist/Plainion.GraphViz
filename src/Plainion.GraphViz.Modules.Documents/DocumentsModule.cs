using System.ComponentModel.Composition;
using Plainion.GraphViz.Infrastructure;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace Plainion.GraphViz.Modules.Documents
{
    [ModuleExport( typeof( DocumentsModule ) )]
    public class DocumentsModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion( Infrastructure.RegionNames.OpenDocuments, typeof( OpenDocumentsView ) );
        }
    }
}
