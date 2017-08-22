using System.ComponentModel.Composition;
using Plainion.GraphViz.Presentation;
using Prism.Mvvm;

namespace Plainion.GraphViz.Infrastructure.ViewModel
{
    [PartCreationPolicy( CreationPolicy.Shared )]
    [Export( typeof( IDomainModel ) )]
    public class DefaultDomainModel : BindableBase, IDomainModel
    {
        private IGraphPresentation myPresentation;
        private ILayoutEngine myLayoutEngine;

        public IGraphPresentation Presentation
        {
            get { return myPresentation; }
            set { SetProperty( ref myPresentation, value ); }
        }

        public ILayoutEngine LayoutEngine
        {
            get { return myLayoutEngine; }
            set { SetProperty( ref myLayoutEngine, value ); }
        }
    }
}
