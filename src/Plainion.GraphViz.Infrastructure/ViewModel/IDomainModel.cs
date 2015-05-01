using System.ComponentModel;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Infrastructure.ViewModel
{
    /// <summary>
    /// Acts as major model and contains all relevant sub-models for the currently visualized graph
    /// </summary>
    public interface IDomainModel : INotifyPropertyChanged
    {
        IGraphPresentation Presentation { get; set; }

        ILayoutEngine LayoutEngine { get; set; }
    }
}
