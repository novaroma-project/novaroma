using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class InverseBooleanToVisibilityConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public InverseBooleanToVisibilityConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var boolValue = (bool?)value;

            if (boolValue == null) return null;

            return boolValue == false ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
