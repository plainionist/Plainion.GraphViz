﻿using Plainion.GraphViz.Model;
using Prism.Events;

namespace Plainion.GraphViz.Viewer.Abstractions
{
    /// <summary>
    /// Raised when a node has been put into user focus.
    /// </summary>
    public class NodeFocusedEvent : PubSubEvent<Node>
    {
    }
}
