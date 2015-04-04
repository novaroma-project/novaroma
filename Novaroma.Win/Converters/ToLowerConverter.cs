using System;
using System.Globalization;
using System.Threading;

namespace Novaroma.Win.Converters {

    public class ToLowerConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public ToLowerConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? string.Empty : str.ToLower(Thread.CurrentThread.CurrentCulture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
