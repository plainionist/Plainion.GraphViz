using System.ComponentModel.Composition;
using Plainion.GraphViz.Infrastructure.ViewModel;

namespace Plainion.GraphViz.Modules.Analysis
{
    [Export(typeof(BookmarksViewModel))]
    class BookmarksViewModel : ViewModelBase
    {
        [ImportingConstructor]
        public BookmarksViewModel()
        {
        }

        protected override void OnModelPropertyChanged(string propertyName)
        {
        }
    }
}
