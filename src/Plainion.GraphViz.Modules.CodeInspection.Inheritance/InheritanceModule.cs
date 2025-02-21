using Plainion.GraphViz.Modules.CodeInspection.Inheritance;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection
{
    public class InheritanceModule : IModule
    {
        private IRegionManager myRegionManager;

        public InheritanceModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(InheritanceGraphMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.InheritanceGraphBuilder, typeof(InheritanceGraphBuilderView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<InheritanceClient>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
