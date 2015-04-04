using System;
using System.Globalization;
using Novaroma.Model;

namespace Novaroma.Win.Converters {

    public class EntityToTypeNameConverter : BaseConverter {

        // ReSharper disable once EmptyConstructor
        public EntityToTypeNameConverter() {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;

            var tvShow = value as TvShow;
            if (tvShow != null) return "TvShow";

            var movie = value as Movie;
            if (movie != null) return "Movie";

            var media = value as Media;
            return media != null ? "Media" : null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
