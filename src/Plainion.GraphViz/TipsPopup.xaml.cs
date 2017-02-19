using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Plainion.GraphViz
{
    public partial class TipsPopup : UserControl
    {
        public TipsPopup()
        {
            InitializeComponent();
        }

        public IEnumerable Bullets
        {
            get { return ( IEnumerable )GetValue( BulletsProperty ); }
            set { SetValue( BulletsProperty, value ); }
        }

        public static readonly DependencyProperty BulletsProperty = DependencyProperty.Register( "Bullets", typeof( IEnumerable ), typeof( TipsPopup ) );

        private void OnTipsClick( object sender, RoutedEventArgs e )
        {
            var tooltip = ( ( ToolTip )( ( Hyperlink )sender ).ToolTip );
            tooltip.IsOpen = !tooltip.IsOpen;

            //HelpClient.OpenPage( "/" );
        }
    }
}
