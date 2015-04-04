using System;
using System.Globalization;

namespace Novaroma.Win.Converters {

    public class ToShortDateConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public ToShortDateConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is DateTime)) return null;

            return ((DateTime)value).ToShortDateString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
