using System;
using System.ComponentModel;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.Abstractions.ViewModel
{
    /// <summary>
    /// Acts as major model and contains all relevant sub-models for the currently visualized graph
    /// </summary>
    public interface IDomainModel 
    {
        IGraphPresentation Presentation { get; set; }
        event EventHandler PresentationChanged;
    }
}
