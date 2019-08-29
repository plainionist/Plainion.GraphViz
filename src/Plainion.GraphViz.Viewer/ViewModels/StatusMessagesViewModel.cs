using System.ComponentModel.Composition;
using Plainion.GraphViz.Infrastructure.Services;
using Prism.Mvvm;

namespace Plainion.GraphViz.Viewer.ViewModels
{
    [Export(typeof(StatusMessagesViewModel))]
    class StatusMessagesViewModel : BindableBase
    {
        [ImportingConstructor]
        public StatusMessagesViewModel(IStatusMessageService statusMessageService)
        {
            StatusMessageService = statusMessageService;
        }

        public IStatusMessageService StatusMessageService { get; private set; }
    }
}
