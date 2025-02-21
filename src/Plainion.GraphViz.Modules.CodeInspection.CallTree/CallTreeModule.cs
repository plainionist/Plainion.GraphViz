using Plainion.GraphViz.Modules.CodeInspection.CallTree;
using Plainion.GraphViz.Modules.CodeInspection.CallTree.Actors;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.CodeInspection
{
    public class CallTreeModule : IModule
    {
        private IRegionManager myRegionManager;

        public CallTreeModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(CallTreeMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.CallTree, typeof(CallTreeView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<CallTreeClient>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
