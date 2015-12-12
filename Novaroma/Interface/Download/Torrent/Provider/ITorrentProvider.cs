using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novaroma.Interface.Download.Torrent.Provider {

    public interface ITorrentProvider: INovaromaService {
        Task<IEnumerable<ITorrentSearchResult>> Search(string search, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null, 
                                                       int? minSize = null, int? maxSize = null, int? minSeed = null, ITorrentDownloader service = null);
    }
}
