using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novaroma.Interface.Download.Torrent.Provider {

    public interface ITorrentMovieProvider : ITorrentProvider {
        Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
                                                            string extraKeywords = null, string excludeKeywords = null, int? minSize = null, int? maxSize = null, 
                                                            ITorrentDownloader service = null);
    }
}
