using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    //https://zamjad.wordpress.com/2014/03/01/radio-button-in-mvvm/
    [ValueConversion( typeof( bool ), typeof( Enum ) )]
    class EnumToBoolExtension : MarkupExtension, IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return parameter.Equals( value );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return ( ( bool )value ) == true ? parameter : DependencyProperty.UnsetValue;
        }

        public override object ProvideValue( IServiceProvider serviceProvider )
        {
            return this;
        }
    }
}
