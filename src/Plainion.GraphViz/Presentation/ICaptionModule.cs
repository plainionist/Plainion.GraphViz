
using Plainion.Graphs.Projections;

namespace Plainion.GraphViz.Presentation
{
    public interface ICaptionModule : IPropertySetModule<Caption>
    {
        ILabelConverter LabelConverter { get; set; }
    }
}
