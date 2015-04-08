namespace Novaroma.Services.Kickass {

    public class KickassSettings : TorrentProviderSettingsBase {
        internal const string ImdbSearchQuery = "imdb:%imdbId%";

        public KickassSettings(): base("http://katproxy.com/") {
            MovieSearchPattern = ImdbSearchQuery;
        }
    }
}
