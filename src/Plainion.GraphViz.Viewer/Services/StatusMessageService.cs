using System.Collections.ObjectModel;
using Plainion.GraphViz.Infrastructure.Services;

namespace Plainion.GraphViz.Viewer.Services
{
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
