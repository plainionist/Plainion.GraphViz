using System.ComponentModel.Composition;
using System.Windows.Input;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    [Export(typeof(PackagingGraphMenuItemModel))]
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
