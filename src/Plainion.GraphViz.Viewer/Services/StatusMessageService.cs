using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Plainion.GraphViz.Infrastructure.Services;

namespace Plainion.GraphViz.Viewer.Services
{
    [Export( typeof( IStatusMessageService ) )]
    class StatusMessageService : IStatusMessageService
    {
        public StatusMessageService()
        {
            Messages = new ObservableCollection<StatusMessage>();
        }

        public void Publish( StatusMessage message )
        {
            Messages.Add( message );
        }

        public ObservableCollection<StatusMessage> Messages
        {
            get;
            private set;
        }
    }
}
