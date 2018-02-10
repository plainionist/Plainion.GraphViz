using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Plainion.GraphViz
{
    public partial class HelpLink : UserControl
    {
        public HelpLink()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PageProperty = DependencyProperty.Register( "Page", typeof( string ), typeof( HelpLink ) );

        public string Page
        {
            get { return ( string )GetValue( PageProperty ); }
            set { SetValue( PageProperty, value ); }
        }

        private void OnTipsClick( object sender, RoutedEventArgs e )
        {
            Process.Start("https://plainionist.github.io/Plainion.GraphViz/");
        }
    }
}
