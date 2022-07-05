namespace Plainion.GraphViz.Presentation
{
    public class Caption : AbstractPropertySet
    {
        private string myDisplayText;

        public Caption( string ownerId, string label )
            : base( ownerId )
        {
            Label = string.IsNullOrEmpty( label ) ? ownerId : label;
            DisplayText = Label;
        }

        public string Label { get; private set; }

        public string DisplayText
        {
            get { return myDisplayText; }
            set { SetProperty( ref myDisplayText, value ); }
        }
    }
}
