using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.Services
{
    [Export( typeof( IPresentationCreationService ) )]
    class PresentationCreationService : IPresentationCreationService
    {
        private ConfigurationService myConfigurationService;

        [ImportingConstructor]
        public PresentationCreationService(ConfigurationService configurationService)
        {
            myConfigurationService = configurationService;
        }

        /// <summary>
        /// Optional path can be specified to check for "context related" configuration.
        /// </summary>
        /// <returns></returns>
        public IGraphPresentation CreatePresentation( string dataRoot )
        {
            if ( !string.IsNullOrEmpty( dataRoot ) )
            {
                myConfigurationService.Update( dataRoot );
            }

            var presentation = new GraphPresentation();

            if ( myConfigurationService.Config.NodeIdAsDefaultToolTip )
            {
                presentation.GetPropertySetFor<ToolTipContent>().DefaultProvider = id => new ToolTipContent( id, new TextBlock { Text = id } );
            }

            presentation.GetModule<CaptionModule>().LabelConverter = new GenericLabelConverter( myConfigurationService.Config.LabelConversion );

            return presentation;
        }
    }
}
