using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novaroma.Interface.Info {

    public interface IAdvancedInfoProvider: IInfoProvider {
        IEnumerable<string> Genres { get; } 
        Task<IEnumerable<IAdvancedInfoSearchResult>> AdvancedSearch(
            string query, MediaTypes mediaTypes = MediaTypes.All, int? releaseYearStart = null, int? releaseYearEnd = null,
            float? ratingMin = null, float? ratingMax = null, int? voteCountMin = null, int? voteCountMax = null,
            int? runtimeMin = null, int? runtimeMax = null, IEnumerable<string> genres = null, Language language = Language.English);
        Task<IMovieInfo> GetMovie(IAdvancedInfoSearchResult searchResult, Language language = Language.English);
        Task<ITvShowInfo> GetTvShow(IAdvancedInfoSearchResult searchResult, Language language = Language.English);
    }
}
