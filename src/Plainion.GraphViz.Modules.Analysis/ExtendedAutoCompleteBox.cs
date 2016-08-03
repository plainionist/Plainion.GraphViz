using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plainion.GraphViz.Modules.Analysis
{
    public class ExtendedAutoCompleteBox : AutoCompleteBox
    {
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Key.Enter)
            {
                if (InputCommittedCommand != null)
                {
                    if (SelectedItem == null)
                    {
                        // user did not manually select one of the preview items so just select
                        // the first one (if preview is not empty)
                        if (ItemFilter != null)
                        {
                            SelectedItem = ItemsSource.OfType<object>().FirstOrDefault(item => ItemFilter(SearchText, item));
                        }
                    }

                    if (InputCommittedCommand.CanExecute(SelectedItem))
                    {
                        InputCommittedCommand.Execute(SelectedItem);
                    }
                }
            }
        }

        public static DependencyProperty InputCommittedCommandProperty = DependencyProperty.Register("InputCommittedCommand", typeof(ICommand), typeof(ExtendedAutoCompleteBox));

        public ICommand InputCommittedCommand
        {
            get { return (ICommand)GetValue(InputCommittedCommandProperty); }
            set { SetValue(InputCommittedCommandProperty, value); }
        }
    }
}
