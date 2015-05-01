using System;
using System.ComponentModel;

namespace Plainion.GraphViz.Presentation
{
    public abstract class AbstractPropertySet : INotifyPropertyChanged
    {
        protected AbstractPropertySet( string ownerId )
        {
            OwnerId = ownerId;
        }

        public string OwnerId
        {
            get;
            private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>( ref T member, T value, string propertyName )
        {
            if( object.ReferenceEquals( member, default( T ) ) && object.ReferenceEquals( value, default( T ) ) )
            {
                return;
            }

            if( !object.ReferenceEquals( member, default( T ) ) && member.Equals( value ) )
            {
                return;
            }

            member = value;

            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
    }
}
