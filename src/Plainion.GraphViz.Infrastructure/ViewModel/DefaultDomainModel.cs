using System;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Infrastructure.ViewModel
{
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
