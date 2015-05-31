using System;
using System.ComponentModel.DataAnnotations;
using Novaroma.Properties;

namespace Novaroma {

    public enum Language {
        [Display(Name = "English", ResourceType = typeof(Resources))]
        [LanguageInfo("en", "eng", true)]
        English,
        [Display(Name = "Turkish", ResourceType = typeof(Resources))]
        [LanguageInfo("tr", "tur", true)]
        Turkish,
        [Display(Name = "Afrikaans", ResourceType = typeof(Resources))]
        [LanguageInfo("af", "afr")]
        Afrikaans,
        [Display(Name = "Albanian", ResourceType = typeof(Resources))]
        [LanguageInfo("sq", "alb")]
        Albanian,
        [Display(Name = "Arabic", ResourceType = typeof(Resources))]
        [LanguageInfo("ar", "ara")]
        Arabic,
        [Display(Name = "Armenian", ResourceType = typeof(Resources))]
        [LanguageInfo("hy", "arm")]
        Armenian,
        [Display(Name = "Basque", ResourceType = typeof(Resources))]
        [LanguageInfo("eu", "baq")]
        Basque,
        [Display(Name = "Belarusian", ResourceType = typeof(Resources))]
        [LanguageInfo("be", "bel")]
        Belarusian,
        [Display(Name = "Bengali", ResourceType = typeof(Resources))]
        [LanguageInfo("bn", "ben")]
        Bengali,
        [Display(Name = "Bosnian", ResourceType = typeof(Resources))]
        [LanguageInfo("bs", "bos")]
        Bosnian,
        [Display(Name = "Breton", ResourceType = typeof(Resources))]
        [LanguageInfo("br", "bre")]
        Breton,
        [Display(Name = "Bulgarian", ResourceType = typeof(Resources))]
        [LanguageInfo("bg", "bul")]
        Bulgarian,
        [Display(Name = "Burmese", ResourceType = typeof(Resources))]
        [LanguageInfo("my", "bur")]
        Burmese,
        [Display(Name = "Catalan", ResourceType = typeof(Resources))]
        [LanguageInfo("ca", "cat")]
        Catalan,
        [Display(Name = "ChineseBilingual", ResourceType = typeof(Resources))]
        [LanguageInfo("ze", "zhe")]
        ChineseBilingual,
        [Display(Name = "ChineseSimplified", ResourceType = typeof(Resources))]
        [LanguageInfo("zh", "chi")]
        ChineseSimplified,
        [Display(Name = "ChineseTraditional", ResourceType = typeof(Resources))]
        [LanguageInfo("zt", "zht")]
        ChineseTraditional,
        [Display(Name = "Croatian", ResourceType = typeof(Resources))]
        [LanguageInfo("hr", "hrv")]
        Croatian,
        [Display(Name = "Czech", ResourceType = typeof(Resources))]
        [LanguageInfo("cs", "cze")]
        Czech,
        [Display(Name = "Danish", ResourceType = typeof(Resources))]
        [LanguageInfo("da", "dan")]
        Danish,
        [Display(Name = "Dutch", ResourceType = typeof(Resources))]
        [LanguageInfo("nl", "dut")]
        Dutch,
        [Display(Name = "Esperanto", ResourceType = typeof(Resources))]
        [LanguageInfo("eo", "epo")]
        Esperanto,
        [Display(Name = "Estonian", ResourceType = typeof(Resources))]
        [LanguageInfo("et", "est")]
        Estonian,
        [Display(Name = "Finnish", ResourceType = typeof(Resources))]
        [LanguageInfo("fi", "fin")]
        Finnish,
        [Display(Name = "French", ResourceType = typeof(Resources))]
        [LanguageInfo("fr", "fre")]
        French,
        [Display(Name = "Galician", ResourceType = typeof(Resources))]
        [LanguageInfo("gl", "glg")]
        Galician,
        [Display(Name = "Georgian", ResourceType = typeof(Resources))]
        [LanguageInfo("ka", "geo")]
        Georgian,
        [Display(Name = "German", ResourceType = typeof(Resources))]
        [LanguageInfo("de", "ger")]
        German,
        [Display(Name = "Greek", ResourceType = typeof(Resources))]
        [LanguageInfo("el", "ell")]
        Greek,
        [Display(Name = "Hebrew", ResourceType = typeof(Resources))]
        [LanguageInfo("he", "heb")]
        Hebrew,
        [Display(Name = "Hindi", ResourceType = typeof(Resources))]
        [LanguageInfo("hi", "hin")]
        Hindi,
        [Display(Name = "Hungarian", ResourceType = typeof(Resources))]
        [LanguageInfo("hu", "hun")]
        Hungarian,
        [Display(Name = "Icelandic", ResourceType = typeof(Resources))]
        [LanguageInfo("is", "ice")]
        Icelandic,
        [Display(Name = "Indonesian", ResourceType = typeof(Resources))]
        [LanguageInfo("id", "ind")]
        Indonesian,
        [Display(Name = "Italian", ResourceType = typeof(Resources))]
        [LanguageInfo("it", "ita")]
        Italian,
        [Display(Name = "Japanese", ResourceType = typeof(Resources))]
        [LanguageInfo("ja", "jpn")]
        Japanese,
        [Display(Name = "Kazakh", ResourceType = typeof(Resources))]
        [LanguageInfo("kk", "kaz")]
        Kazakh,
        [Display(Name = "Khmer", ResourceType = typeof(Resources))]
        [LanguageInfo("km", "khm")]
        Khmer,
        [Display(Name = "Korean", ResourceType = typeof(Resources))]
        [LanguageInfo("ko", "kor")]
        Korean,
        [Display(Name = "Latvian", ResourceType = typeof(Resources))]
        [LanguageInfo("lv", "lav")]
        Latvian,
        [Display(Name = "Lithuanian", ResourceType = typeof(Resources))]
        [LanguageInfo("lt", "lit")]
        Lithuanian,
        [Display(Name = "Luxembourgish", ResourceType = typeof(Resources))]
        [LanguageInfo("lb", "ltz")]
        Luxembourgish,
        [Display(Name = "Macedonian", ResourceType = typeof(Resources))]
        [LanguageInfo("mk", "mac")]
        Macedonian,
        [Display(Name = "Malay", ResourceType = typeof(Resources))]
        [LanguageInfo("ms", "may")]
        Malay,
        [Display(Name = "Malayalam", ResourceType = typeof(Resources))]
        [LanguageInfo("ml", "mal")]
        Malayalam,
        [Display(Name = "Manipuri", ResourceType = typeof(Resources))]
        [LanguageInfo("ma", "mni")]
        Manipuri,
        [Display(Name = "Mongolian", ResourceType = typeof(Resources))]
        [LanguageInfo("mn", "mon")]
        Mongolian,
        [Display(Name = "Montenegrin", ResourceType = typeof(Resources))]
        [LanguageInfo("me", "mne")]
        Montenegrin,
        [Display(Name = "Norwegian", ResourceType = typeof(Resources))]
        [LanguageInfo("no", "nor")]
        Norwegian,
        [Display(Name = "Occitan", ResourceType = typeof(Resources))]
        [LanguageInfo("oc", "oci")]
        Occitan,
        [Display(Name = "Persian", ResourceType = typeof(Resources))]
        [LanguageInfo("fa", "per")]
        Persian,
        [Display(Name = "Polish", ResourceType = typeof(Resources))]
        [LanguageInfo("pl", "pol")]
        Polish,
        [Display(Name = "Portuguese", ResourceType = typeof(Resources))]
        [LanguageInfo("pt", "por")]
        Portuguese,
        [Display(Name = "PortugueseBrazilian", ResourceType = typeof(Resources))]
        [LanguageInfo("pb", "pob")]
        PortugueseBrazilian,
        [Display(Name = "Romanian", ResourceType = typeof(Resources))]
        [LanguageInfo("ro", "rum")]
        Romanian,
        [Display(Name = "Russian", ResourceType = typeof(Resources))]
        [LanguageInfo("ru", "rus")]
        Russian,
        [Display(Name = "Serbian", ResourceType = typeof(Resources))]
        [LanguageInfo("sr", "scc")]
        Serbian,
        [Display(Name = "Sinhalese", ResourceType = typeof(Resources))]
        [LanguageInfo("si", "sin")]
        Sinhalese,
        [Display(Name = "Slovak", ResourceType = typeof(Resources))]
        [LanguageInfo("sk", "slo")]
        Slovak,
        [Display(Name = "Slovenian", ResourceType = typeof(Resources))]
        [LanguageInfo("sl", "slv")]
        Slovenian,
        [Display(Name = "Spanish", ResourceType = typeof(Resources))]
        [LanguageInfo("es", "spa")]
        Spanish,
        [Display(Name = "Swahili", ResourceType = typeof(Resources))]
        [LanguageInfo("sw", "swa")]
        Swahili,
        [Display(Name = "Swedish", ResourceType = typeof(Resources))]
        [LanguageInfo("sv", "swe")]
        Swedish,
        [Display(Name = "Syriac", ResourceType = typeof(Resources))]
        [LanguageInfo("sy", "syr")]
        Syriac,
        [Display(Name = "Tagalog", ResourceType = typeof(Resources))]
        [LanguageInfo("tl", "tgl")]
        Tagalog,
        [Display(Name = "Tamil", ResourceType = typeof(Resources))]
        [LanguageInfo("ta", "tam")]
        Tamil,
        [Display(Name = "Telugu", ResourceType = typeof(Resources))]
        [LanguageInfo("te", "tel")]
        Telugu,
        [Display(Name = "Thai", ResourceType = typeof(Resources))]
        [LanguageInfo("th", "tha")]
        Thai,
        [Display(Name = "Ukrainian", ResourceType = typeof(Resources))]
        [LanguageInfo("uk", "ukr")]
        Ukrainian,
        [Display(Name = "Urdu", ResourceType = typeof(Resources))]
        [LanguageInfo("ur", "urd")]
        Urdu,
        [Display(Name = "Vietnamese", ResourceType = typeof(Resources))]
        [LanguageInfo("vi", "vie")]
        Vietnamese,
    }

    public class LanguageInfoAttribute : Attribute {
        private readonly string _twoLetterCode;
        private readonly string _threeLetterCode;
        private readonly bool _uiSupported;

        public LanguageInfoAttribute(string twoLetterCode, string threeLetterCode, bool uiSupported = false) {
            _twoLetterCode = twoLetterCode;
            _threeLetterCode = threeLetterCode;
            _uiSupported = uiSupported;
        }

        public string TwoLetterCode {
            get { return _twoLetterCode; }
        }

        public string ThreeLetterCode {
            get { return _threeLetterCode; }
        }

        public bool UISupported {
            get { return _uiSupported; }
        }
    }
}
