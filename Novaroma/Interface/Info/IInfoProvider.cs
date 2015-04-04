using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novaroma.Interface.Info {

    public interface IInfoProvider : INovaromaService {
        Task<IEnumerable<IInfoSearchResult>> Search(string query, Language language = Language.English);
        Task<IMovieInfo> GetMovie(IInfoSearchResult searchResult, Language language = Language.English);
        Task<IMovieInfo> GetMovie(string id, Language language = Language.English);
        Task<ITvShowInfo> GetTvShow(IInfoSearchResult searchResult, Language language = Language.English);
        Task<ITvShowInfo> GetTvShow(string id, Language language = Language.English);
    }
}
