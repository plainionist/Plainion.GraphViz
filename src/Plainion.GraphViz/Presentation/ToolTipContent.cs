
namespace Plainion.GraphViz.Presentation
{
    public class ToolTipContent : AbstractPropertySet
    {
        private object myContent;

        public ToolTipContent( string ownerId, object content )
            : base( ownerId )
        {
            Content = content;
        }

        public object Content
        {
            get { return myContent; }
            set { SetProperty( ref myContent, value, "Content" ); }
        }
    }
}
