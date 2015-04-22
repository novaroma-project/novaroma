using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Model;
using Novaroma.Model.Search;
using Novaroma.Properties;
using Novaroma.Services.UTorrent;
using Novaroma.Win.Infrastructure;
using Novaroma.Win.Utilities;
using Novaroma.Win.Views;

namespace Novaroma.Win.ViewModels {

    public class MainViewModel : ViewModelBase, IDisposable {

        #region Fields

        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger _logger;
        private readonly IEnumerable<IPluginService> _pluginServices;
        private readonly Timer _updateCheckTimer;
        private readonly RelayCommand _installUpdateCommand;
        private readonly RelayCommand _aboutCommand;
        private readonly RelayCommand _newCommand;
        private readonly RelayCommand _watchDirectoryCommand;
        private readonly RelayCommand _discoverCommand;
        private readonly RelayCommand _manualDownloadCommand;
        private readonly RelayCommand _executePluginServiceCommand;
        private readonly RelayCommand _settingsCommand;
        private readonly RelayCommand _executeDownloadsCommand;
        private readonly RelayCommand _executeTvShowUpdatesCommand;
        private readonly RelayCommand _backupDatabaseCommand;
        private readonly RelayCommand _manageRuntimeServicesCommand;
        private readonly RelayCommand _clearLogsAndActivitiesCommand;
        private readonly MovieSearchModel _movieSearchModel;
        private readonly TvShowSearchModel _tvShowSearchModel;
        private readonly ActivitySearchModel _activitySearchModel;
        private readonly RelayCommand _saveSearchModelCommand;
        private readonly RelayCommand _movieSearchCommand;
        private readonly RelayCommand _tvShowSearchCommand;
        private readonly RelayCommand _clearMovieSearchModelCommand;
        private readonly RelayCommand _clearTvShowSearchModelCommand;
        private readonly RelayCommand _moviePlayCommand;
        private readonly RelayCommand _movieMarkWatchedCommand;
        private readonly RelayCommand _movieUpdateInfoCommand;
        private readonly RelayCommand _movieDeleteCommand;
        private readonly RelayCommand _movieSaveCommand;
        private readonly RelayCommand _movieDownloadCommand;
        private readonly RelayCommand _movieSubtitleDownloadCommand;
        private readonly RelayCommand _movieDiscoverFilesCommand;
        private readonly RelayCommand _movieGoToDirectoryCommand;
        private readonly RelayCommand _tvShowPlayCommand;
        private readonly RelayCommand _tvShowMarkEpisodeWatchedCommand;
        private readonly RelayCommand _tvShowEpisodePlayCommand;
        private readonly RelayCommand _tvShowEpisodeDeleteCommand;
        private readonly RelayCommand _tvShowUpdateInfoCommand;
        private readonly RelayCommand _tvShowDeleteCommand;
        private readonly RelayCommand _tvShowSaveCommand;
        private readonly RelayCommand _tvShowEpisodeDownloadCommand;
        private readonly RelayCommand _tvShowEpisodeSubtitleDownloadCommand;
        private readonly RelayCommand _tvShowDiscoverFilesCommand;
        private readonly RelayCommand _tvShowGoToDirectoryCommand;
        private readonly RelayCommand _activityPlayCommand;
        private readonly RelayCommand _sendASmileCommand;
        private readonly RelayCommand _sendAFrownCommand;
        private readonly RelayCommand _escapeKeyCommand;
        private readonly RelayCommand _allTvDownloadCheckCommand;
        private readonly RelayCommand _allSeasonDownloadCheckCommand;
        private readonly RelayCommand _episodeDownloadCheckCommand;
        private readonly RelayCommand _movieDownloadCheckCommand;
        private readonly IEnumerable<EnumInfo<VideoQuality>> _videoQualityEnumInfo;
        private bool _updateAvailable;
        private QueryResult<Movie> _movies;
        private QueryResult<TvShow> _tvShows;
        private QueryResult<Activity> _activities;
        private bool _isMoviesBusy;
        private bool _isTvShowsBusy;
        private int _notReadActivityCount;
        private bool _isActivitiesSelected;
        private Movie _focusedMovie;
        private Movie _selectedMovie;
        private TvShow _focusedTvShow;
        private TvShow _selectedTvShow;
        private int _movieMaxPage;
        private int _tvShowMaxPage;
        private bool _enableMoviePreviousButton;
        private bool _enableMovieNextButton;
        private bool _enableTvShowPreviousButton;
        private bool _enableTvShowNextButton;
        private bool _pinTvShowFlyout;

        #endregion

        public MainViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, ILogger logger)
            : base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _logger = logger;
            _pluginServices = engine.Services.OfType<IPluginService>();

            var genres = engine.MediaGenres;
            _movieSearchModel = new MovieSearchModel(genres);
            _tvShowSearchModel = new TvShowSearchModel(genres);
            _activitySearchModel = new ActivitySearchModel();

