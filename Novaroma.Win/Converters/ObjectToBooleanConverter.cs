using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class ObjectToBooleanConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public ObjectToBooleanConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value != null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
