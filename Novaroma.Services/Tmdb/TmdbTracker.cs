using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Interface.Track;
using Novaroma.Properties;
using TMDbLib.Client;
using TMDbLib.Objects.Find;
using TMDbLib.Objects.TvShows;

namespace Novaroma.Services.Tmdb {

    public class TmdbTracker : IShowTracker, IImdbIdConverter {
        private const string ApiKey = "c23af065c02503d98c54719f37958015";

        public static Task<TvShowUpdate> GetUpdate(int id, DateTime? lastUpdate = null, Language language = Language.English) {
            return Task.Run(() => {
                var client = new TMDbClient(ApiKey);

                var episodes = new List<TvShowEpisodeInfo>();
                var updateDate = DateTime.Now;
                var show = client.GetTvShow(id, language: GetLanguage(language));
                if (show == null) return null;

                foreach (var seasonTmp in show.Seasons) {
                    if (seasonTmp.SeasonNumber == 0) continue;

                    var season = client.GetTvSeason(show.Id, seasonTmp.SeasonNumber);
                    if (season == null) continue;

                    foreach (var episode in season.Episodes) {
                        var ad = episode.AirDate;
                        DateTime? airDate;
                        if (ad.Year < 1896) airDate = null;
                        else
                            airDate = new DateTime(ad.Year, ad.Month, ad.Day, 20, 0, 0, DateTimeKind.Utc);
                        episodes.Add(new TvShowEpisodeInfo(season.SeasonNumber, episode.EpisodeNumber, episode.Name, airDate, episode.Overview));
                    }
                }

                return new TvShowUpdate(updateDate, show.InProduction, show.Status, episodes);
            });
        }

        public Task<ExternalIds> GetExternalIds(string imdbId) {
            return Task.Run(() => {
                var client = new TMDbClient(ApiKey);
                var tvShow = client.Find(FindExternalSource.Imdb, imdbId).TvResults.FirstOrDefault();
                return tvShow != null ? client.GetTvShowExternalIds(tvShow.Id) : null;
            });
        }

        private static string GetLanguage(Language language) {
            return Helper.GetTwoLetterLanguageCode(language);
        }

        #region IShowTracker Members

        public async Task<ITvShowUpdate> GetTvShowUpdate(string id, DateTime? lastUpdate = null, Language language = Language.English) {
            return await GetUpdate(Convert.ToInt32(id), lastUpdate, language);
        }

        public async Task<ITvShowUpdate> GetTvShowUpdateByImdbId(string imdbId, DateTime? lastUpdate = null, Language language = Language.English) {
            var ids = await GetExternalIds(imdbId);
            if (ids == null)
                throw new NovaromaException(Resources.ImdbIdNotFound);

            var id = ids.Id;
            return await GetUpdate(id, lastUpdate, language);
        }

        #endregion

        #region ITvShowImdbIdConverter Members

        public async Task<IDictionary<string, object>> GetServiceIds(string imdbId) {
            var externalIds = await GetExternalIds(imdbId);

            if (externalIds != null) {
                var serviceIds = new Dictionary<string, object>();

                serviceIds.Add(ServiceNames.Freebase, externalIds.FreebaseId);
                serviceIds.Add(ServiceNames.Tmdb, externalIds.Id);
                serviceIds.Add(ServiceNames.Tvdb, externalIds.TvdbId);
                serviceIds.Add(ServiceNames.TvRage, externalIds.TvrageId);

                return serviceIds;
            }

            return new Dictionary<string, object>();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ServiceNames.Tmdb; }
        }

        #endregion
    }
}
