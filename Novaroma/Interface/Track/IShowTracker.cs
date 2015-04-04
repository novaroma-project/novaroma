using System;
using System.Threading.Tasks;

namespace Novaroma.Interface.Track {
    
    public interface IShowTracker: INovaromaService {
        Task<ITvShowUpdate> GetTvShowUpdate(string id, DateTime? lastUpdate = null, Language language = Language.English);
        Task<ITvShowUpdate> GetTvShowUpdateByImdbId(string imdbId, DateTime? lastUpdate = null, Language language = Language.English);
    }
}
