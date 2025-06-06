﻿using Plainion.GraphViz.Viewer.Abstractions.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using Unity;
using Unity.Lifetime;

namespace Plainion.GraphViz.Modules.Documents
{
    public class DocumentsModule : IModule
    {
        private IRegionManager myRegionManager;
        private IUnityContainer myContainer;

        public DocumentsModule(IRegionManager regionManager, IUnityContainer container)
        {
            myRegionManager = regionManager;
            myContainer = container;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.PrimaryToolBox, typeof(OpenDocumentsView));
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.PrimaryToolBox, typeof(SaveDocumentsView));

            myContainer.RegisterType<IDocumentLoader, OpenDocumentsViewModel>(new ContainerControlledLifetimeManager());
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
