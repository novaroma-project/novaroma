using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class IntegerToVisibilityConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public IntegerToVisibilityConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var val = value as int?;

            if (parameter != null && parameter.ToString()=="N")
                return val <= 0 ? Visibility.Visible : Visibility.Collapsed;

            return val == null || val <= 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
