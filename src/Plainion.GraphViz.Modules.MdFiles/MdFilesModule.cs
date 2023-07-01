using System.IO.Abstractions;
using Plainion.GraphViz.Modules.MdFiles.Dependencies;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Parser;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Resolver;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Verifier;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.GraphViz.Modules.MdFiles
{
    public class MdFilesModule : IModule
    {
        private readonly IRegionManager myRegionManager;

        public MdFilesModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Infrastructure.RegionNames.AddIns, typeof(ToolsMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.MdFilesDependencies, typeof(ConfigurationView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<Analyzer>();
            containerRegistry.RegisterSingleton<IMarkdownParser, MarkdigParser>();
            containerRegistry.RegisterSingleton<ILinkResolver, LinkResolver>();
            containerRegistry.RegisterSingleton<ILinkVerifier, LinkVerifier>();
            containerRegistry.RegisterSingleton<IFileSystem, FileSystem>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}