using System;
using System.Globalization;

namespace Novaroma.Win.Converters {

    public class EnumToEnumInfoConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public EnumToEnumInfoConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Novaroma.Helper.GetEnumInfo(value.GetType());
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
