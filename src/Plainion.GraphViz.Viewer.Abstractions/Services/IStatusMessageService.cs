using System.Collections.ObjectModel;

namespace Plainion.GraphViz.Viewer.Abstractions.Services
{
    public interface IStatusMessageService
    {
        void Publish( StatusMessage message );
        ObservableCollection<StatusMessage> Messages { get; }
    }
}
