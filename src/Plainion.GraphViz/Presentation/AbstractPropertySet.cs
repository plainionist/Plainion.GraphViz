using Plainion.Graphs;

namespace Plainion.GraphViz.Presentation
{
    public abstract class AbstractPropertySet : NotifyPropertyChangedBase
    {
        protected AbstractPropertySet(string ownerId)
        {
            System.Contract.RequiresNotNull(ownerId);

            OwnerId = ownerId;
        }

        public string OwnerId { get; private set; }
    }
}
