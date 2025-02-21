using System.Windows.Input;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree
{
    class CallTreeMenuItemModel : BindableBase
    {
        public CallTreeMenuItemModel()
        {
            OpenCallTreeCommand = new DelegateCommand(OnOpenCallTree);
            CallTreeRequest = new InteractionRequest<INotification>();
        }

        private void OnOpenCallTree()
        {
            var notification = new Notification();
            notification.Title = "CallTree";

            CallTreeRequest.Raise(notification, c => { });
        }

        public ICommand OpenCallTreeCommand { get; private set; }

        public InteractionRequest<INotification> CallTreeRequest { get; private set; }
    }
}
