using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Novaroma.Win.Converters {

    public class ListToStringConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public ListToStringConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var list = value as IEnumerable<object>;
            if (list == null) return null;
            if (parameter == null) parameter = ", ";
            return string.Join(parameter.ToString(), list.Select(x => x.ToString()));
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
