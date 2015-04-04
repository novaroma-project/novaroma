namespace Novaroma.Services.ThePirateBay {

    public class ThePirateBaySettings : TorrentProviderSettingsBase {
        internal const string ImdbSearchQuery = "%imdbId%";

        public ThePirateBaySettings(): base("https://thepiratebay.se/") {
            MovieSearchPattern = ImdbSearchQuery;
        }
    }
}
