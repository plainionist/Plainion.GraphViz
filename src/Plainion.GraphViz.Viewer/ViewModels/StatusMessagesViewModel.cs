using Plainion.GraphViz.Viewer.Abstractions.Services;
using Prism.Mvvm;

namespace Plainion.GraphViz.Viewer.ViewModels
{
    class StatusMessagesViewModel : BindableBase
    {
        public StatusMessagesViewModel(IStatusMessageService statusMessageService)
        {
            StatusMessageService = statusMessageService;
        }

        public IStatusMessageService StatusMessageService { get; private set; }
    }
}
