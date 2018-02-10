namespace Plainion.GraphViz.Presentation
{
    public class Bookmark
    {
        public Bookmark(string caption, byte[] state)
        {
            Contract.RequiresNotNullNotEmpty(caption, nameof(caption));
            Contract.RequiresNotNull(state, nameof(state));

            Caption = caption;
            State = state;
        }

        public string Caption { get; private set; }
        internal byte[] State { get; private set; }
    }
}