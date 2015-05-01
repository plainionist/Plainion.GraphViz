using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Controls;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.Services
{
    [Export( typeof( IPresentationCreationService ) )]
    public class PresentationCreationService : IPresentationCreationService
    {
        [Import]
        internal ConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Optional path can be specified to check for "context related" configuration.
        /// </summary>
        /// <returns></returns>
        public IGraphPresentation CreatePresentation( string dataRoot )
        {
            if ( !string.IsNullOrEmpty( dataRoot ) )
            {
                ConfigurationService.Update( dataRoot );
            }

            var presentation = new GraphPresentation();

            if ( ConfigurationService.Config.NodeIdAsDefaultToolTip )
            {
                presentation.GetPropertySetFor<ToolTipContent>().DefaultProvider = id => new ToolTipContent( id, new TextBlock { Text = id } );
            }

            presentation.GetModule<CaptionModule>().LabelConverter = new GenericLabelConverter( ConfigurationService.Config.LabelConversion );

            return presentation;
        }
    }
}