            _installUpdateCommand = new RelayCommand(InstallUpdate);
            _aboutCommand = new RelayCommand(About);
            _newCommand = new RelayCommand(NewMedia);
            _watchDirectoryCommand = new RelayCommand(WatchDirectory);
            _discoverCommand = new RelayCommand(Discover);
            _manualDownloadCommand = new RelayCommand(DoManualDownload);
            _executePluginServiceCommand = new RelayCommand(DoExecutePluginService);
            _settingsCommand = new RelayCommand(EditSettings);
            _executeDownloadsCommand = new RelayCommand(ExecuteDownloads);
            _executeTvShowUpdatesCommand = new RelayCommand(ExecuteTvShowUpdates);
            _backupDatabaseCommand = new RelayCommand(DoBackupDatabase);
            _manageRuntimeServicesCommand = new RelayCommand(ManageRuntimeServices);
            _clearLogsAndActivitiesCommand = new RelayCommand(DoClearLogsAndActivities);

            _saveSearchModelCommand = new RelayCommand(DoSaveSearchModel);
            _movieSearchCommand = new RelayCommand(DoGetMovies);
            _tvShowSearchCommand = new RelayCommand(DoGetTvShows);
            _clearMovieSearchModelCommand = new RelayCommand(DoClearMovieSearchModel);
            _clearTvShowSearchModelCommand = new RelayCommand(DoClearTvShowSearchModel);

            _moviePlayCommand = new RelayCommand(PlayMovie, CanPlayMovie);
            _movieMarkWatchedCommand = new RelayCommand(MarkMovieWatched, CanMarkMovieWatched);
            _movieUpdateInfoCommand = new RelayCommand(DoUpdateMovieInfo);
            _movieDeleteCommand = new RelayCommand(DoDeleteMovie);
            _movieSaveCommand = new RelayCommand(DoSaveMovie);
            _movieDownloadCommand = new RelayCommand(DoDownloadMovie);
            _movieSubtitleDownloadCommand = new RelayCommand(DoDownloadMovieSubtitle);
            _movieDiscoverFilesCommand = new RelayCommand(DiscoverMovieFiles);
            _movieGoToDirectoryCommand = new RelayCommand(GoToMovieDirectory);

            _tvShowPlayCommand = new RelayCommand(PlayTvShow, CanPlayTvShow);
            _tvShowMarkEpisodeWatchedCommand = new RelayCommand(MarkEpisodeWatched, CanMarkEpisodeWatched);
            _tvShowEpisodePlayCommand = new RelayCommand(PlayTvShowEpisode, CanPlayTvShowEpisode);
            _tvShowEpisodeDeleteCommand = new RelayCommand(DeleteTvShowEpisode, CanDeleteTvShowEpisode);
            _tvShowUpdateInfoCommand = new RelayCommand(DoUpdateTvShowInfo);
            _tvShowDeleteCommand = new RelayCommand(DoDeleteTvShow);
            _tvShowSaveCommand = new RelayCommand(DoSaveTvShow);
            _tvShowEpisodeDownloadCommand = new RelayCommand(DoDownloadTvShowEpisode);
            _tvShowEpisodeSubtitleDownloadCommand = new RelayCommand(DoDownloadTvShowEpisodeSubtitle);
            _tvShowDiscoverFilesCommand = new RelayCommand(DiscoverTvShowFiles);
            _tvShowGoToDirectoryCommand = new RelayCommand(GoToTvShowDirectory);
            _activityPlayCommand = new RelayCommand(PlayActivity);

            _sendASmileCommand = new RelayCommand(SendASmile);
            _sendAFrownCommand = new RelayCommand(SendAFrown);

            _escapeKeyCommand = new RelayCommand(PressEscapeKey);

            _allTvDownloadCheckCommand = new RelayCommand(o => Helper.AllTvDownloadCheck(o, _engine));
            _allSeasonDownloadCheckCommand = new RelayCommand(o => Helper.AllSeasonDownloadCheck(o, _engine));
            _episodeDownloadCheckCommand = new RelayCommand(o => Helper.EpisodeDownloadCheck(o, _engine));
            _movieDownloadCheckCommand = new RelayCommand(o => Helper.MovieDownloadCheck(o, _engine));

            _videoQualityEnumInfo = Constants.VideoQualityEnumInfo;

            _engine.MoviesChanged += EngineOnMoviesChanged;
            _engine.TvShowsChanged += EngineOnTvShowsChanged;
            _engine.ActivitiesChanged += EngineOnActivitiesChanged;
            _engine.LanguageChanged += EngineOnLanguageChanged;

