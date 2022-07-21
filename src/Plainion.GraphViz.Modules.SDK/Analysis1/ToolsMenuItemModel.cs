using System.Windows.Input;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.SDK.Analysis1
{
    class ToolsMenuItemModel : BindableBase
    {
        public ToolsMenuItemModel()
        {
            StartAnalysisCommand = new DelegateCommand(OnStartAnalysis);
            StartAnalysisRequest = new InteractionRequest<INotification>();
        }

        private void OnStartAnalysis()
        {
            var notification = new Notification();
            notification.Title = "Analysis SDK";

            StartAnalysisRequest.Raise(notification, c => { });
        }

        public ICommand StartAnalysisCommand { get; }

        public InteractionRequest<INotification> StartAnalysisRequest { get; private set; }
    }
}
