using System.Collections.ObjectModel;

namespace Plainion.GraphViz.Infrastructure.Services
{
    public interface IStatusMessageService
    {
        void Publish( StatusMessage message );
        ObservableCollection<StatusMessage> Messages { get; }
    }
}
