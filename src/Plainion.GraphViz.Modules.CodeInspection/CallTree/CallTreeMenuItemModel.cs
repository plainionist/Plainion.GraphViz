using System.ComponentModel.Composition;
using System.Windows.Input;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree
{
    [Export]
    public class CallTreeMenuItemModel : BindableBase
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
