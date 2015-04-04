using System;
using System.Globalization;
using System.Windows;

namespace Novaroma.Win.Converters {

    public class BooleanToBoldFontWeightConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public BooleanToBoldFontWeightConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var boolValue = (bool?)value;
            return boolValue == true ? FontWeights.Bold : FontWeights.Normal;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
