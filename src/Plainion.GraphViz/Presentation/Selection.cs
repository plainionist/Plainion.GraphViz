using System.ComponentModel;

namespace Plainion.GraphViz.Presentation
{
    public class Selection : AbstractPropertySet
    {
        private bool myIsSelected;

        public Selection( string ownerId )
            : base( ownerId )
        {
            IsSelected = false;
        }

        public bool IsSelected
        {
            get { return myIsSelected; }
            set { SetProperty( ref myIsSelected, value, "IsSelected" ); }
        }
    }
}
