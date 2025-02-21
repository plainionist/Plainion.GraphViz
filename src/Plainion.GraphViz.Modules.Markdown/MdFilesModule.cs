using System.IO.Abstractions;
using Plainion.GraphViz.Modules.Markdown.Dependencies;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Parser;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Resolver;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Verifier;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Plainion.GraphViz.Modules.Markdown
{
    public class MarkdownModule : IModule
    {
        private readonly IRegionManager myRegionManager;

        public MarkdownModule(IRegionManager regionManager)
        {
            myRegionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            myRegionManager.RegisterViewWithRegion(Viewer.Abstractions.RegionNames.AddIns, typeof(ToolsMenuItem));
            myRegionManager.RegisterViewWithRegion(RegionNames.MarkdownDependencies, typeof(ConfigurationView));

            // explicitly register as singletons here to ensure that the host gets closed
            containerRegistry.RegisterSingleton<MarkdownAnalyzer>();
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