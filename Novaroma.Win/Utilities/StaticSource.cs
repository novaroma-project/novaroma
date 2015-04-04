using System;
using System.Collections.Generic;

namespace Novaroma.Win.Utilities {

    public class StaticSource {
        private static readonly Lazy<StaticSource> _instance = new Lazy<StaticSource>();

        public static StaticSource Instance {
            get { return _instance.Value; }
        }

        public IEnumerable<EnumInfo<Language>> LanguagesEnumInfo {
            get { return Constants.LanguagesEnumInfo; }
        }

        public IEnumerable<EnumInfo<VideoQuality>> VideoQualityEnumInfo {
            get { return Constants.VideoQualityEnumInfo; }
        }
    }
}
