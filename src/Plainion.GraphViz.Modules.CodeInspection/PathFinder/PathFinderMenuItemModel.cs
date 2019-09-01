using System.Windows.Input;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder
{
    class PathFinderMenuItemModel : BindableBase
    {
        public PathFinderMenuItemModel()
        {
            OpenPathFinderCommand = new DelegateCommand(OnOpenPathFinder);
            PathFinderRequest = new InteractionRequest<INotification>();
        }

        private void OnOpenPathFinder()
        {
            var notification = new Notification();
            notification.Title = "PathFinder";

            PathFinderRequest.Raise(notification, c => { });
        }

        public ICommand OpenPathFinderCommand { get; private set; }

        public InteractionRequest<INotification> PathFinderRequest { get; private set; }
    }
}
