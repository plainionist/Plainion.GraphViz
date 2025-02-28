using System;
using System.Windows.Input;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Bookmarks;

class OpenBookmarksViewModel : BindableBase
{
    private readonly IDomainModel myModel;

    public OpenBookmarksViewModel(IDomainModel model)
    {
        myModel = model;

        myModel.PresentationChanged += OnPresentationChanged;

        OpenBookmarksCommand = new DelegateCommand(OnOpenBookmarks, () => myModel.Presentation != null);
        BookmarksRequest = new InteractionRequest<INotification>();
    }

    public DelegateCommand OpenBookmarksCommand { get; private set; }

    private void OnOpenBookmarks()
    {
        var notification = new Notification();
        notification.Title = "Bookmarks";

        BookmarksRequest.Raise(notification, c => { });
    }

    public InteractionRequest<INotification> BookmarksRequest { get; private set; }

    private void OnPresentationChanged(object sender, EventArgs e)
    {
        OpenBookmarksCommand.RaiseCanExecuteChanged();
    }
}
