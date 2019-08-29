using System;
using System.ComponentModel.Composition;
using Plainion.Prism.Mvvm;

namespace Plainion.GraphViz.Infrastructure.ViewModel
{
    public abstract class ViewModelBase : ValidatableBindableBase
    {
        private IDomainModel myModel;

        [Import]
        public IDomainModel Model
        {
            get { return myModel; }
            set
            {
                myModel = value;
                myModel.PresentationChanged += OnPresentationChanged;

                OnPresentationChanged();
            }
        }

        private void OnPresentationChanged(object sender, EventArgs e)
        {
            OnPresentationChanged();
        }

        protected virtual void OnPresentationChanged() { }
    }
}
