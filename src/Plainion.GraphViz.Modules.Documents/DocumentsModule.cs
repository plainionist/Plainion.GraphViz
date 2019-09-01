using System.ComponentModel.Composition;
using Plainion.GraphViz.Infrastructure.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;
using Unity.Lifetime;

namespace Plainion.GraphViz.Modules.Documents
{
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

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.OpenDocuments, typeof(OpenDocumentsView));
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.SaveDocuments, typeof(SaveDocumentsView));

            myContainer.RegisterType<IDocumentLoader, OpenDocumentsViewModel>(new ContainerControlledLifetimeManager());
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
