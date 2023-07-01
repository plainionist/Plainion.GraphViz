using System.Windows.Input;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
{
    internal class ToolsMenuItemModel : BindableBase
    {
        public ToolsMenuItemModel()
        {
            StartAnalysisCommand = new DelegateCommand(OnStartAnalysis);
            StartAnalysisRequest = new InteractionRequest<INotification>();
        }

        private void OnStartAnalysis()
        {
            var notification = new Notification
            {
                Title = "Markdown Files Dependencies"
            };

            StartAnalysisRequest.Raise(notification, c => { });
        }

        public ICommand StartAnalysisCommand { get; }

        public InteractionRequest<INotification> StartAnalysisRequest { get; private set; }
    }
}