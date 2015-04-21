using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novaroma.Interface.Download.Torrent.Provider {

    public interface ITorrentTvShowProvider : ITorrentProvider {
        Task<IEnumerable<ITorrentSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId = null,
                                                                    VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null, 
                                                                    int? minSize = null, int? maxSize = null, ITorrentDownloader service = null);
    }
}
