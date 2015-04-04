using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class ObjectToVisibilityReverseConverter : ObjectToVisibilityConverter {

        // ReSharper disable once EmptyConstructor
        public ObjectToVisibilityReverseConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value != null ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
