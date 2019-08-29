using System.ComponentModel.Composition;
using Plainion.GraphViz.Infrastructure.Services;
using Prism.Mvvm;

namespace Plainion.GraphViz.Viewer.ViewModels
{
    [Export( typeof( StatusMessagesViewModel ) )]
    class StatusMessagesViewModel : BindableBase
    {
        [Import]
        public IStatusMessageService StatusMessageService { get; set; }
    }
}
