using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Novaroma.Interface.Download.Torrent.Provider;
using Novaroma.Properties;

namespace Novaroma.Interface.Download.Torrent {

    public abstract class TorrentDownloaderBase : ITorrentDownloader {
        protected readonly IExceptionHandler ExceptionHandler;

        protected TorrentDownloaderBase(IExceptionHandler exceptionHandler) {
            ExceptionHandler = exceptionHandler;
        }

        public virtual async Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year, string imdbId, VideoQuality videoQuality = VideoQuality.Any,
                                                                                 string extraKeywords = null, string excludeKeywords = null) {
            var results = new List<ITorrentSearchResult>();
            var tasks = MovieProviders.RunTasks(p => 
                p.SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords, this)
                    .ContinueWith(t => results.AddRange(t.Result)),
                ExceptionHandler
            );

            await Task.WhenAll(tasks);
            return results.OrderByDescending(r => r.Seed);
        }

        public virtual async Task<IEnumerable<ITorrentSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId,
                                                                                         VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null) {
            var results = new List<ITorrentSearchResult>();
            var tasks = TvShowProviders.RunTasks(p => 
                p.SearchTvShowEpisode(name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords, this)
                    .ContinueWith(t => results.AddRange(t.Result)),
                ExceptionHandler
            );

            await Task.WhenAll(tasks);
            var episodeStr = episode.ToString(CultureInfo.InvariantCulture);
            return results.OrderByDescending(r => r.Seed).Where(r => r.Name.Contains(episodeStr));
        }

        public virtual async Task<IEnumerable<ITorrentSearchResult>> Search(string query, VideoQuality videoQuality,
                                                                            string excludeKeywords) {
            var providers = ((IEnumerable<ITorrentProvider>)MovieProviders).Union(TvShowProviders);

            var results = new List<ITorrentSearchResult>();
            var tasks = providers.RunTasks(p => 
                p.Search(query, videoQuality, excludeKeywords, this)
                    .ContinueWith(t => results.AddRange(t.Result)),
                ExceptionHandler
            );

            await Task.WhenAll(tasks);
            return results.OrderByDescending(r => r.Seed);
        }

        protected abstract IEnumerable<ITorrentMovieProvider> MovieProviders { get; }

        protected abstract IEnumerable<ITorrentTvShowProvider> TvShowProviders { get; }

        protected abstract string ServiceName { get; }

        protected virtual void OnDownloadCompleted(DownloadCompletedEventArgs args) {
            var handler = DownloadCompleted;
            if (handler != null) handler(this, args);
        }

        public override string ToString() {
            return ServiceName;
        }

        #region ITorrentDownloader Members

        public abstract Task<string> Download(string path, ITorrentSearchResult searchResult);

        IEnumerable<ITorrentMovieProvider> ITorrentDownloader.MovieProviders {
            get { return MovieProviders; }
        }

        IEnumerable<ITorrentTvShowProvider> ITorrentDownloader.TvShowProviders {
            get { return TvShowProviders; }
        }

        #endregion

        #region IDownloader Members

        async Task<IEnumerable<IDownloadSearchResult>> IDownloader.SearchMovie(string name, int? year, string imdbId, VideoQuality videoQuality, string extraKeywords, string excludeKeywords) {
            return await SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords);
        }

        async Task<string> IDownloader.DownloadMovie(string path, string name, int? year, string imdbId, VideoQuality videoQuality, string extraKeywords, string excludeKeywords) {
            var resultList = await SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords);

            var results = resultList.OrderByDescending(r => r.Seed);
            var result = results.FirstOrDefault();

            if (result != null)
                return await Download(path, result);

            return string.Empty;
        }

        async Task<IEnumerable<IDownloadSearchResult>> IDownloader.SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId, VideoQuality videoQuality, string extraKeywords, string excludeKeywords) {
            return await SearchTvShowEpisode(name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords);
        }

        async Task<string> IDownloader.DownloadTvShowEpisode(string path, string name, int season, int episode, string episodeName, string imdbId, VideoQuality videoQuality, string extraKeywords, string excludeKeywords) {
            var resultList = await SearchTvShowEpisode(name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords);

            var results = resultList.OrderByDescending(r => r.Seed);
            var result = results.FirstOrDefault();

            if (result != null)
                return await Download(path, result);

            return string.Empty;
        }

        async Task<IEnumerable<IDownloadSearchResult>> IDownloader.Search(string query, VideoQuality videoQuality, string excludeKeywords) {
            return await Search(query, videoQuality, excludeKeywords);
        }

        Task<string> IDownloader.Download(string path, IDownloadSearchResult searchResult) {
            var torrentSearchResult = searchResult as ITorrentSearchResult;
            if (torrentSearchResult == null)
                throw new NovaromaException(Resources.UnsupportedSearchResult);

            return Download(path, torrentSearchResult);
        }

        public abstract Task Refresh();

        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

        #endregion

        #region INovaromaService Members

        string INovaromaService.ServiceName {
            get { return ServiceName; }
        }

        #endregion
    }
}
