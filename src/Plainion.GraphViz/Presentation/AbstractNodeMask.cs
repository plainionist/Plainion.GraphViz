using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    [Serializable]
    public abstract class AbstractNodeMask : INodeMask
    {
        private bool myIsApplied;
        private bool myIsShowMask;
        private string myLabel;

        protected AbstractNodeMask()
        {
        }

        protected AbstractNodeMask( SerializationInfo info, StreamingContext context )
        {
            myLabel = ( string )info.GetValue( "Label", typeof( string ) );
            myIsApplied = ( bool )info.GetValue( "IsApplied", typeof( bool ) );
            myIsShowMask = ( bool )info.GetValue( "IsShowMask", typeof( bool ) );
        }

        public virtual void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "Label", myLabel );
            info.AddValue( "IsApplied", myIsApplied );
            info.AddValue( "IsShowMask", myIsShowMask );
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

            RaisePropertyChanged( propertyName );
        }

        protected void RaisePropertyChanged( string propertyName )
        {
            IsDirty = true;

            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        public string Label
        {
            get { return myLabel; }
            set { SetProperty( ref myLabel, value, "Label" ); }
        }

        public bool IsShowMask
        {
            get { return myIsShowMask; }
            set { SetProperty( ref myIsShowMask, value, "IsShowMask" ); }
        }

        public bool IsApplied
        {
            get { return myIsApplied; }
            set { SetProperty( ref myIsApplied, value, "IsApplied" ); }
        }

        public abstract bool? IsSet( Node node );

        public bool IsDirty
        {
            get;
            protected set;
        }

        public void MarkAsClean()
        {
            IsDirty = false;
        }
    }
}
