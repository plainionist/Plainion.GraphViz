using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Input;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Presentation;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export(typeof(BookmarksViewModel))]
    class BookmarksViewModel : ViewModelBase
    {
        private IGraphPresentation myPresentation;
        private Bookmark myBookmark;
        private string myCaption;

        [ImportingConstructor]
        public BookmarksViewModel()
        {
            Bookmarks = new ObservableCollection<Bookmark>();

            AddBookmarkCommand = new DelegateCommand(OnAddBookmark, () => !string.IsNullOrWhiteSpace(Caption));
            DeleteBookmarkCommand = new DelegateCommand(OnDeleteBookmark);
            ApplyBookmarkCommand = new DelegateCommand(OnApplyBookmark);
        }

        protected override void OnPresentationChanged()
        {
            if (myPresentation == Model.Presentation)
            {
                return;
            }

            myPresentation = Model.Presentation;

            Bookmarks.Clear();
        }

        public string Caption
        {
            get { return myCaption; }
            set
            {
                if (SetProperty(ref myCaption, value))
                {
                    AddBookmarkCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<Bookmark> Bookmarks { get; private set; }

        public Bookmark SelectedBookmark
        {
            get { return myBookmark; }
            set { SetProperty(ref myBookmark, value); }
        }

        public DelegateCommand AddBookmarkCommand { get; private set; }

        private void OnAddBookmark()
        {
            if (string.IsNullOrWhiteSpace(Caption))
            {
                return;
            }

            var builder = new BookmarkBuilder();
            var bookmark = builder.Create(myPresentation, Caption);
            Bookmarks.Add(bookmark);

            Caption = string.Empty;
        }

        public ICommand DeleteBookmarkCommand { get; private set; }

        private void OnDeleteBookmark()
        {
            if (SelectedBookmark == null)
            {
                return;
            }

            Bookmarks.Remove(SelectedBookmark);
        }

        public ICommand ApplyBookmarkCommand { get; private set; }

        private void OnApplyBookmark()
        {
            if (SelectedBookmark == null)
            {
                return;
            }

            var builder = new BookmarkBuilder();
            builder.Apply(myPresentation, SelectedBookmark);
        }
    }
}
