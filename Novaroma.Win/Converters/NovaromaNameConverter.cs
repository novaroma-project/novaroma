using System;
using System.Globalization;

namespace Novaroma.Win.Converters {

    public class NovaromaNameConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public NovaromaNameConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.NovaromaName();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
