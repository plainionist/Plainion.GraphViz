using System.ComponentModel.Composition;
using Microsoft.Practices.Unity;
using Plainion.GraphViz.Infrastructure.Services;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.Documents
{
    [ModuleExport(typeof(DocumentsModule))]
    public class DocumentsModule : IModule
    {
        private IRegionManager myRegionManager;
        private IUnityContainer myContainer;

        [ImportingConstructor]
        public DocumentsModule(IRegionManager regionManager, IUnityContainer container)
        {
            myRegionManager = regionManager;
            myContainer = container;
        }

        public void Initialize()
        {
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.OpenDocuments, typeof(OpenDocumentsView));
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.SaveDocuments, typeof(SaveDocumentsView));

            myContainer.RegisterType<IDocumentLoader, OpenDocumentsViewModel>(new ContainerControlledLifetimeManager());
        }
    }
}
