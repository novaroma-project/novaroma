using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class BooleanToVisibilityConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public BooleanToVisibilityConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var boolValue = (bool?)value;

            if (boolValue == null) return null;

            return boolValue == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
