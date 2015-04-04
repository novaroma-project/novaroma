using System;
using System.Globalization;
using System.Windows;
using Novaroma.Interface;

namespace Novaroma.Win.Converters {

    public class ConfigurableToVisibilityConverter: BaseConverter {

        public ConfigurableToVisibilityConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (value as IConfigurable) != null ? Visibility.Visible : Visibility.Hidden;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
