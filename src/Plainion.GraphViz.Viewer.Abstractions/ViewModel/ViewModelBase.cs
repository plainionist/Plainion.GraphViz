using System;
using Plainion.Prism.Mvvm;

namespace Plainion.GraphViz.Viewer.Abstractions.ViewModel
{
    public abstract class ViewModelBase : ValidatableBindableBase
    {
        protected ViewModelBase(IDomainModel model)
        {
            Contract.RequiresNotNull(model, nameof(model));

            Model = model;
            Model.PresentationChanged += OnPresentationChanged;
        }

        public IDomainModel Model { get; private set; }

        private void OnPresentationChanged(object sender, EventArgs e)
        {
            OnPresentationChanged();
        }

        protected virtual void OnPresentationChanged() { }
    }
}
