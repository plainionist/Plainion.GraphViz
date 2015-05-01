using System.ComponentModel.Composition;
using System.Windows;

namespace Plainion.GraphViz.Viewer
{
    [Export( typeof( Shell ) )]
    public partial class Shell : Window
    {
        [ImportingConstructor]
        public Shell( ShellViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}