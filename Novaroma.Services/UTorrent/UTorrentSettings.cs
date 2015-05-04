using System.Collections.Generic;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.Services.UTorrent {

    public class UTorrentSettings : TorrentDownloaderSettingsBase {

        public UTorrentSettings(IEnumerable<ITorrentMovieProvider> movieProviders, IEnumerable<ITorrentTvShowProvider> tvShowProviders)
            : base(movieProviders, tvShowProviders) {
        }
    }
}
