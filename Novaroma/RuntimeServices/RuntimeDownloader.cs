using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Download;

namespace Novaroma.RuntimeServices {

    public class RuntimeDownloader : RuntimeServiceBase<IDownloader>, IDownloader {

        public override string DefaultCode {
            get { return DEFAULT_CODE; }
        }

        #region IDownloader Members

        public Task<IEnumerable<IDownloadSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                                                    string extraKeywords = null, string excludeKeywords = null) {
            return Instance.SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords);
        }

        public Task<string> DownloadMovie(string path, string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                          string extraKeywords = null, string excludeKeywords = null) {
            return Instance.DownloadMovie(path, name, year, imdbId, videoQuality, extraKeywords, excludeKeywords);
        }

        public Task<IEnumerable<IDownloadSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName = null, 
                                                                            string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                                                            string extraKeywords = null, string excludeKeywords = null) {
            return Instance.SearchTvShowEpisode(name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords);
        }

        public Task<string> DownloadTvShowEpisode(string path, string name, int season, int episode, string episodeName = null, string imdbId = null, 
                                                  VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null) {
            return Instance.DownloadTvShowEpisode(path, name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords);
        }

        public Task<IEnumerable<IDownloadSearchResult>> Search(string query, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null) {
            return Instance.Search(query, videoQuality, excludeKeywords);
        }

        public Task<string> Download(string path, IDownloadSearchResult searchResult) {
            return Instance.Download(path, searchResult);
        }

        public Task Refresh() {
            return Instance.Refresh();
        }

        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted {
            add { Instance.DownloadCompleted += value; }
            remove { Instance.DownloadCompleted -= value; }
        }

        #endregion

        public const string DEFAULT_CODE =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Download;

namespace Novaroma.MyServices {

    public class MyDownloader : IDownloader {

        #region IDownloader Members

        public Task<IEnumerable<IDownloadSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                                                    string extraKeywords = null, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<string> DownloadMovie(string path, string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                          string extraKeywords = null, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDownloadSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName = null, string imdbId = null, 
                                                                            VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<string> DownloadTvShowEpisode(string path, string name, int season, int episode, string episodeName = null, string imdbId = null, 
                                                  VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDownloadSearchResult>> Search(string query, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<string> Download(string path, IDownloadSearchResult searchResult) {
            throw new NotImplementedException();
        }

        public Task Refresh() {
            throw new NotImplementedException();
        }

        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyDownloader""; }
        }

        #endregion
    }
}
";
    }
}
