using System.Windows.Input;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    class PackagingGraphMenuItemModel : BindableBase
    {
        public PackagingGraphMenuItemModel()
        {
            OpenPackagingGraphBuilderCommand = new DelegateCommand(OnOpenPackagingGraphBuilder);
            PackagingGraphBuilderRequest = new InteractionRequest<INotification>();
        }

        private void OnOpenPackagingGraphBuilder()
        {
            var notification = new Notification();
            notification.Title = "Packaging Graph Builder";

            PackagingGraphBuilderRequest.Raise(notification, c => { });
        }

        public ICommand OpenPackagingGraphBuilderCommand { get; private set; }

        public InteractionRequest<INotification> PackagingGraphBuilderRequest { get; private set; }
    }
}
