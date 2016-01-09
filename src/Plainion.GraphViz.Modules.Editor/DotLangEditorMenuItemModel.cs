using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Editor
{
    [Export( typeof( DotLangEditorMenuItemModel ) )]
    public class DotLangEditorMenuItemModel : BindableBase
    {
        public DotLangEditorMenuItemModel()
        {
            OpenDotLangEditorCommand = new DelegateCommand( OnOpenDotLangEditor );
            DotLangEditorRequest = new InteractionRequest<INotification>();
        }

        private void OnOpenDotLangEditor()
        {
            var notification = new Notification();
            notification.Title = "DOT Language Editor";

            DotLangEditorRequest.Raise( notification, c => { } );
        }

        public ICommand OpenDotLangEditorCommand { get; private set; }

        public InteractionRequest<INotification> DotLangEditorRequest { get; private set; }
    }
}
