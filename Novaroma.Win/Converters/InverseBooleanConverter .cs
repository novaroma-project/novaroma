using System;
using System.Globalization;

namespace Novaroma.Win.Converters {

    public class InverseBooleanConverter  : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public InverseBooleanConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var boolValue = value as bool?;
            return boolValue == null ? null : !boolValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
