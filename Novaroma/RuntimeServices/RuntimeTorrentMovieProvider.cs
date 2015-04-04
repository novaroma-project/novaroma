using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Download.Torrent;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.RuntimeServices {

    public class RuntimeTorrentMovieProvider: RuntimeServiceBase<ITorrentMovieProvider>, ITorrentMovieProvider {
        public override string DefaultCode {
            get { return DEFAULT_CODE; }
        }

        #region ITorrentMovieProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                                                   string extraKeywords = null, string excludeKeywords = null, ITorrentDownloader service = null) {
            return Instance.SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords, service);
        }

        #endregion

        #region ITorrentProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> Search(string search, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null, ITorrentDownloader service = null) {
            return Instance.Search(search, videoQuality, excludeKeywords, service);
        }

        #endregion

        public const string DEFAULT_CODE =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Download.Torrent;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.MyServices {

    public class MyTorrentMovieProvider : ITorrentMovieProvider {

        #region ITorrentMovieProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                                                   string extraKeywords = null, string excludeKeywords = null, ITorrentDownloader service = null) {
            throw new NotImplementedException();
        }

        #endregion

        #region ITorrentProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> Search(string search, VideoQuality videoQuality = VideoQuality.Any, 
                                                              string excludeKeywords = null, ITorrentDownloader service = null) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyTorrentMovieProvider""; }
        }

        #endregion
    }
}";
    }
}
