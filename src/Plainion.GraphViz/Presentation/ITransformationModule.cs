using System.Collections.Generic;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Allows transformation of the graph (e.g. folding of nodes)
    /// </summary>
    public interface ITransformationModule : IModule<IGraphTransformation>
    {
        void Add( IGraphTransformation transformation );
        
        void Remove( IGraphTransformation transformation );

        IGraph Graph { get; }
    }
}
