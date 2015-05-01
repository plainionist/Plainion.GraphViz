using System.ComponentModel;
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
                myModel.PropertyChanged += OnModelPropertyChanged;

                OnModelPropertyChanged( "Model" );

                // to simplify usage by derived classes notify about the implicit changes here as well
                foreach( var prop in Model.GetType().GetProperties() )
                {
                    OnModelPropertyChanged( prop.Name );
                }
            }
        }

        private void OnModelPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            OnModelPropertyChanged( e.PropertyName );
        }

        protected abstract void OnModelPropertyChanged( string propertyName );
    }
}
