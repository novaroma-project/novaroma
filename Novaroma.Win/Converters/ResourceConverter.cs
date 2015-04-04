using System;
using System.Globalization;
using Novaroma.Properties;

namespace Novaroma.Win.Converters {

    public class ResourceConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public ResourceConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var resourceName = (string)value;
            return Resources.ResourceManager.GetString(resourceName);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
