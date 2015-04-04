using System;
using System.Globalization;

namespace Novaroma.Win.Converters {

    public class ObjectToBooleanReverseConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public ObjectToBooleanReverseConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
