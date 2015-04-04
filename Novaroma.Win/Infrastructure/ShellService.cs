using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Interface.Model;
using Novaroma.Model;
using Novaroma.Win.ViewModels;

namespace Novaroma.Win.Infrastructure {

    public class ShellService : IShellService {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IDialogService _dialogService;
        private readonly MainViewModel _mainViewModel;

        public ShellService(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, MainViewModel mainViewModel) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _dialogService = dialogService;
            _mainViewModel = mainViewModel;
        }

        #region IShellService Members

        public void Test() {
        }

        public Task<Language> GetSelectedLanguage() {
            return Task.FromResult(_engine.Language);
        }

        public Task ShowMainWindow() {
            return Task.Run(() => Helper.ShowMainWindow());
        }

        public Task HandleExeArgs(string[] args) {
            var tasks = new List<Task>();

            foreach (var argTmp in args) {
                var arg = argTmp.ToLowerInvariant();

                var imdbId = Helper.ParseArg(arg, "novaromaimdb:");
                if (!string.IsNullOrEmpty(imdbId))
                    tasks.Add(Helper.AddFromImdbId(_engine, _exceptionHandler, _dialogService, imdbId));
                else {
                    var searchQuery = Helper.ParseArg(arg, "novaroma:");
                    if (!string.IsNullOrEmpty(searchQuery))
                        Helper.AddFromSearch(_engine, _exceptionHandler, _dialogService, searchQuery);
                }
            }

            return Task.WhenAll(tasks.ToArray());
        }

        public Task<DirectoryWatchStatus> GetDirectoryWatchStatus(string directory) {
            return _engine.GetDirectoryWatchStatus(directory);
        }

        public Task WatchDirectory(string directory) {
            Helper.WatchDirectory(_engine, _exceptionHandler, _dialogService, directory);

            return Task.FromResult(true);
        }

        public Task StopWatching(string directory) {
            return _engine.StopWatching(directory);
        }

        public Task AddMedia(string[] directories) {
            return Helper.AddFromDirectories(_engine, _exceptionHandler, _dialogService, directories);
        }

        public Task NewMedia(string parentDirectory = null) {
            Helper.NewMedia(_engine, _exceptionHandler, _dialogService, string.Empty, parentDirectory);

            return Task.FromResult(true);
        }

        public Task DiscoverMedia(string parentDirectory = null) {
            Helper.DiscoverMedia(_engine, _exceptionHandler, _dialogService, parentDirectory);

            return Task.FromResult(true);
        }

        public Task<Media> GetMedia(string directory) {
            return _engine.GetMedia(directory);
        }

        public Task<Movie> GetMovie(string directory) {
            return _engine.GetMovie(directory);
        }

        public Task<TvShow> GetTvShow(string directory) {
            return _engine.GetTvShow(directory);
        }

        public Task<IDownloadable> GetDownloadable(string filePath) {
            return _engine.GetDownloadable(filePath);
        }

        public Task<Movie> GetMovieByFile(string filePath) {
            return _engine.GetMovieByFile(filePath);
        }

        public Task<TvShowEpisode> GetTvShowEpisode(string filePath) {
            return _engine.GetTvShowEpisode(filePath);
        }

        public Task UpdateMovieWatchStatus(string directory, bool isWatched) {
            return _engine.GetMovie(directory)
                .ContinueWith(t => {
                    var movie = t.Result;
                    if (movie != null) {
                        movie.IsWatched = isWatched;
                        _engine.UpdateEntity(movie);
                    }
                });
        }

        public Task UpdateDownloadableWatchStatus(string path, bool isWatched) {
            return _engine.GetDownloadable(path)
                .ContinueWith(t => {
                    t.Result.IsWatched = isWatched;
                    _engine.UpdateEntity(t.Result.Media);
                });
        }

        public async Task DownloadMovie(string directory) {
            var movie = await _engine.GetMovie(directory);
            if (movie != null) {
                await Helper.ManualDownload(_engine, _exceptionHandler, _dialogService, movie);
                if (movie.IsModified)
                    await _engine.UpdateEntity(movie);
            }
        }

        public async Task DownloadSubtitle(string filePath) {
            var downloadable = await _engine.GetDownloadable(filePath);
            if (downloadable != null) {
                var downloaded = false;

                if (_engine.SubtitlesEnabled) {
                    var movie = downloadable as Movie;
                    if (movie != null)
                        downloaded = await _engine.DownloadSubtitleForMovie(movie);
                    else {
                        var episode = downloadable as TvShowEpisode;
                        if (episode != null)
                            downloaded = await _engine.DownloadSubtitleForTvShowEpisode(episode);
                    }
                }

                if (!downloaded) {
                    await Helper.ManualSubtitleDownload(_engine, _exceptionHandler, _dialogService, downloadable);
                    if (downloadable.Media.IsModified)
                        await _engine.UpdateEntity(downloadable.Media);
                }
            }
            else {
                var downloaded = await _engine.DownloadSubtitle(filePath);
                if (downloaded) return;

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                    await Helper.ManualSubtitleDownload(_engine, _exceptionHandler, _dialogService, fileInfo);
            }
        }

        public async Task EditMedia(string directory) {
            var media = await _engine.GetMedia(directory);
            if (media == null) return;

            var movie = media as Movie;
            if (movie != null)
                _mainViewModel.SelectedMovie = movie;
            else {
                var tvShow = media as TvShow;
                if (tvShow != null)
                    _mainViewModel.SelectedTvShow = tvShow;
            }

            await ShowMainWindow();
        }

        #endregion
    }
}
