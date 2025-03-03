using System;
using Plainion.Graphs;
using Plainion.Graphs.Algorithms;
using Plainion.Graphs.Projections;

namespace Plainion.GraphViz.Presentation;

public interface IGraphPresentation : IModuleRepository, IDisposable, IGraphProjections, ICaptionProvider
{
    new IGraph Graph { get;  set; }

    void InvalidateLayout();

    event EventHandler GraphVisibilityChanged;

    IGraphPresentation UnionWith(IGraphPresentation other, Func<IGraphPresentation> presentationCreator);
}
