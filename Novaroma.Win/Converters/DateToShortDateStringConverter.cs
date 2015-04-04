using System;
using System.Globalization;

namespace Novaroma.Win.Converters {

    public class DateToShortDateStringConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public DateToShortDateStringConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var dt = value as DateTime?;
            if (dt == null) return string.Empty;

            return dt.Value.ToShortDateString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
