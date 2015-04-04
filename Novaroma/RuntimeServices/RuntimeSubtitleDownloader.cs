using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Subtitle;

namespace Novaroma.RuntimeServices {

    public class RuntimeSubtitleDownloader: RuntimeServiceBase<ISubtitleDownloader>, ISubtitleDownloader {

        public override string DefaultCode {
            get { return DEFAULT_CODE; }
        }

        #region ISubtitleDownloader Members

        public Task<IEnumerable<ISubtitleSearchResult>> SearchForMovie(string name, string videoFilePath, Language[] languages, string imdbId = null) {
            return Instance.SearchForMovie(name, videoFilePath, languages, imdbId);
        }

        public Task<bool> DownloadForMovie(string name, string videoFilePath, Language[] languages, string imdbId = null) {
            return Instance.DownloadForMovie(name, videoFilePath, languages, imdbId);
        }

        public Task<IEnumerable<ISubtitleSearchResult>> SearchForTvShowEpisode(string name, int season, int episode, string videoFilePath, Language[] languages, string imdbId = null) {
            return Instance.SearchForTvShowEpisode(name, season, episode, videoFilePath, languages, imdbId);
        }

        public Task<bool> DownloadForTvShowEpisode(string name, int season, int episode, string videoFilePath, Language[] languages, string imdbId = null) {
            return Instance.DownloadForTvShowEpisode(name, season, episode, videoFilePath, languages, imdbId);
        }

        public Task<IEnumerable<ISubtitleSearchResult>> Search(string query, Language[] languages, string imdbId = null) {
            return Instance.Search(query, languages, imdbId);
        }

        public Task<bool> Download(string videoFilePath, ISubtitleSearchResult searchResult) {
            return Instance.Download(videoFilePath, searchResult);
        }

        public Task<bool> Download(string videoFilePath, Language[] languages, string imdbId = null) {
            return Instance.Download(videoFilePath, languages, imdbId);
        }

        #endregion

        public const string DEFAULT_CODE =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Subtitle;

namespace Novaroma.MyServices {

    public class MySubtitleDownloader : ISubtitleDownloader {

        #region ISubtitleDownloader Members

        public Task<IEnumerable<ISubtitleSearchResult>> SearchForMovie(string name, string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<bool> DownloadForMovie(string name, string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ISubtitleSearchResult>> SearchForTvShowEpisode(string name, int season, int episode, string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<bool> DownloadForTvShowEpisode(string name, int season, int episode, string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ISubtitleSearchResult>> Search(string query, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<bool> Download(string videoFilePath, ISubtitleSearchResult searchResult) {
            throw new NotImplementedException();
        }

        public Task<bool> Download(string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MySubtitleDownloader""; }
        }

        #endregion
    }
}";
    }
}
