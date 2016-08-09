using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                                                                                 string extraKeywords = null, string excludeKeywords = null,
                                                                                 int? minSize = null, int? maxSize = null, int? minSeed = null) {
            var results = new ConcurrentBag<ITorrentSearchResult>();
            var tasks = MovieProviders.RunTasks(p =>
                p.SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords, minSize, maxSize, minSeed, this)
                    .ContinueWith(t => results.AddRange(t.Result)),
                ExceptionHandler
            );

            await Task.WhenAll(tasks);
            return results.OrderByDescending(r => r.Seed);
        }

        public virtual async Task<IEnumerable<ITorrentSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId,
                                                                                         VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null,
                                                                                         int? minSize = null, int? maxSize = null, int? minSeed = null) {
            var results = new ConcurrentBag<ITorrentSearchResult>();
            var tasks = TvShowProviders.RunTasks(p =>
                p.SearchTvShowEpisode(name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords, minSize, maxSize, minSeed, this)
                    .ContinueWith(t => results.AddRange(t.Result)),
                ExceptionHandler
            );

            await Task.WhenAll(tasks);
            return results
                .Where(r => {
                    int? s, e;
                    Helper.DetectEpisodeInfo(r.Name, name, out s, out e);
                    return (s == null || s == season) && e == episode;
                })
                .OrderByDescending(r => r.Seed)
                .ToList();
        }

        public virtual async Task<IEnumerable<ITorrentSearchResult>> Search(string query, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null,
                                                                            int? minSize = null, int? maxSize = null, int? minSeed = null) {
            var providers = ((IEnumerable<ITorrentProvider>)MovieProviders).Union(TvShowProviders);

            var results = new ConcurrentBag<ITorrentSearchResult>();
            var tasks = providers.RunTasks(p =>
                p.Search(query, videoQuality, excludeKeywords, minSize, maxSize, minSeed, this)
                 .ContinueWith(t => results.AddRange(t.Result)),
                ExceptionHandler
            );

            await Task.WhenAll(tasks);
            return results.OrderByDescending(r => r.Seed);
        }

        public abstract bool IsAvailable { get; }

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

        async Task<IEnumerable<IDownloadSearchResult>> IDownloader.SearchMovie(string name, int? year, string imdbId, VideoQuality videoQuality, string extraKeywords,
                                                                               string excludeKeywords, int? minSize, int? maxSize, int? minSeed) {
            return await SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords, minSize, maxSize, minSeed);
        }

        async Task<string> IDownloader.DownloadMovie(string path, string name, int? year, string imdbId, VideoQuality videoQuality, string extraKeywords,
                                                     string excludeKeywords, int? minSize, int? maxSize, int? minSeed) {
            var resultList = await SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords, minSize, maxSize, minSeed);

            var results = resultList.OrderByDescending(r => r.Seed);
            var result = results.FirstOrDefault();

            if (result != null)
                return await Download(path, result);

            return string.Empty;
        }

        async Task<IEnumerable<IDownloadSearchResult>> IDownloader.SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId, VideoQuality videoQuality,
                                                                                       string extraKeywords, string excludeKeywords, int? minSize, int? maxSize, int? minSeed) {
            return await SearchTvShowEpisode(name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords, minSize, maxSize, minSeed);
        }

        async Task<string> IDownloader.DownloadTvShowEpisode(string path, string name, int season, int episode, string episodeName, string imdbId, VideoQuality videoQuality,
                                                             string extraKeywords, string excludeKeywords, int? minSize, int? maxSize, int? minSeed) {
            var resultList = await SearchTvShowEpisode(name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords, minSize, maxSize, minSeed);

            var results = resultList.OrderByDescending(r => r.Seed);
            var result = results.FirstOrDefault();

            if (result != null)
                return await Download(path, result);

            return string.Empty;
        }

        async Task<IEnumerable<IDownloadSearchResult>> IDownloader.Search(string query, VideoQuality videoQuality, string excludeKeywords, int? minSize, int? maxSize, int? minSeed) {
            return await Search(query, videoQuality, excludeKeywords, minSize, maxSize, minSeed);
        }

        Task<string> IDownloader.Download(string path, IDownloadSearchResult searchResult) {
            var torrentSearchResult = searchResult as ITorrentSearchResult;
            if (torrentSearchResult == null)
                throw new NovaromaException(Resources.UnsupportedSearchResult);

            return Download(path, torrentSearchResult);
        }

        public abstract Task Refresh(bool downloadOnly);

        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

        #endregion

        #region INovaromaService Members

        string INovaromaService.ServiceName {
            get { return ServiceName; }
        }

        #endregion
    }
}
