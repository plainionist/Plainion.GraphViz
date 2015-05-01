using Plainion.GraphViz.Model;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Plainion.GraphViz.Infrastructure
{
    /// <summary>
    /// Raised when a node has been put into user focus.
    /// </summary>
    public class NodeFocusedEvent : PubSubEvent<Node>
    {
    }
}