            _updateCheckTimer = new Timer(o => DoCheckForUpdate(), null, TimeSpan.FromSeconds(5), TimeSpan.FromHours(1));
        }

        #region Methods

        #region Big Buttons

        private void NewMedia() {
            Helper.AddFromSearch(_engine, _exceptionHandler, DialogService);
        }

        private void WatchDirectory() {
            Helper.WatchDirectory(_engine, _exceptionHandler, DialogService);
        }

        private void Discover() {
            Helper.DiscoverMedia(_engine, _exceptionHandler, DialogService);
        }

        private async void DoManualDownload() {
            await ManualDownload();
        }

        private Task ManualDownload() {
            return Helper.ManualDownload(_engine, _exceptionHandler, DialogService);
        }

        private async void DoExecutePluginService(object prm) {
            var plugin = prm as IPluginService;
            if (plugin == null) return;

            await ExecutePluginService(plugin);
        }

        private async Task ExecutePluginService(IPluginService plugin) {
            try {
                await plugin.Activate();
            }
            catch (Exception ex) {
                _exceptionHandler.HandleException(ex);
            }
        }

        private void EditSettings() {
            Helper.EditSettings(_engine, DialogService, _engine);
        }

        private void ExecuteDownloads() {
            _engine.ExecuteDownloadJob();
            _engine.ExecuteSubtitleDownloadJob();
        }

        private void ExecuteTvShowUpdates() {
            _engine.ExecuteTvShowUpdateJob();
        }

        private async void DoBackupDatabase() {
            await BackupDatabase();
        }

        private async Task BackupDatabase() {
            var path = DialogService.SaveFileDialog(Resources.SelectAFile, Constants.Novaroma, ".db");
            if (string.IsNullOrEmpty(path)) return;

            await _engine.BackupDatabase(path);
        }

        private void ManageRuntimeServices() {
            new ScriptServicesWindow(_engine, DialogService).ShowDialog();
        }

        private async void DoClearLogsAndActivities() {
            await ClearLogsAndActivities();
        }

        private async Task ClearLogsAndActivities() {
            await _logger.Clear();
            await _engine.ClearActivities();
        }

        #endregion

        #region Fetch Data

        public async Task ListData() {
            await LoadSavedSearchModels();
            _movieSearchModel.RefreshNeeded += (sender, args) => DoGetMovies();
            _tvShowSearchModel.RefreshNeeded += (sender, args) => DoGetTvShows();

            await Task.WhenAll(GetMovies(), GetTvShows(), GetActivities());
        }

        private async void DoSaveSearchModel(object prm) {
            await SaveSearchModel((MediaSearchModel)prm);
        }

        private Task SaveSearchModel(MediaSearchModel searchModel) {
            var c = (IConfigurable)searchModel;
            return _engine.SaveSettings(c.SettingName, c.SerializeSettings());
        }

        private async void DoGetMovies() {
            await GetMovies();
        }

        public async Task GetMovies() {
            IsMoviesBusy = true;
            Movies = await _engine.GetMovies(_movieSearchModel);
            IsMoviesBusy = false;

            MovieMaxPage = GetMaxPageCount(Movies.InlineCount, MovieSearchModel.PageSize);
            EnableMovieNextButton = MovieSearchModel.Page < MovieMaxPage;
            EnableMoviePreviousButton = MovieSearchModel.Page > 1;
            RaisePropertyChanged("HasMovies");
        }

        private async void DoGetTvShows() {
            await GetTvShows();
        }

        private async Task GetTvShows() {
            IsTvShowsBusy = true;
            TvShows = await _engine.GetTvShows(_tvShowSearchModel);
            IsTvShowsBusy = false;

            TvShowMaxPage = GetMaxPageCount(TvShows.InlineCount, TvShowSearchModel.PageSize);
            EnableTvShowNextButton = TvShowSearchModel.Page < TvShowMaxPage;
            EnableTvShowPreviousButton = TvShowSearchModel.Page > 1;
            RaisePropertyChanged("HasTvShows");
        }

        private static int GetMaxPageCount(int count, int pageSize) {
            return (int)Math.Ceiling((double)count / pageSize);
        }

        private async void DoGetActivities() {
            await GetActivities();
        }

        private async Task GetActivities() {
            Activities = await _engine.GetActivities(_activitySearchModel);
            NotReadActivityCount = _activities.Results.Count(a => !a.IsRead);
        }

        private async void DoClearMovieSearchModel() {
            await ClearMovieSearchModel();
        }

        private async Task ClearMovieSearchModel() {
            ClearMediaSearchModel(_movieSearchModel);
            await GetMovies();
        }

        private async void DoClearTvShowSearchModel() {
            await ClearTvShowSearchModel();
        }

        private async Task ClearTvShowSearchModel() {
            ClearMediaSearchModel(_tvShowSearchModel);
            _tvShowSearchModel.Ended = null;
            await GetTvShows();
        }

        private static void ClearMediaSearchModel<T>(T mediaSearchModel) where T : MediaSearchModel {
            mediaSearchModel.Query = null;

            mediaSearchModel.ReleaseYearStart
            = mediaSearchModel.ReleaseYearEnd
            = mediaSearchModel.NumberOfVotesMin
            = mediaSearchModel.NumberOfVotesMax
            = mediaSearchModel.RuntimeMin
            = mediaSearchModel.RuntimeMax
            = null;

            mediaSearchModel.RatingMin
            = mediaSearchModel.RatingMax
            = null;

            mediaSearchModel.NotWatched
            = mediaSearchModel.Downloaded
            = mediaSearchModel.SubtitleDownloaded
            = mediaSearchModel.NotFound
            = mediaSearchModel.SubtitleNotFound
            = null;

            mediaSearchModel.Page = 1;
            mediaSearchModel.Genres.Selections.ToList().ForEach(si => si.IsSelected = false);
        }

        #endregion

        #region Movie

        private static void PlayMovie(object prm) {
            PlayMovie((Movie)prm);
        }

        private static void PlayMovie(Movie movie) {
            Process.Start(movie.FilePath);
        }

        private static bool CanPlayMovie(object prm) {
            return CanPlayMovie(prm as Movie);
        }

        private static bool CanPlayMovie(Movie movie) {
            return movie != null && !string.IsNullOrEmpty(movie.FilePath) && File.Exists(movie.FilePath);
        }

        private async void MarkMovieWatched(object prm) {
            await MarkMovieWatched((Movie)prm);
        }

        private async Task MarkMovieWatched(Movie movie) {
            movie.IsWatched = true;
            await _engine.UpdateEntity(movie);
        }

        private static bool CanMarkMovieWatched(object prm) {
            return CanMarkMovieWatched(prm as Movie);
        }

        private static bool CanMarkMovieWatched(Movie movie) {
            return movie != null && !movie.IsWatched;
        }

        private async void DoUpdateMovieInfo(object prm) {
            var movie = prm as Movie;
            if (movie == null) return;

            await UpdateMovieInfo(movie);
        }

        private async Task UpdateMovieInfo(Movie movie) {
            await Novaroma.Helper.RunTask(() => _engine.UpdateMediaInfo(movie), _exceptionHandler);

            if (SelectedMovie == movie) {
                SelectedMovie = null;
                SelectedMovie = movie;
            }
        }

        private async void DoDeleteMovie(object prm) {
            var movie = prm as Movie;
            if (movie == null) return;

            SelectedMovie = null;
            FocusedMovie = null;
            await DeleteMovie(movie);
        }

        private async Task DeleteMovie(Movie movie) {
            if (await Confirm(Resources.MontyNi, Resources.AreYouSure))
                await _engine.DeleteEntity(movie);
        }

        private async void DoSaveMovie(object prm) {
            var movie = prm as Movie;
            if (movie == null) return;

            if (movie.IsModified)
                await SaveMovie(SelectedMovie);
        }

        private async Task SaveMovie(Movie movie) {
            await _engine.UpdateEntity(movie);
        }

        private async void DoDownloadMovie(object prm) {
            var movie = prm as Movie;
            if (movie == null) return;

            await DownloadMovie(movie);
        }

        private async Task DownloadMovie(Movie movie) {
            await Helper.ManualDownload(_engine, _exceptionHandler, DialogService, movie);
        }

        private async void DoDownloadMovieSubtitle(object prm) {
            var movie = prm as Movie;
            if (movie == null) return;

            await DownloadMovieSubtitle(movie);
        }

        private async Task DownloadMovieSubtitle(Movie movie) {
            await Helper.ManualSubtitleDownload(_engine, _exceptionHandler, DialogService, movie);
        }

        private void DiscoverMovieFiles(object prm) {
            var movie = prm as Movie;
            if (movie == null) return;

            DiscoverMovieFiles(movie);
        }

        private void DiscoverMovieFiles(Movie movie) {
            Novaroma.Helper.InitMovie(movie, _engine);
        }

        private static void GoToMovieDirectory(object prm) {
            var movie = prm as Movie;
            if (movie == null) return;

            if (!string.IsNullOrEmpty(movie.Directory) && Directory.Exists(movie.Directory))
                Process.Start(movie.Directory);
        }

        #endregion

        #region Tv Show

        private static void PlayTvShow(object prm) {
            PlayTvShow((TvShow)prm);
        }

        private static void PlayTvShow(TvShow tvShow) {
            PlayTvShowEpisode(tvShow.UnseenEpisode);
        }

        private static bool CanPlayTvShow(object prm) {
            return CanPlayTvShow(prm as TvShow);
        }

        private static bool CanPlayTvShow(TvShow tvShow) {
            return tvShow != null && CanPlayTvShowEpisode(tvShow.UnseenEpisode);
        }

        private async void MarkEpisodeWatched(object prm) {
            await MarkEpisodeWatched(prm as TvShow);
        }

        private async Task MarkEpisodeWatched(TvShow tvShow) {
            var episode = tvShow.UnseenEpisode;
            episode.IsWatched = true;
            await _engine.UpdateEntity(tvShow);
        }

        private static bool CanMarkEpisodeWatched(object prm) {
            return CanMarkEpisodeWatched(prm as TvShow);
        }

        private static bool CanMarkEpisodeWatched(TvShow tvShow) {
            return tvShow != null && tvShow.UnseenEpisode != null;
        }

        private static void PlayTvShowEpisode(object prm) {
            PlayTvShowEpisode((TvShowEpisode)prm);
        }

        private static void PlayTvShowEpisode(TvShowEpisode episode) {
            Process.Start(episode.FilePath);
        }

        private static bool CanPlayTvShowEpisode(object prm) {
            return CanPlayTvShowEpisode(prm as TvShowEpisode);
        }

        private static bool CanPlayTvShowEpisode(TvShowEpisode episode) {
            return episode != null && !string.IsNullOrWhiteSpace(episode.FilePath) && File.Exists(episode.FilePath);
        }

        private void DeleteTvShowEpisode(object prm) {
            DeleteTvShowEpisode((TvShowEpisode)prm);
        }

        private async void DeleteTvShowEpisode(TvShowEpisode episode) {
            PinTvShowFlyout = true;

            var result = await DialogService.Confirm(Resources.MontyNi, Resources.AreYouSure);
            if (result) {
                try {
                    var fileInfo = new FileInfo(episode.FilePath);
                    var subtitleFilePath = Novaroma.Helper.GetSubtitleFilePath(fileInfo);
                    if (File.Exists(subtitleFilePath))
                        File.Delete(subtitleFilePath);
                    fileInfo.Delete();

                    episode.FilePath = string.Empty;
                    episode.SubtitleDownloaded = false;
                }
                catch (Exception ex) {
                    _exceptionHandler.HandleException(ex);
                }
            }

            PinTvShowFlyout = false;
        }

        private static bool CanDeleteTvShowEpisode(object prm) {
            return CanDeleteTvShowEpisode(prm as TvShowEpisode);
        }

        private static bool CanDeleteTvShowEpisode(TvShowEpisode episode) {
            return episode != null && !string.IsNullOrEmpty(episode.FilePath) && File.Exists(episode.FilePath);
        }

        private async void DoUpdateTvShowInfo(object prm) {
            var tvShow = prm as TvShow;
            if (tvShow == null) return;

            await UpdateTvShowInfo(tvShow);
        }

        private async Task UpdateTvShowInfo(TvShow tvShow) {
            await Novaroma.Helper.RunTask(() => _engine.UpdateTvShow(tvShow), _exceptionHandler);

            if (SelectedTvShow == tvShow) {
                SelectedTvShow = null;
                SelectedTvShow = tvShow;
            }
        }

        private async void DoDeleteTvShow(object prm) {
            var tvShow = prm as TvShow;
            if (tvShow == null) return;

            SelectedTvShow = null;
            FocusedTvShow = null;
            await DeleteTvShow(tvShow);
        }

        private async Task DeleteTvShow(TvShow tvShow) {
            if (await Confirm(Resources.MontyNi, Resources.AreYouSure))
                await _engine.DeleteEntity(tvShow);
        }

        private async void DoSaveTvShow(object prm) {
            var tvShow = prm as TvShow;
            if (tvShow == null) return;

            if (tvShow.IsModified)
                await SaveTvShow(tvShow);
        }

        private async Task SaveTvShow(TvShow tvShow) {
            await _engine.UpdateEntity(tvShow);
        }

        private async void DoDownloadTvShowEpisode(object prm) {
            var tvShowEpisode = prm as TvShowEpisode;
            if (tvShowEpisode == null) return;

            await DownloadTvShowEpisode(tvShowEpisode);
        }

        private async Task DownloadTvShowEpisode(TvShowEpisode tvShowEpisode) {
            await Helper.ManualDownload(_engine, _exceptionHandler, DialogService, tvShowEpisode);
        }

        private async void DoDownloadTvShowEpisodeSubtitle(object prm) {
            var tvShowEpisode = prm as TvShowEpisode;
            if (tvShowEpisode == null) return;

            await DownloadTvShowEpisodeSubtitle(tvShowEpisode);
        }

        private async Task DownloadTvShowEpisodeSubtitle(TvShowEpisode tvShowEpisode) {
            await Helper.ManualSubtitleDownload(_engine, _exceptionHandler, DialogService, tvShowEpisode);
        }

        private void DiscoverTvShowFiles(object prm) {
            var tvShow = prm as TvShow;
            if (tvShow == null) return;

            DiscoverTvShowFiles(tvShow);
        }

        private void DiscoverTvShowFiles(TvShow tvShow) {
            Novaroma.Helper.InitTvShow(tvShow, _engine);
        }

        private static void GoToTvShowDirectory(object prm) {
            var tvShow = prm as TvShow;
            if (tvShow == null) return;

            if (!string.IsNullOrEmpty(tvShow.Directory) && Directory.Exists(tvShow.Directory))
                Process.Start(tvShow.Directory);
        }

        #endregion

        public void InitialConfiguration() {
            var uTorrentDownloader = IoCContainer.Resolve<UTorrentDownloader>();
            new ConfigurationWindow(_engine, _exceptionHandler, DialogService, uTorrentDownloader).ShowDialog();
        }

        private async Task LoadSavedSearchModels() {
            await LoadSavedSearchModel(MovieSearchModel);
            await LoadSavedSearchModel(TvShowSearchModel);
        }

        private async Task LoadSavedSearchModel(MediaSearchModel searchModel) {
            var c = (IConfigurable)searchModel;
            var cs = await _engine.LoadSettings(c.SettingName);
            if (!string.IsNullOrEmpty(cs))
                c.DeserializeSettings(cs);
        }

        private async void DoCheckForUpdate() {
            await CheckForUpdate();
        }

        private async Task CheckForUpdate() {
            var result = false;
            var updaterPath = Path.Combine(Environment.CurrentDirectory, "Novaroma.Updater.exe");
            if (File.Exists(updaterPath)) {
                var checkProcess = Process.Start(updaterPath, "/justcheck");
                if (checkProcess != null && checkProcess.WaitForExit(10000))
                    result = checkProcess.ExitCode == 0;
            }
            UpdateAvailable = result;

            if (UpdateAvailable) {
                _updateCheckTimer.Dispose();

                var activity = new Activity {
                    ActivityDate = DateTime.Now,
                    Description = Resources.UpdateAvailable,
                    Path = updaterPath + " > /checknow"
                };
                await _engine.InsertEntity(activity);
            }
        }

        private static void InstallUpdate() {
            var updaterPath = Path.Combine(Environment.CurrentDirectory, "Novaroma.Updater.exe");
            Process.Start(updaterPath, " /checknow");
        }

        private static void About() {
            new AboutWindow().ShowDialog();
        }

        private static void PlayActivity(object prm) {
            var activity = prm as Activity;
            if (activity == null) return;

            PlayActivity(activity);
        }

        private static void PlayActivity(Activity activity) {
            var path = activity.Path;
            if (string.IsNullOrWhiteSpace(path)) return;

            var idx = path.IndexOf('>');
            if (idx > 0) {
                var args = path.Substring(idx + 1).Trim();
                path = path.Substring(0, idx).Trim();
                Process.Start(path, args);
            }
            else
                Process.Start(activity.Path);
        }

        private void SendASmile() {
            new FeedbackWindow(_exceptionHandler, _logger, DialogService, false).ShowDialog();
        }

        private void SendAFrown() {
            new FeedbackWindow(_exceptionHandler, _logger, DialogService, true).ShowDialog();
        }

        private void PressEscapeKey() {
            if (_selectedMovie != null) SelectedMovie = null;
            if (_selectedTvShow != null) SelectedTvShow = null;
        }

        #endregion

        #region Event-Handlers

        private void EngineOnMoviesChanged(object sender, EntityContainerChangeEventArgs e) {
            if (e.HasAdded || e.HasDeleted)
                DoGetMovies();
        }

        private void EngineOnTvShowsChanged(object sender, EntityContainerChangeEventArgs e) {
            if (e.HasAdded || e.HasDeleted)
                DoGetTvShows();
        }

        private void EngineOnActivitiesChanged(object sender, EntityContainerChangeEventArgs e) {
            if (e.HasAdded || e.HasDeleted)
                DoGetActivities();
        }

        private void EngineOnLanguageChanged(object sender, LanguageChangeEventArgs e) {
            _tvShowSearchModel.RaiseResourceProperties();
            _movieSearchModel.RaiseResourceProperties();
        }

        #endregion

        #region Properties

        #region Commands

        public RelayCommand InstallUpdateCommand {
            get { return _installUpdateCommand; }
        }

        public RelayCommand AboutCommand {
            get { return _aboutCommand; }
        }

        public RelayCommand NewCommand {
            get { return _newCommand; }
        }

        public RelayCommand WatchDirectoryCommand {
            get { return _watchDirectoryCommand; }
        }

        public RelayCommand DiscoverCommand {
            get { return _discoverCommand; }
        }

        public RelayCommand ManualDownloadCommand {
            get { return _manualDownloadCommand; }
        }

        public RelayCommand ExecutePluginServiceCommand {
            get { return _executePluginServiceCommand; }
        }

        public RelayCommand SettingsCommand {
            get { return _settingsCommand; }
        }

        public RelayCommand ExecuteDownloadsCommand {
            get { return _executeDownloadsCommand; }
        }

        public RelayCommand ExecuteTvShowUpdatesCommand {
            get { return _executeTvShowUpdatesCommand; }
        }

        public RelayCommand BackupDatabaseCommand {
            get { return _backupDatabaseCommand; }
        }

        public RelayCommand ManageRuntimeServicesCommand {
            get { return _manageRuntimeServicesCommand; }
        }

        public RelayCommand ClearLogsAndActivitiesCommand {
            get { return _clearLogsAndActivitiesCommand; }
        }

        public MovieSearchModel MovieSearchModel {
            get { return _movieSearchModel; }
        }

        public TvShowSearchModel TvShowSearchModel {
            get { return _tvShowSearchModel; }
        }

        public RelayCommand SaveSearchModelCommand {
            get { return _saveSearchModelCommand; }
        }

        public RelayCommand MovieSearchCommand {
            get { return _movieSearchCommand; }
        }

        public RelayCommand TvShowSearchCommand {
            get { return _tvShowSearchCommand; }
        }

        public RelayCommand ClearMovieSearchModelCommand {
            get { return _clearMovieSearchModelCommand; }
        }

        public RelayCommand ClearTvShowSearchModelCommand {
            get { return _clearTvShowSearchModelCommand; }
        }

        public RelayCommand MoviePlayCommand {
            get { return _moviePlayCommand; }
        }

        public RelayCommand MovieMarkWatchedCommand {
            get { return _movieMarkWatchedCommand; }
        }

        public RelayCommand MovieUpdateInfoCommand {
            get { return _movieUpdateInfoCommand; }
        }

        public RelayCommand MovieDeleteCommand {
            get { return _movieDeleteCommand; }
        }

        public RelayCommand MovieSaveCommand {
            get { return _movieSaveCommand; }
        }

        public RelayCommand MovieDownloadCommand {
            get { return _movieDownloadCommand; }
        }

        public RelayCommand MovieSubtitleDownloadCommand {
            get { return _movieSubtitleDownloadCommand; }
        }

        public RelayCommand MovieDiscoverFilesCommand {
            get { return _movieDiscoverFilesCommand; }
        }

        public RelayCommand MovieGoToDirectoryCommand {
            get { return _movieGoToDirectoryCommand; }
        }

        public RelayCommand TvShowPlayCommand {
            get { return _tvShowPlayCommand; }
        }

        public RelayCommand TvShowMarkEpisodeWatchedCommand {
            get { return _tvShowMarkEpisodeWatchedCommand; }
        }

        public RelayCommand TvShowEpisodePlayCommand {
            get { return _tvShowEpisodePlayCommand; }
        }

        public RelayCommand TvShowEpisodeDeleteCommand {
            get { return _tvShowEpisodeDeleteCommand; }
        }

        public RelayCommand TvShowUpdateInfoCommand {
            get { return _tvShowUpdateInfoCommand; }
        }

        public RelayCommand TvShowDeleteCommand {
            get { return _tvShowDeleteCommand; }
        }

        public RelayCommand TvShowSaveCommand {
            get { return _tvShowSaveCommand; }
        }

        public RelayCommand TvShowEpisodeDownloadCommand {
            get { return _tvShowEpisodeDownloadCommand; }
        }

        public RelayCommand TvShowEpisodeSubtitleDownloadCommand {
            get { return _tvShowEpisodeSubtitleDownloadCommand; }
        }

        public RelayCommand TvShowDiscoverFilesCommand {
            get { return _tvShowDiscoverFilesCommand; }
        }

        public RelayCommand TvShowGoToDirectoryCommand {
            get { return _tvShowGoToDirectoryCommand; }
        }

        public RelayCommand ActivityPlayCommand {
            get { return _activityPlayCommand; }
        }

        public RelayCommand SendASmileCommand {
            get { return _sendASmileCommand; }
        }

        public RelayCommand SendAFrownCommand {
            get { return _sendAFrownCommand; }
        }

        public RelayCommand EscapeKeyCommand {
            get { return _escapeKeyCommand; }
        }

        public RelayCommand AllTvDownloadCheckCommand {
            get { return _allTvDownloadCheckCommand; }
        }

        public RelayCommand AllSeasonDownloadCheckCommand {
            get { return _allSeasonDownloadCheckCommand; }
        }

        public RelayCommand EpisodeDownloadCheckCommand {
            get { return _episodeDownloadCheckCommand; }
        }

        public RelayCommand MovieDownloadCheckCommand {
            get { return _movieDownloadCheckCommand; }
        }

        #endregion

        public bool UpdateAvailable {
            get { return _updateAvailable; }
            set {
                _updateAvailable = value;

                RaisePropertyChanged("UpdateAvailable");
            }
        }

        public IEnumerable<IPluginService> PluginServices {
            get { return _pluginServices; }
        }

        public bool HasPlugin {
            get { return _pluginServices.Any(); }
        }

        public bool IsEngineInitialized {
            get { return _engine.IsInitialized; }
        }

        public QueryResult<Movie> Movies {
            get { return _movies; }
            private set {
                if (Equals(_movies, value)) return;

                _movies = value;
                RaisePropertyChanged("Movies");
            }
        }

        public QueryResult<TvShow> TvShows {
            get { return _tvShows; }
            private set {
                if (Equals(_tvShows, value)) return;

                _tvShows = value;
                RaisePropertyChanged("TvShows");
            }
        }

        public QueryResult<Activity> Activities {
            get { return _activities; }
            set {
                if (Equals(_activities, value)) return;

                _activities = value;
                RaisePropertyChanged("Activities");
            }
        }

        public bool IsMoviesBusy {
            get { return _isMoviesBusy; }
            set {
                _isMoviesBusy = value;

                RaisePropertyChanged("IsMoviesBusy");
            }
        }

        public bool IsTvShowsBusy {
            get { return _isTvShowsBusy; }
            set {
                _isTvShowsBusy = value;

                RaisePropertyChanged("IsTvShowsBusy");
            }
        }

        public int NotReadActivityCount {
            get { return _notReadActivityCount; }
            private set {
                if (_notReadActivityCount == value) return;

                _notReadActivityCount = value;
                RaisePropertyChanged("NotReadActivityCount");
            }
        }

        public bool IsActivitiesSelected {
            get { return _isActivitiesSelected; }
            set {
                if (_isActivitiesSelected == value) return;

                _isActivitiesSelected = value;
                RaisePropertyChanged("IsActivitiesSelected");

                if (value) return;

                var unreadActivities = _activities.Results.Where(a => !a.IsRead).ToList();
                if (unreadActivities.Any()) {
                    unreadActivities.ForEach(a => a.IsRead = true);
                    _engine.SaveChanges(null, unreadActivities);
                }
            }
        }

        public Movie SelectedMovie {
            get { return _selectedMovie; }
            set {
                if (_selectedMovie == value) return;

                DoSaveMovie(_selectedMovie);
                _selectedMovie = value;
                RaisePropertyChanged("SelectedMovie");

                SelectedTvShow = null;
            }
        }

        public Movie FocusedMovie {
            get { return _focusedMovie; }
            set {
                if (_focusedMovie == value) return;
                _focusedMovie = value;
                RaisePropertyChanged("FocusedMovie");
            }
        }

        public TvShow SelectedTvShow {
            get { return _selectedTvShow; }
            set {
                if (_selectedTvShow == value) return;

                DoSaveTvShow(_selectedTvShow);
                _selectedTvShow = value;
                RaisePropertyChanged("SelectedTvShow");

                SelectedMovie = null;
            }
        }

        public TvShow FocusedTvShow {
            get { return _focusedTvShow; }
            set {
                if (_focusedTvShow == value) return;

                _focusedTvShow = value;
                RaisePropertyChanged("FocusedTvShow");
            }
        }

        public int MovieMaxPage {
            get { return _movieMaxPage; }
            set {
                if (_movieMaxPage == value) return;

                _movieMaxPage = value;
                RaisePropertyChanged("MovieMaxPage");
            }
        }

        public int TvShowMaxPage {
            get { return _tvShowMaxPage; }
            set {
                if (_tvShowMaxPage == value) return;

                _tvShowMaxPage = value;
                RaisePropertyChanged("TvShowMaxPage");
            }
        }

        public bool EnableMoviePreviousButton {
            get { return _enableMoviePreviousButton; }
            set {
                if (_enableMoviePreviousButton == value) return;

                _enableMoviePreviousButton = value;
                RaisePropertyChanged("EnableMoviePreviousButton");
            }
        }

        public bool EnableMovieNextButton {
            get { return _enableMovieNextButton; }
            set {
                if (_enableMovieNextButton == value) return;

                _enableMovieNextButton = value;
                RaisePropertyChanged("EnableMovieNextButton");
            }
        }

        public bool EnableTvShowPreviousButton {
            get { return _enableTvShowPreviousButton; }
            set {
                if (_enableTvShowPreviousButton == value) return;

                _enableTvShowPreviousButton = value;
                RaisePropertyChanged("EnableTvShowPreviousButton");
            }
        }

        public bool EnableTvShowNextButton {
            get { return _enableTvShowNextButton; }
            set {
                if (_enableTvShowNextButton == value) return;

                _enableTvShowNextButton = value;
                RaisePropertyChanged("EnableTvShowNextButton");
            }
        }

        public IEnumerable<EnumInfo<VideoQuality>> VideoQualityEnumInfo {
            get { return _videoQualityEnumInfo; }
        }

        public bool HasTvShows {
            get {
                return TvShows != QueryResult<TvShow>.Empty;
            }
        }

        public bool HasMovies {
            get {
                return Movies != QueryResult<Movie>.Empty;
            }
        }

        public bool PinTvShowFlyout {
            get { return _pinTvShowFlyout; }
            set {
                _pinTvShowFlyout = value;
                RaisePropertyChanged("PinTvShowFlyout");
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            _engine.MoviesChanged -= EngineOnMoviesChanged;
            _engine.TvShowsChanged -= EngineOnTvShowsChanged;
            _engine.ActivitiesChanged -= EngineOnActivitiesChanged;
        }

        #endregion
    }
}
