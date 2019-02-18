using System.ComponentModel;
using System.Runtime.Serialization;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public interface INodeMask : INotifyPropertyChanged, ISerializable
    {
        string Label { get; }

        /// <summary>
        /// Indicates whether the mask defines the nodes to show or to hide
        /// Default: true
        /// </summary>
        bool IsShowMask { get; set; }

        /// <summary>
        /// Defines whether mask is considered when rendering the scene or not.
        /// this approach avoid having two models (all masks, rendered masks) in sync.
        /// Default: true
        /// </summary>
        bool IsApplied { get; set; }

        bool? IsSet(Node node);

        void Invert(IGraphPresentation presentation);
    }
}
