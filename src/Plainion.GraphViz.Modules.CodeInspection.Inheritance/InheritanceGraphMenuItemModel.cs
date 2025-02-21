using System.Windows.Input;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance
{
    class InheritanceGraphMenuItemModel : BindableBase
    {
        public InheritanceGraphMenuItemModel()
        {
            OpenInheritanceGraphBuilderCommand = new DelegateCommand( OnOpenInheritanceGraphBuilder );
            InheritanceGraphBuilderRequest = new InteractionRequest<INotification>();
        }

        private void OnOpenInheritanceGraphBuilder()
        {
            var notification = new Notification();
            notification.Title = "Inheritance Graph Builder";

            InheritanceGraphBuilderRequest.Raise( notification, c => { } );
        }

        public ICommand OpenInheritanceGraphBuilderCommand
        {
            get;
            private set;
        }

        public InteractionRequest<INotification> InheritanceGraphBuilderRequest { get; private set; }
    }
}
