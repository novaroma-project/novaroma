namespace Novaroma.Services.Kickass {

    public class KickassSettings : TorrentProviderSettingsBase {
        internal const string ImdbSearchQuery = "imdb:%imdbId%";

        public KickassSettings(): base("http://kickasstorrentsan.com/") {
            MovieSearchPattern = ImdbSearchQuery;
        }
    }
}
