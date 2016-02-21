using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Reflection.Analysis
{
    [Export( typeof( TypeDependencyGraphMenuItemModel ) )]
    public class TypeDependencyGraphMenuItemModel : BindableBase
    {
        public TypeDependencyGraphMenuItemModel()
        {
            OpenGraphBuilderCommand = new DelegateCommand( OnOpenGraphBuilder );
            GraphBuilderRequest = new InteractionRequest<INotification>();
        }

        private void OnOpenGraphBuilder()
        {
            var notification = new Notification();
            notification.Title = "Type Dependency Graph Builder";

            GraphBuilderRequest.Raise( notification, c => { } );
        }

        public ICommand OpenGraphBuilderCommand { get; private set; }

        public InteractionRequest<INotification> GraphBuilderRequest { get; private set; }
    }
}
