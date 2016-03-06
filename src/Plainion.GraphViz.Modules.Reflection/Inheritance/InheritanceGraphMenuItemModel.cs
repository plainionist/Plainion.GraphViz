using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Inheritance
{
    [Export( typeof( InheritanceGraphMenuItemModel ) )]
    public class InheritanceGraphMenuItemModel : BindableBase
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
