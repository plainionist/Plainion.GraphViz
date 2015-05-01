using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plainion.GraphViz.Modules.Analysis
{
    public class ExtendedAutoCompleteBox : AutoCompleteBox
    {
        public static DependencyProperty InputCommittedCommandProperty = DependencyProperty.Register( "InputCommittedCommand", typeof( ICommand ), typeof( ExtendedAutoCompleteBox ) );
        public static DependencyProperty InputCommittedCommandParameterProperty = DependencyProperty.Register( "InputCommittedCommandParameter", typeof( object ), typeof( ExtendedAutoCompleteBox ) );

        protected override void OnKeyUp( KeyEventArgs e )
        {
            base.OnKeyUp( e );

            if( e.Key == Key.Enter )
            {
                if( InputCommittedCommand != null )
                {
                    var parameter = InputCommittedCommandParameter != null ? InputCommittedCommandParameter : SelectedItem;
                    if( InputCommittedCommand.CanExecute( parameter ) )
                    {
                        InputCommittedCommand.Execute( parameter );
                    }
                }
            }
        }

        public ICommand InputCommittedCommand
        {
            get
            {
                return ( ICommand )GetValue( InputCommittedCommandProperty );
            }

            set
            {
                SetValue( InputCommittedCommandProperty, value );
            }
        }

        public object InputCommittedCommandParameter
        {
            get
            {
                return GetValue( InputCommittedCommandParameterProperty );
            }

            set
            {
                SetValue( InputCommittedCommandParameterProperty, value );
            }
        }

    }
}
