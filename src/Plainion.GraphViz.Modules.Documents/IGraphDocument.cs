using System.Collections.Generic;
using Plainion.GraphViz.Viewer.Abstractions;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Modules.Documents
{
    public interface IGraphDocument : IDocument
    {
        IGraph Graph { get; }
    
        /// <summary>
        /// Allows a document to read its content more tolerant and report failures additionally but without exceptions.
        /// </summary>
        IEnumerable<FailedItem> FailedItems { get; }
    }
}
