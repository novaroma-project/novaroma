using System;
using System.Globalization;
using System.Threading;

namespace Novaroma.Win.Converters {

    public class ToUpperConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public ToUpperConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? string.Empty : str.ToUpper(Thread.CurrentThread.CurrentCulture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
