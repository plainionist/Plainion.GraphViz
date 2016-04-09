using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Plainion.GraphViz.Presentation
{
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>( ref T storage, T value, [CallerMemberName] string propertyName = null )
        {
            if( object.Equals( storage, value ) )
            {
                return false;
            }

            storage = value;

            OnPropertyChanged( propertyName );

            return true;
        }

        protected virtual void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
    }
}
