using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// provides generic multi-pass label conversion
    /// </summary>
    public class GenericLabelConverter : ILabelConverter
    {
        private IEnumerable<ILabelConversionStep> myConversionSteps;

        public GenericLabelConverter( IEnumerable<ILabelConversionStep> steps )
        {
            myConversionSteps = steps.ToList();
        }

        public string Convert( string originalLabel )
        {
            var label = originalLabel;

            foreach( var step in myConversionSteps )
            {
                label = step.Convert( label );
            }

            return label;
        }
    }
}
