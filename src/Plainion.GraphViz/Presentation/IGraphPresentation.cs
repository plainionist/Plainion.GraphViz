using System;
using System.Collections.Generic;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public interface IGraphPresentation : IDisposable
    {
        T GetModule<T>();

        IPropertySetModule<T> GetPropertySetFor<T>() where T : AbstractPropertySet;

        IGraph Graph { get; set; }

        IGraphPicking Picking { get; }

        void InvalidateLayout();

        event EventHandler GraphVisibilityChanged;

        IGraphPresentation UnionWith( IGraphPresentation other, Func<IGraphPresentation> presentationCreator );
    }
}
