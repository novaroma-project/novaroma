using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.Interface.Download.Torrent {

    public interface ITorrentDownloader: IDownloader {
        Task<string> Download(string path, ITorrentSearchResult searchResult);
        IEnumerable<ITorrentMovieProvider> MovieProviders { get; }
        IEnumerable<ITorrentTvShowProvider> TvShowProviders { get; }
    }
}
