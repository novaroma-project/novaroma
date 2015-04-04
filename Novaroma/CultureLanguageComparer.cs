using System;
using System.Collections.Generic;
using System.Globalization;

namespace Novaroma {

    public class CultureLanguageComparer : IComparer<Language> {

        public int Compare(Language x, Language y) {
            if (x == y) return 0;

            var langCode1 = Helper.GetTwoLetterLanguageCode(x);
            var langCode2 = Helper.GetTwoLetterLanguageCode(y);
            var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (string.Equals(langCode1, culture, StringComparison.OrdinalIgnoreCase))
                return -1;
            if (string.Equals(langCode2, culture, StringComparison.OrdinalIgnoreCase))
                return 1;

            if (string.Equals(langCode1, "en", StringComparison.OrdinalIgnoreCase))
                return -1;
            if (string.Equals(langCode2, "en", StringComparison.OrdinalIgnoreCase))
                return 1;

            return 0;
        }
    }
}
