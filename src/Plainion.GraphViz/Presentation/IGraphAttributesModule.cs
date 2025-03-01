using System.Collections.Generic;
using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Presentation;

public interface IGraphAttributesModule : IModule<GraphAttribute>
{
    /// <summary>
    /// Empty values are considered as "not set" and are filtered out.
    /// </summary>
    IEnumerable<GraphAttribute> ItemsFor(LayoutAlgorithm algo);
}