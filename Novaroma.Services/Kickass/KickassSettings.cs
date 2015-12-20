namespace Novaroma.Services.Kickass {

    public class KickassSettings : TorrentProviderSettingsBase {
        internal const string ImdbSearchQuery = "imdb:%imdbId%";

        public KickassSettings(): base("https://kat.cr/") {
            MovieSearchPattern = ImdbSearchQuery;
        }
    }
}
