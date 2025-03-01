using System;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation;

public interface IGraphPresentation : IModuleRepository, IDisposable
{
    IGraph Graph { get; set; }

    IGraphPicking Picking { get; }

    void InvalidateLayout();

    event EventHandler GraphVisibilityChanged;

    IGraphPresentation UnionWith(IGraphPresentation other, Func<IGraphPresentation> presentationCreator);
}
