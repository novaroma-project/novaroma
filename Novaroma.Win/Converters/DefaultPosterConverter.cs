using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class DefaultPosterConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public DefaultPosterConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null || string.IsNullOrEmpty(value.ToString()) ? "/Resources/Images/Img_DefaultPoster_150x220.png" : value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
