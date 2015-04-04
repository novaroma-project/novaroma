using System;
using System.Globalization;
using System.IO;

namespace Novaroma.Win.Converters {
 
    public class PathToBoolConverter: BaseConverter {
        
        // ReSharper disable once EmptyConstructor
        public PathToBoolConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var path = (string) value;
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
