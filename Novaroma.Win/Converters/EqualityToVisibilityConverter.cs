using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class EqualityToVisibilityConverter : BaseConverter {
   
        // ReSharper disable once EmptyConstructor
        public EqualityToVisibilityConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Equals(value, parameter) ? Visibility.Visible : Visibility.Hidden;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
