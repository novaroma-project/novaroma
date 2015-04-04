using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class StringToVisibilityConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public StringToVisibilityConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value==null || string.IsNullOrWhiteSpace(value.ToString()) ? Visibility.Collapsed : Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
