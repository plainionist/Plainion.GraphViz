using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;

namespace Plainion.GraphViz.Modules.Editor
{
    [Export( typeof( DotLangEditorViewModel ) )]
    public class DotLangEditorViewModel : ViewModelBase
    {
        public DotLangEditorViewModel()
        {
            OpenBrowserCommand = new DelegateCommand<string>( OpenBrowser );
        }

        [Import]
        public IPresentationCreationService PresentationCreationService { get; set; }

        protected override void OnModelPropertyChanged( string propertyName )
        {
        }

        public ICommand OpenBrowserCommand { get; private set; }

        private void OpenBrowser( string url )
        {
            Process.Start( url );
        }

        //var presentation = PresentationCreationService.CreatePresentation( Path.GetDirectoryName( AssemblyToAnalyseLocation ) );

        //var captionModule = presentation.GetPropertySetFor<Caption>();
        //var tooltipModule = presentation.GetPropertySetFor<ToolTipContent>();
        //var edgeStyleModule = presentation.GetPropertySetFor<EdgeStyle>();

        //presentation.Graph = document.Graph;

        //foreach ( var desc in document.Descriptors )
        //{
        //    captionModule.Add( new Caption( desc.Id, desc.Name ) );
        //    tooltipModule.Add( new ToolTipContent( desc.Id, desc.FullName ) );
        //}

        //foreach ( var entry in document.EdgeTypes )
        //{
        //    edgeStyleModule.Add( new EdgeStyle( entry.Key )
        //    {
        //        Color = entry.Value == EdgeType.DerivesFrom ? Brushes.Black : Brushes.Blue
        //    } );
        //}

        //if ( myAddToGraph && Model.Presentation != null && Model.Presentation.Graph != null )
        //{
        //    presentation = Model.Presentation.UnionWith( presentation,
        //        () => PresentationCreationService.CreatePresentation( Path.GetDirectoryName( AssemblyToAnalyseLocation ) ) );

        //    myAddToGraph = false;
        //}

    }
}
