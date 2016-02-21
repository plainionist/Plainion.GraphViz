
namespace Plainion.GraphViz.Presentation
{
    public abstract class AbstractStyle : AbstractPropertySet
    {
        private string myStyle;

        public AbstractStyle( string ownerId )
            : base( ownerId )
        {
        }

        // e.g. "solid"
        public string Style
        {
            get { return myStyle; }
            set { SetProperty( ref myStyle, value ); }
        }
    }
}
