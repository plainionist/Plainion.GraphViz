using System;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class ChangeClusterAssignment
    {
        private readonly IGraphPresentation myPresentation;

        public ChangeClusterAssignment( IGraphPresentation presentation )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            myPresentation = presentation;
        }

        public void Execute( Action<DynamicClusterTransformation> action )
        {
            var transformations = myPresentation.GetModule<ITransformationModule>();
            var transformation = transformations.Items
                .OfType<DynamicClusterTransformation>()
                .SingleOrDefault();

            if( transformation == null )
            {
                transformation = new DynamicClusterTransformation();

                action( transformation );

                transformations.Add( transformation );
            }
            else
            {
                action( transformation );
            }
        }
    }
}
