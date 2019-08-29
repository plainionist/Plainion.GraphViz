using System;
using System.ComponentModel.Composition;
using Plainion.GraphViz.Presentation;
using Prism.Mvvm;

namespace Plainion.GraphViz.Infrastructure.ViewModel
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDomainModel))]
    public class DefaultDomainModel : IDomainModel
    {
        private IGraphPresentation myPresentation;

        public IGraphPresentation Presentation
        {
            get { return myPresentation; }
            set
            {
                if (myPresentation != value)
                {
                    myPresentation = value;
                    PresentationChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public event EventHandler PresentationChanged;
    }
}
