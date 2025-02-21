using Plainion.GraphViz.Viewer.Abstractions.Services;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.Services
{
    class PresentationCreationService : IPresentationCreationService
    {
        private ConfigurationService myConfigurationService;

        public PresentationCreationService(ConfigurationService configurationService)
        {
            myConfigurationService = configurationService;
        }

        /// <summary>
        /// Optional path can be specified to check for "context related" configuration.
        /// </summary>
        /// <returns></returns>
        public IGraphPresentation CreatePresentation(string dataRoot)
        {
            if (!string.IsNullOrEmpty(dataRoot))
            {
                myConfigurationService.Update(dataRoot);
            }

            var presentation = new GraphPresentation();

            presentation.GetModule<CaptionModule>().LabelConverter = new GenericLabelConverter(myConfigurationService.Config.LabelConversion);

            return presentation;
        }
    }
}
