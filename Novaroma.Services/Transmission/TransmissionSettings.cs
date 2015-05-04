using System.Collections.Generic;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.Services.Transmission {

    public class TransmissionSettings : TorrentDownloaderSettingsBase {

        public TransmissionSettings(IEnumerable<ITorrentMovieProvider> movieProviders, IEnumerable<ITorrentTvShowProvider> tvShowProviders)
            : base(movieProviders, tvShowProviders) {
            UserName = null;
            Password = null;
            Port = 9091;
        }
    }
}
