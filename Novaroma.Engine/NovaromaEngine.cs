using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Novaroma.Engine.Jobs;
using Novaroma.Interface;
using Novaroma.Interface.Download;
using Novaroma.Interface.Info;
using Novaroma.Interface.Model;
using Novaroma.Interface.Subtitle;
using Novaroma.Interface.Track;
using Novaroma.Model;
using Novaroma.Model.Search;
using Novaroma.Properties;
using Novaroma.Services.Imdb;
using Quartz;

namespace Novaroma.Engine {

    public class NovaromaEngine : INovaromaEngine {
        private readonly IContextFactory _contextFactory;
        private readonly IScheduler _scheduler;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly bool _isInitialized;
        private readonly EngineSettings _settings;
        private readonly IEnumerable<IImdbIdConverter> _imdbIdConverters;
        private readonly ObservableCollection<string> _mediaGenres;
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private readonly IEnumerable<INovaromaService> _services;
        private const string DownloadJobName = "DownloadJob";
        private const string DownloadJobDefaultTriggerName = "DownloadJobTrigger";
        private const string TvShowUpdateJobName = "TvShowUpdateJob";
        private const string TvShowUpdateJobDefaultTriggerName = "TvShowUpdateJobTrigger";
        private readonly Semaphore _infoSemaphore = new Semaphore(20, 20);
        private readonly Semaphore _tvShowUpdateSemaphore = new Semaphore(20, 20);
        private readonly Semaphore _downloadSemaphore = new Semaphore(20, 20);
        private readonly Semaphore _subtitleSemaphore = new Semaphore(20, 20);

        public NovaromaEngine(IContextFactory contextFactory, IEnumerable<INovaromaService> services, IScheduler scheduler, IExceptionHandler exceptionHandler) {
            _contextFactory = contextFactory;
            _scheduler = scheduler;
            _exceptionHandler = exceptionHandler;

            List<ScriptService> scriptServices;
            List<WatchDirectory> watchDirectories;
            List<Setting> settingStrings;
            using (var context = _contextFactory.CreateContext()) {
                scriptServices = context.ScriptServices.ToList();
                watchDirectories = context.WatchDirectories.ToList();
                settingStrings = context.Settings.ToList();
                var mediaGenres = context.Medias
                    .SelectMany(m => m.Genres.Select(g => g.Name))
                    .Distinct()
                    .ToList()
                    .OrderBy(g => g);
                _mediaGenres = new NovaromaObservableCollection<string>(mediaGenres);
            }

            var scriptAssemblies = scriptServices.Select(ss => Helper.CompileCode(ss.Code).CompiledAssembly).ToList();
            var scriptServiceTypes = scriptAssemblies.SelectMany(a => a.ExportedTypes.Where(t => typeof(INovaromaService).IsAssignableFrom(t)));
            var scriptServicesInstances = scriptServiceTypes.Select(Activator.CreateInstance).OfType<INovaromaService>();

            var serviceList = services as List<INovaromaService> ?? services.ToList();
            serviceList.AddRange(scriptServicesInstances);
            _services = serviceList;
            _settings = new EngineSettings(serviceList);
            _imdbIdConverters = serviceList.OfType<IImdbIdConverter>();

            foreach (var watchDirectory in watchDirectories)
                AddDirectoryWatcher(watchDirectory.Directory);

            _isInitialized = settingStrings.Any(s => s.SettingName == ((IConfigurable)this).SettingName);
            foreach (var configurable in serviceList.OfType<IConfigurable>().Union(new[] { this })) {
                var settingStr = settingStrings.FirstOrDefault(s => s.SettingName == configurable.SettingName);
                if (settingStr != null)
                    configurable.DeserializeSettings(settingStr.Value);
            }
            Helper.SetCulture(Settings.LanguageSelection.SelectedItem.Item);

            foreach (var downloader in Settings.Downloader.Items)
                downloader.DownloadCompleted += DownloaderOnDownloadCompleted;

            var downloadJob = JobBuilder.Create<DownloadJob>().WithIdentity(DownloadJobName).Build();
            var downloadJobTrigger = CreateIntervalTrigger(DownloadJobDefaultTriggerName, Settings.DownloadInterval);
            scheduler.ScheduleJob(downloadJob, downloadJobTrigger);

            var tvShowUpdateJob = JobBuilder.Create<TvShowUpdateJob>().WithIdentity(TvShowUpdateJobName).Build();
            var tvShowUpdateJobTrigger = CreateIntervalTrigger(TvShowUpdateJobDefaultTriggerName, Settings.TvShowUpdateInterval * 60);
            scheduler.ScheduleJob(tvShowUpdateJob, tvShowUpdateJobTrigger);

            scheduler.Start();

            Settings.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "DownloadInterval") {
                    var newTrigger = CreateIntervalTrigger(DownloadJobDefaultTriggerName, Settings.DownloadInterval);
                    scheduler.RescheduleJob(new TriggerKey(DownloadJobDefaultTriggerName), newTrigger);
                }
                else if (args.PropertyName == "TvShowUpdateInterval") {
                    var newTrigger = CreateIntervalTrigger(TvShowUpdateJobDefaultTriggerName, Settings.TvShowUpdateInterval * 60);
                    scheduler.RescheduleJob(new TriggerKey(TvShowUpdateJobDefaultTriggerName), newTrigger);
                }
            };
            Settings.LanguageSelection.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "SelectedItem") {
                    var language = Settings.LanguageSelection.SelectedItem.Item;
                    Helper.SetCulture(language);
                    OnLanguageChanged(language);
                }
            };
            Settings.MovieDirectory.PropertyChanged += async (sender, args) => {
                await WatchDirectory(Settings.MovieDirectory.Path);
            };
            Settings.TvShowDirectory.PropertyChanged += async (sender, args) => {
                await WatchDirectory(Settings.TvShowDirectory.Path);
            };
        }

        #region Methods

        private static ITrigger CreateIntervalTrigger(string triggerName, int interval) {
            return TriggerBuilder.Create()
                .WithIdentity(triggerName)
                .StartAt(DateTime.UtcNow.AddMinutes(1))
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(interval)
                    .RepeatForever())
                .Build();
        }

        private void AddDirectoryWatcher(string directory) {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) return;

            if (directory.EndsWith(@"\")) directory = directory.Substring(0, directory.Length - 1);
            if (_watchers.Any(w => string.Equals(w.Path, directory, StringComparison.OrdinalIgnoreCase))) return;

            var fsw = new FileSystemWatcher(directory) { IncludeSubdirectories = true, EnableRaisingEvents = true };
            fsw.Created += DirectoryWatcherOnCreated;
            fsw.Deleted += DirectoryWatcherOnDeleted;
            fsw.Renamed += DirectoryWatcherOnRenamed;
            _watchers.Add(fsw);
        }

        private void RemoveDirectoryWatcher(string directory) {
            if (directory.EndsWith(@"\")) directory = directory.Substring(0, directory.Length - 1);

            var watcher = _watchers.FirstOrDefault(fsw => string.Equals(fsw.Path, directory, StringComparison.OrdinalIgnoreCase));
            if (watcher != null) {
                _watchers.Remove(watcher);
                watcher.Dispose();
            }
        }

        private void DirectoryWatcherOnCreated(object sender, FileSystemEventArgs args) {
            if (Directory.Exists(args.FullPath))
                OnDirectoryAdded(args.FullPath);
        }

        private async void DirectoryWatcherOnDeleted(object sender, FileSystemEventArgs args) {
            using (var context = _contextFactory.CreateContext()) {
                if (Directory.Exists(args.FullPath)) {
                    var media = context.Medias.FirstOrDefault(m => string.Equals(m.Directory, args.FullPath, StringComparison.OrdinalIgnoreCase));
                    if (media != null) {
                        media.IsDeleted = true;

                        context.Update(media);
                        await context.SaveChanges();

                        var movie = media as Movie;
                        if (movie != null) OnMoviesChanged();
                        else {
                            var tvShow = media as TvShow;
                            if (tvShow != null) OnTvShowsChanged();
                        }
                    }

                    OnDirectoryDeleted(args.FullPath);
                }
                else {
                    var movie = context.Movies.FirstOrDefault(m => string.Equals(m.FilePath, args.FullPath, StringComparison.OrdinalIgnoreCase));
                    if (movie != null) {
                        movie.FilePath = string.Empty;
                        context.Update(movie);
                        await context.SaveChanges();

                        OnMoviesChanged();
                    }
                    else {
                        var tvShowEpisode = context.TvShows.Episodes().FirstOrDefault(e => string.Equals(e.FilePath, args.FullPath, StringComparison.OrdinalIgnoreCase));
                        if (tvShowEpisode != null) {
                            tvShowEpisode.FilePath = string.Empty;

                            context.Update(tvShowEpisode.TvShowSeason.TvShow);
                            await context.SaveChanges();

                            OnTvShowsChanged();
                        }
                    }

                }
            }
        }

        private async void DirectoryWatcherOnRenamed(object sender, RenamedEventArgs args) {
            using (var context = _contextFactory.CreateContext()) {
                if (Directory.Exists(args.FullPath)) {
                    var media = context.Medias.FirstOrDefault(m => string.Equals(m.Directory, args.OldFullPath, StringComparison.OrdinalIgnoreCase));
                    if (media != null) {
                        media.Directory = args.FullPath;

                        context.Update(media);
                        await context.SaveChanges();

                        if (media is Movie) OnMoviesChanged();
                        else if (media is TvShow) OnTvShowsChanged();
                    }

                    OnDirectoryRenamed(args.FullPath, args.OldFullPath);
                }
                else {
                    var movie = context.Movies.FirstOrDefault(m => string.Equals(m.FilePath, args.OldFullPath, StringComparison.OrdinalIgnoreCase));
                    if (movie != null) {
                        movie.FilePath = args.FullPath;
                        context.Update(movie);
                        await context.SaveChanges();

                        OnMoviesChanged();
                    }
                    else {
                        var tvShowEpisode = context.TvShows.Episodes().FirstOrDefault(e => string.Equals(e.FilePath, args.OldFullPath, StringComparison.OrdinalIgnoreCase));
                        if (tvShowEpisode != null) {
                            tvShowEpisode.FilePath = args.FullPath;

                            context.Update(tvShowEpisode.TvShowSeason.TvShow);
                            await context.SaveChanges();

                            OnTvShowsChanged();
                        }
                    }

                }
            }
        }

        private void DownloaderOnDownloadCompleted(object sender, DownloadCompletedEventArgs args) {
            var fileName = Helper.GetFirstVideoFileName(args.DownloadDirectory);

            using (var context = _contextFactory.CreateContext()) {
                var episode = context.TvShows.Episodes().FirstOrDefault(e => e.DownloadKey == args.DownloadKey);

                if (episode != null) {
                    var directory = Helper.GetTvShowSeasonDirectory(Settings.TvShowSeasonDirectoryTemplate, episode);
                    if (Directory.Exists(args.DownloadDirectory)) {
                        if (args.DownloadDirectory != directory) {
                            Helper.MoveDirectory(args.DownloadDirectory, directory, Settings.DeleteExtensions,
                                args.Files);
                            args.Delete = true;
                        }

                        if (!string.IsNullOrEmpty(fileName))
                            episode.FilePath = Directory.GetFiles(directory, fileName).FirstOrDefault();
                    }
                    OnTvShowEpisodeDownloadCompleted(episode);

                    var season = episode.TvShowSeason;
                    var show = season.TvShow;
                    var activity = CreateActivity(
                        string.Format(Resources.TvShowEpisodeDownloaded, show.Title, season.Season, episode.Episode),
                        episode.FilePath
                    );
                    context.Insert(activity);
                    context.Update(episode.TvShowSeason.TvShow);
                    context.SaveChanges().Wait();

                    if (SubtitlesEnabled)
                        DownloadSubtitleForTvShowEpisode(episode).Wait();
                    else {
                        OnTvShowsChanged();
                        OnActivitiesChanged();
                    }
                }
                else {
                    var movie = context.Movies.FirstOrDefault(m => m.DownloadKey == args.DownloadKey);

                    if (movie != null) {
                        if (Directory.Exists(args.DownloadDirectory)) {
                            if (args.DownloadDirectory != movie.Directory) {
                                Helper.MoveDirectory(args.DownloadDirectory, movie.Directory, Settings.DeleteExtensions, args.Files);
                                args.Delete = true;
                            }

                            movie.FilePath = Directory.GetFiles(movie.Directory, fileName).FirstOrDefault();
                        }
                        OnMovieDownloadCompleted(movie);

                        var activity = CreateActivity(string.Format(Resources.MovieDownloaded, movie.Title), movie.FilePath);
                        context.Insert(activity);
                        context.Update(movie);
                        context.SaveChanges().Wait();

                        if (SubtitlesEnabled)
                            DownloadSubtitleForMovie(movie).Wait();
                        else {
                            OnMoviesChanged();
                            OnActivitiesChanged();
                        }
                    }
                    else if (SubtitlesEnabled) {
                        var files = Directory.GetFiles(args.DownloadDirectory).Where(Helper.IsVideoFile).ToList();
                        foreach (var file in files)
                            DownloadSubtitle(file).Wait();
                    }
                }
            }
        }

        private static QueryResult<TMedia> FilterMediaQuery<TMedia>(IQueryable<TMedia> query, MediaSearchModel searchModel) where TMedia : Media {
            if (!string.IsNullOrWhiteSpace(searchModel.Query))
                query = query.Where(x => x.Title.StartsWith(searchModel.Query, StringComparison.OrdinalIgnoreCase)
                                    || x.Title.IndexOf(searchModel.Query, StringComparison.OrdinalIgnoreCase) > 0);

            if (searchModel.ReleaseYearStart.HasValue)
                query = query.Where(x => x.Year >= searchModel.ReleaseYearStart);

            if (searchModel.ReleaseYearEnd.HasValue)
                query = query.Where(x => x.Year <= searchModel.ReleaseYearEnd);

            if (searchModel.RatingMin.HasValue && searchModel.RatingMin.Value > 0)
                query = query.Where(x => x.Rating >= searchModel.RatingMin);

            if (searchModel.RatingMax.HasValue && searchModel.RatingMax.Value > 0)
                query = query.Where(x => x.Rating <= searchModel.RatingMax);

            if (searchModel.NumberOfVotesMin.HasValue)
                query = query.Where(x => x.VoteCount >= searchModel.NumberOfVotesMin);

            if (searchModel.NumberOfVotesMax.HasValue)
                query = query.Where(x => x.VoteCount <= searchModel.NumberOfVotesMax);

            if (searchModel.RuntimeMin.HasValue)
                query = query.Where(x => x.Runtime >= searchModel.RuntimeMin);

            if (searchModel.RuntimeMax.HasValue)
                query = query.Where(x => x.Runtime <= searchModel.RuntimeMax);

            if (searchModel.Genres.SelectedItems.Any())
                query = query.Where(x => searchModel.Genres.SelectedItems.Any(g => x.Genres.Any(y => y.Name == g)));

            var selectedOrder = searchModel.SelectedOrder;
            if (selectedOrder != null) {
                switch (selectedOrder.Order.Item) {
                    case OrderFields.Title:
                        query = selectedOrder.IsDescending
                            ? query.OrderByDescending(x => x.Title)
                            : query.OrderBy(x => x.Title);
                        break;
                    case OrderFields.Year:
                        query = selectedOrder.IsDescending
                            ? query.OrderByDescending(x => x.Year)
                            : query.OrderBy(x => x.Year);
                        break;
                    case OrderFields.Rating:
                        query = selectedOrder.IsDescending
                            ? query.OrderByDescending(x => x.Rating)
                            : query.OrderBy(x => x.Rating);
                        break;
                }
            }

            var inlineCount = query.Count();

            var take = searchModel.PageSize;
            var skip = (searchModel.Page - 1) * take;
            query = query.Skip(skip).Take(take);
            var result = query.ToList();

            return new QueryResult<TMedia>(result, inlineCount);
        }

        private static void SetServiceIds(Media media, IEnumerable<KeyValuePair<string, object>> serviceIds) {
            foreach (var serviceId in serviceIds) {
                if (media.ServiceMappings.All(sm => sm.ServiceName != serviceId.Key))
                    media.ServiceMappings.Add(new ServiceMapping {
                        MediaId = media.Id,
                        ServiceId = serviceId.Value.ToString(),
                        ServiceName = serviceId.Key
                    });
            }
        }

        private async Task<ITvShowUpdate> GetTvShowUpdate(TvShow tvShow) {
            var service = Settings.ShowTracker.SelectedItem;

            object serviceId;
            if (tvShow.ServiceName == service.ServiceName)
                serviceId = tvShow.ServiceId;
            else {
                var serviceMapping = tvShow.ServiceMappings.FirstOrDefault(sm => sm.ServiceName == service.ServiceName);

                if (serviceMapping != null)
                    serviceId = serviceMapping.ServiceId;
                else {
                    if (string.IsNullOrEmpty(tvShow.ImdbId))
                        throw new NovaromaException(Resources.ImdbIdNotFound);

                    var serviceIds = await ConvertImdbId(tvShow);
                    SetServiceIds(tvShow, serviceIds);

                    serviceId = tvShow.ServiceMappings
                        .Where(sm => sm.ServiceName == service.ServiceName)
                        .Select(sm => sm.ServiceId)
                        .FirstOrDefault();

                    if (serviceId == null)
                        throw new NovaromaException(string.Format(Resources.ServiceIdNotFound, service.ServiceName));
                }
            }

            await Task.Run(() => _tvShowUpdateSemaphore.WaitOne());

            try {
                return await service.GetTvShowUpdate(serviceId.ToString(), tvShow.LastUpdateDate);
            }
            finally {
                _tvShowUpdateSemaphore.Release();
            }
        }

        private static Activity CreateActivity(string description, string path) {
            return new Activity {
                ActivityDate = DateTime.UtcNow,
                Description = description,
                Path = path
            };
        }

        private void SetMovieDirectory(Movie movie) {
            var movieDirectory = MovieDirectory;
            if (string.IsNullOrEmpty(movieDirectory))
                movieDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), Resources.Movies);

            movie.Directory = Path.Combine(movieDirectory, movie.Title);
        }

        private void SetTvShowDirectory(TvShow tvShow) {
            var tvShowDirectory = TvShowDirectory;
            if (string.IsNullOrEmpty(tvShowDirectory))
                tvShowDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), Resources.TvShows);

            tvShow.Directory = Path.Combine(tvShowDirectory, tvShow.Title);
        }

        private async Task<FileInfo> ValidateSubtitleDownload(string filePath) {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            Activity activity = null;
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                activity = CreateActivity(string.Format(Resources.SubtitleNotDownloaded_FileNotFound, filePath), filePath);

            if (!SubtitleLanguages.Any())
                activity = CreateActivity(Resources.SubtitleNotDownloaded_LanguagesNotSelected, string.Empty);

            if (!Settings.SubtitleDownloaders.SelectedItems.Any())
                activity = CreateActivity(Resources.SubtitleNotDownloaded_DownloadersNotSelected, string.Empty);

            if (activity != null) {
                await InsertEntity(activity);
                return null;
            }

            return fileInfo;
        }

        private Task<IEnumerable<Movie>> GetMoviesToDownload() {
            if (Settings.Downloader.SelectedItem == null)
                return Task.FromResult(Enumerable.Empty<Movie>());

            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.Movies
                        .Where(m => m.BackgroundDownload)
                        .Take(10)
                        .ToList()
                        .AsEnumerable();
                }
            });
        }

        private Task<IEnumerable<TvShowEpisode>> GetTvShowEpisodesToDownload() {
            if (Settings.Downloader.SelectedItem == null)
                return Task.FromResult(Enumerable.Empty<TvShowEpisode>());

            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.TvShows
                        .Episodes()
                        .Where(e => e.BackgroundDownload && e.AirDate.Date.AddHours(24) < DateTime.UtcNow)
                        .Take(10)
                        .ToList()
                        .AsEnumerable();
                }
            });
        }

        private Task<IEnumerable<Movie>> GetMoviesForSubtitleDownload() {
            if (!SubtitlesEnabled)
                return Task.FromResult(Enumerable.Empty<Movie>());

            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.Movies
                        .Where(m => !string.IsNullOrEmpty(m.FilePath) && m.BackgroundSubtitleDownload)
                        .Where(m => File.Exists(m.FilePath))
                        .Take(10)
                        .ToList()
                        .AsEnumerable();
                }
            });
        }

        private Task<IEnumerable<TvShowEpisode>> GetTvShowEpisodesForSubtitleDownload() {
            if (!SubtitlesEnabled)
                return Task.FromResult(Enumerable.Empty<TvShowEpisode>());

            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.TvShows
                        .Episodes()
                        .Where(e => !string.IsNullOrEmpty(e.FilePath) && e.BackgroundSubtitleDownload)
                        .Take(10)
                        .ToList()
                        .AsEnumerable();
                }
            });
        }

        private Task<IEnumerable<TvShow>> GetTvshowsToUpdate() {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.TvShows
                        .ToList()
                        .AsEnumerable();
                }
            });
        }

        #endregion

        #region Properties

        public EngineSettings Settings {
            get { return _settings; }
        }

        #endregion

        #region INovaromaEngine Members

        public Task<DirectoryWatchStatus> GetDirectoryWatchStatus(string directory) {
            return Task.Run(() => {
                if (directory.EndsWith(@"\")) directory = directory.Substring(0, directory.Length - 1);
                using (var context = _contextFactory.CreateContext()) {
                    var watchDirectory = context.WatchDirectories.FirstOrDefault(wd => string.Equals(directory, wd.Directory, StringComparison.OrdinalIgnoreCase));
                    if (watchDirectory != null) return DirectoryWatchStatus.Direct;

                    return context.WatchDirectories.Any(wd => directory.StartsWith(wd.Directory + @"\", StringComparison.OrdinalIgnoreCase))
                        ? DirectoryWatchStatus.Parent
                        : DirectoryWatchStatus.None;
                }
            });
        }

        public Task WatchDirectory(string directory) {
            return Task.Run(async () => {
                if (!Directory.Exists(directory)) return;

                if (directory.EndsWith(@"\")) directory = directory.Substring(0, directory.Length - 1);
                using (var context = _contextFactory.CreateContext()) {
                    if (!context.WatchDirectories.Any(wd => string.Equals(wd.Directory, directory, StringComparison.OrdinalIgnoreCase))) {
                        var childWatchers = context.WatchDirectories.Where(wd => wd.Directory.StartsWith(directory + @"\", StringComparison.OrdinalIgnoreCase));
                        foreach (var watchDirectory in childWatchers) {
                            context.Delete(watchDirectory);
                            RemoveDirectoryWatcher(watchDirectory.Directory);
                        }

                        context.Insert(new WatchDirectory { Directory = directory });
                        await context.SaveChanges()
                            .ContinueWith(t => AddDirectoryWatcher(directory));
                    }

                    if (Settings.MakeSpecialFolder) {
                        try {
                            Helper.MakeSpecialFolder(new DirectoryInfo(directory), Resources.Img_NovaromaFolder, string.Empty);
                        }
                        catch (Exception ex) {
                            _exceptionHandler.HandleException(ex);
                        }
                    }
                }
            });
        }

        public Task StopWatching(string directory) {
            return Task.Run(async () => {
                if (directory.EndsWith(@"\")) directory = directory.Substring(0, directory.Length - 1);
                using (var context = _contextFactory.CreateContext()) {
                    var watchDirectory = context.WatchDirectories.FirstOrDefault(wd => string.Equals(wd.Directory, directory, StringComparison.OrdinalIgnoreCase));
                    if (watchDirectory != null) {
                        context.Delete(watchDirectory);
                        await context.SaveChanges()
                            .ContinueWith(t => RemoveDirectoryWatcher(directory));

                        var iniPath = Path.Combine(directory, "desktop.ini");
                        if (File.Exists(iniPath))
                            File.Delete(iniPath);
                        var icoPath = Path.Combine(directory, "Folder.ico");
                        if (File.Exists(icoPath))
                            File.Delete(icoPath);
                    }
                }
            });
        }

        public async Task<IEnumerable<IInfoSearchResult>> SearchInfo(string query) {
            await Task.Run(() => _infoSemaphore.WaitOne());

            try {
                return await Settings.InfoProvider.SelectedItem.Search(query);
            }
            finally {
                _infoSemaphore.Release();
            }
        }

        public async Task<IMovieInfo> GetMovieInfo(IInfoSearchResult searchResult) {
            await Task.Run(() => _infoSemaphore.WaitOne());

            try {
                return await searchResult.Service.GetMovie(searchResult);
            }
            finally {
                _infoSemaphore.Release();
            }
        }

        public async Task<ITvShowInfo> GetTvShowInfo(IInfoSearchResult searchResult) {
            await Task.Run(() => _infoSemaphore.WaitOne());

            try {
                return await searchResult.Service.GetTvShow(searchResult);
            }
            finally {
                _infoSemaphore.Release();
            }
        }

        public IEnumerable<string> GetAdvancedInfoProviderGenres() {
            return Settings.AdvancedInfoProvider.SelectedItem.Genres;
        }

        public async Task<IEnumerable<IAdvancedInfoSearchResult>> AdvancedSearchInfo(string query, MediaTypes mediaTypes = MediaTypes.All, int? releaseYearStart = null, int? releaseYearEnd = null,
                                                                                     float? ratingMin = null, float? ratingMax = null, int? numberOfVotesMin = null, int? numberOfVotesMax = null,
                                                                                     int? runtimeMin = null, int? runtimeMax = null, IEnumerable<string> genres = null) {
            await Task.Run(() => _infoSemaphore.WaitOne());

            try {
                return await Settings.AdvancedInfoProvider.SelectedItem.AdvancedSearch(query, mediaTypes, releaseYearStart, releaseYearEnd, ratingMin, ratingMax,
                                                                                       numberOfVotesMin, numberOfVotesMax, runtimeMin, runtimeMax, genres);
            }
            finally {
                _infoSemaphore.Release();
            }
        }

        public async Task<IDictionary<string, object>> ConvertImdbId(Media media) {
            var serviceIdsList = new List<IDictionary<string, object>>();
            var tasks = _imdbIdConverters.RunTasks(iic =>
                iic.GetServiceIds(media.ImdbId)
                    .ContinueWith(t => serviceIdsList.Add(t.Result)),
                _exceptionHandler
            );
            await Task.WhenAll(tasks);

            var retVal = new Dictionary<string, object>();
            foreach (var serviceIds in serviceIdsList) {
                foreach (var serviceId in serviceIds) {
                    if (serviceId.Value != null && !retVal.ContainsKey(serviceId.Key))
                        retVal.Add(serviceId.Key, serviceId.Value);
                }
            }
            return retVal;
        }

        public async Task<Media> GetMedia(IInfoSearchResult searchResult) {
            if (searchResult.IsTvShow) {
                var tvShowInfo = await GetTvShowInfo(searchResult);
                var tvShow = Helper.MapToModel<TvShow>(tvShowInfo);
                await GetTvShowUpdate(tvShow).ContinueWith(tt => tvShow.Update(tt.Result));
                SetTvShowDirectory(tvShow);
                return tvShow;
            }

            var movieInfo = await GetMovieInfo(searchResult);
            var movie = Helper.MapToModel<Movie>(movieInfo);
            SetMovieDirectory(movie);
            return movie;
        }

        public async Task<Media> GetImdbMedia(string imdbId) {
            await Task.Run(() => _infoSemaphore.WaitOne());

            try {
                var imdb = Settings.InfoProvider.Items.OfType<ImdbInfoProvider>().First();
                var mediaInfo = await imdb.GetTitle(imdbId);
                var tvShowInfo = mediaInfo as ITvShowInfo;
                if (tvShowInfo != null) {
                    var tvShow = Helper.MapToModel<TvShow>(tvShowInfo);
                    await GetTvShowUpdate(tvShow).ContinueWith(tt => tvShow.Update(tt.Result));
                    SetTvShowDirectory(tvShow);
                    return tvShow;
                }

                var movie = Helper.MapToModel<Movie>(mediaInfo);
                SetMovieDirectory(movie);
                return movie;
            }
            finally {
                _infoSemaphore.Release();
            }
        }

        public async Task UpdateMediaInfo(Media media) {
            var service = Settings.InfoProvider.SelectedItem;

            object serviceId;
            if (media.ServiceName == service.ServiceName)
                serviceId = media.ServiceId;
            else {
                var serviceMapping = media.ServiceMappings.FirstOrDefault(sm => sm.ServiceName == service.ServiceName);

                if (serviceMapping != null)
                    serviceId = serviceMapping.ServiceId;
                else {
                    if (string.IsNullOrEmpty(media.ImdbId))
                        throw new NovaromaException(Resources.ImdbIdNotFound);

                    var serviceIds = await ConvertImdbId(media);
                    SetServiceIds(media, serviceIds);

                    serviceId = media.ServiceMappings
                        .Where(sm => sm.ServiceName == service.ServiceName)
                        .Select(sm => sm.ServiceId)
                        .FirstOrDefault();

                    if (serviceId == null)
                        throw new NovaromaException(string.Format(Resources.ServiceIdNotFound, service.ServiceName));
                }
            }

            await Task.Run(() => _infoSemaphore.WaitOne());

            try {
                var serviceIdStr = serviceId.ToString();
                var tvShow = media as TvShow;
                if (tvShow != null) {
                    var info = await service.GetTvShow(serviceIdStr);
                    Helper.MapToModel(info, tvShow);
                }
                else {
                    var movie = media as Movie;
                    var info = await service.GetMovie(serviceIdStr);
                    Helper.MapToModel(info, movie);
                }

                await UpdateEntity(media);
            }
            finally {
                _infoSemaphore.Release();
            }
        }

        public Task InsertEntity(IEntity entity) {
            return SaveChanges(new[] { entity }, null);
        }

        public Task UpdateEntity(IEntity entity) {
            return SaveChanges(null, new[] { entity });
        }

        public Task DeleteEntity(IEntity entity) {
            return SaveChanges(null, null, new[] { entity });
        }

        public Task SaveChanges(IEnumerable<IEntity> add, IEnumerable<IEntity> update, IEnumerable<IEntity> delete = null) {
            return Task.Run(async () => {
                using (var context = _contextFactory.CreateContext()) {
                    bool movieModify, movieDelete, tvShowAdd, tvShowModify, tvShowDelete, activityAdd, activityModify, activityDelete;
                    var movieAdd = movieModify = movieDelete = tvShowAdd = tvShowModify = tvShowDelete = activityAdd = activityModify = activityDelete = false;

                    var modifiedMediaList = new List<Media>();
                    if (add != null)
                        foreach (var entity in add) {
                            context.Insert(entity);

                            var media = entity as Media;
                            if (media != null) {
                                modifiedMediaList.Add(media);

                                if (media is Movie) movieAdd = true;
                                else tvShowAdd = true;
                            }
                            else if (entity is Activity)
                                activityAdd = true;
                        }

                    if (update != null)
                        foreach (var entity in update) {
                            context.Update(entity);

                            var media = entity as Media;
                            if (media != null) {
                                modifiedMediaList.Add(media);

                                if (media is Movie) movieModify = true;
                                else tvShowModify = true;
                            }
                            else if (entity is Activity)
                                activityModify = true;
                        }

                    var deletedMediaList = new List<Media>();
                    if (delete != null)
                        foreach (var entity in delete) {
                            context.Delete(entity);

                            var media = entity as Media;
                            if (media != null) {
                                deletedMediaList.Add(media);

                                if (entity is Movie) movieDelete = true;
                                else if (entity is TvShow) tvShowDelete = true;
                            }
                            else if (entity is Activity) activityDelete = true;
                        }

                    await context.SaveChanges();


                    foreach (var media in modifiedMediaList) {
                        if (Settings.MakeSpecialFolder) {
                            try {
                                modifiedMediaList.ForEach(Helper.MakeSpecialFolder);
                            }
                            catch (Exception ex) {
                                _exceptionHandler.HandleException(ex);
                            }
                        }

                        try {
                            Helper.CreateMediaInfo(media);
                        }
                        catch (Exception ex) {
                            _exceptionHandler.HandleException(ex);
                        }
                    }

                    if (Settings.DeleteDirectoriesAlso) {
                        foreach (var deletedMedia in deletedMediaList) {
                            try {
                                var dir = deletedMedia.Directory;
                                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                                    Directory.Delete(dir, true);
                            }
                            catch (Exception ex) {
                                _exceptionHandler.HandleException(ex);
                            }
                        }
                    }

                    OnMoviesChanged(movieModify, movieAdd, movieDelete);
                    OnTvShowsChanged(tvShowModify, tvShowAdd, tvShowDelete);
                    OnActivitiesChanged(activityModify, activityAdd, activityDelete);
                }
            });
        }

        public Task SaveSettings(string settingName, string settingsJson) {
            return Task.Run(async () => {
                using (var context = _contextFactory.CreateContext()) {
                    var entity = context.Settings.FirstOrDefault(s => s.SettingName == settingName);
                    bool isNew;
                    if (entity == null) {
                        entity = new Setting { SettingName = settingName };
                        isNew = true;
                    }
                    else
                        isNew = false;

                    entity.Value = settingsJson;

                    if (isNew)
                        context.Insert(entity);
                    else
                        context.Update(entity);

                    await context.SaveChanges();
                }
            });
        }

        public Task<IEnumerable<ScriptService>> GetScriptServices() {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    IEnumerable<ScriptService> results = context.ScriptServices.ToList();
                    return results;
                }
            });
        }

        public Task<Media> GetMedia(string directory) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.Medias.FirstOrDefault(m => string.Equals(m.Directory, directory, StringComparison.OrdinalIgnoreCase));
                }
            });
        }

        public Task<Movie> GetMovie(string directory) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.Movies.FirstOrDefault(m => string.Equals(m.Directory, directory, StringComparison.OrdinalIgnoreCase));
                }
            });
        }

        public Task<TvShow> GetTvShow(string directory) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.TvShows.FirstOrDefault(m => string.Equals(m.Directory, directory, StringComparison.OrdinalIgnoreCase));
                }
            });
        }

        public Task<IDownloadable> GetDownloadable(string path) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    var dir = new DirectoryInfo(path);
                    if (dir.Exists)
                        return context.Movies.FirstOrDefault(m => string.Equals(m.Directory, path, StringComparison.OrdinalIgnoreCase));

                    return context.TvShows.Episodes().FirstOrDefault(d => string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase))
                        ?? context.Movies.FirstOrDefault(d => string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase)) as IDownloadable;
                }
            });
        }

        public Task<Movie> GetMovieByFile(string filePath) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.Movies.FirstOrDefault(m => string.Equals(m.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
                }
            });
        }

        public Task<TvShowEpisode> GetTvShowEpisode(string filePath) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    return context.TvShows.Episodes().FirstOrDefault(e => string.Equals(e.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
                }
            });
        }

        public Task<IEnumerable<Media>> GetMediaList(IEnumerable<string> directories) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    var directoryList = directories.ToList();
                    IEnumerable<Media> results = context.Medias.Where(m => directoryList.Any(d => string.Equals(d, m.Directory, StringComparison.OrdinalIgnoreCase))).ToList();
                    return results;
                }
            });
        }

        public Task<IEnumerable<Media>> GetMedias(IEnumerable<string> imdbIds) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    var imdbIdList = imdbIds.ToList();
                    IEnumerable<Media> results = context.Medias.Where(m => imdbIdList.Any(i => i == m.ImdbId)).ToList();
                    return results;
                }
            });
        }

        public Task<QueryResult<Media>> GetMedias(MediaSearchModel searchModel) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    var q = context.Medias;
                    return FilterMediaQuery(q, searchModel);
                }
            });
        }

        public Task<QueryResult<Movie>> GetMovies(MovieSearchModel searchModel) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    var q = context.Movies;

                    if (searchModel.NotWatched.HasValue)
                        q = q.Where(m => m.IsWatched == !searchModel.NotWatched.Value);

                    if (searchModel.Downloaded.HasValue)
                        q = q.Where(m => string.IsNullOrEmpty(m.FilePath) == !searchModel.Downloaded.Value);

                    if (searchModel.SubtitleDownloaded.HasValue)
                        q = q.Where(m => m.SubtitleDownloaded == searchModel.SubtitleDownloaded.Value);

                    if (searchModel.NotFound.HasValue)
                        q = q.Where(m => m.NotFound == searchModel.NotFound);

                    if (searchModel.SubtitleNotFound.HasValue)
                        q = q.Where(m => m.SubtitleNotFound == searchModel.SubtitleNotFound);

                    return FilterMediaQuery(q, searchModel);
                }
            });
        }

        public Task<QueryResult<TvShow>> GetTvShows(TvShowSearchModel searchModel) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    var currentDate = DateTime.UtcNow.Date;
                    var q = context.TvShows;

                    if (searchModel.NotWatched.HasValue)
                        q = q.Where(t => t.Seasons.Any(s => s.Episodes.Any(e => e.IsWatched == !searchModel.NotWatched && e.AirDate < currentDate)));

                    if (searchModel.Downloaded.HasValue)
                        q = q.Where(t => t.Seasons.Any(s => s.Episodes.Any(e => string.IsNullOrEmpty(e.FilePath) == !searchModel.Downloaded.Value)));

                    if (searchModel.SubtitleDownloaded.HasValue)
                        q = q.Where(t => t.Seasons.Any(s => s.Episodes.Any(e => e.SubtitleDownloaded == searchModel.SubtitleDownloaded.Value)));

                    if (searchModel.NotFound.HasValue)
                        q = q.Where(t => t.Seasons.Any(s => s.Episodes.Any(e => e.NotFound == searchModel.NotFound)));

                    if (searchModel.SubtitleNotFound.HasValue)
                        q = q.Where(t => t.Seasons.Any(s => s.Episodes.Any(e => e.SubtitleNotFound == searchModel.SubtitleNotFound)));

                    return FilterMediaQuery(q, searchModel);
                }
            });
        }

        public Task<QueryResult<Activity>> GetActivities(ActivitySearchModel searchModel) {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    IQueryable<Activity> query = context.Activities.OrderByDescending(a => a.ActivityDate);

                    if (searchModel.NotRead.HasValue)
                        query = searchModel.NotRead.Value ? query.Where(a => !a.IsRead) : query.Where(a => a.IsRead);

                    var inlineCount = query.Count();
                    var take = searchModel.PageSize;
                    var skip = (searchModel.Page - 1) * take;
                    query = query.Skip(skip).Take(take);
                    var results = query
                        .OrderByDescending(a => a.ActivityDate)
                        .ToList();

                    return new QueryResult<Activity>(results, inlineCount);
                }
            });
        }

        public async Task<string> DownloadMovie(Movie movie) {
            await Task.Run(() => _downloadSemaphore.WaitOne());

            try {
                var downloader = Settings.Downloader.SelectedItem;
                if (downloader == null) return string.Empty;

                var downloadKey = await downloader.DownloadMovie(movie.Directory, movie.OriginalTitle, movie.Year, movie.ImdbId, 
                                                                 movie.VideoQuality, movie.ExtraKeywords, movie.ExcludeKeywords);
                Helper.SetDownloadProperties(downloadKey, movie);

                if (!string.IsNullOrEmpty(downloadKey)) {
                    var activity = CreateActivity(string.Format(Resources.MovieDownloadStarted, movie.Title), movie.FilePath);
                    await SaveChanges(new[] { activity }, new[] { movie });
                }
                else await UpdateEntity(movie);

                return downloadKey;
            }
            finally {
                _downloadSemaphore.Release();
            }
        }

        public async Task<string> DownloadTvShowEpisode(TvShowEpisode episode) {
            await Task.Run(() => _downloadSemaphore.WaitOne());

            try {
                var downloader = Settings.Downloader.SelectedItem;
                if (downloader == null) return string.Empty;

                var season = episode.TvShowSeason;
                var show = season.TvShow;
                var directory = Helper.GetTvShowSeasonDirectory(Settings.TvShowSeasonDirectoryTemplate, episode);

                var downloadKey = await downloader.DownloadTvShowEpisode(directory, show.OriginalTitle, season.Season, episode.Episode, episode.Name, show.ImdbId,
                                                                         show.VideoQuality, show.ExtraKeywords, show.ExcludeKeywords);
                Helper.SetDownloadProperties(downloadKey, episode);

                if (!string.IsNullOrEmpty(downloadKey)) {
                    var activity = CreateActivity(string.Format(Resources.TvShowEpisodeDownloadStarted, show.Title, season.Season, episode.Episode), episode.FilePath);
                    await SaveChanges(new[] { activity }, new[] { episode.TvShowSeason.TvShow });
                }
                else await UpdateEntity(episode.TvShowSeason.TvShow);

                return downloadKey;
            }
            finally {
                _downloadSemaphore.Release();
            }
        }

        public Task RefreshDownloaders() {
            var tasks = Settings.Downloader.Items.RunTasks(d => d.Refresh(), _exceptionHandler);
            return Task.WhenAll(tasks);
        }

        public async Task<bool> DownloadSubtitleForMovie(Movie movie) {
            await Task.Run(() => _subtitleSemaphore.WaitOne());

            try {
                var fileInfo = await ValidateSubtitleDownload(movie.FilePath);
                if (fileInfo == null) return false;

                var languages = SubtitleLanguages.ToArray();
                foreach (var subtitleDownloader in Settings.SubtitleDownloaders.SelectedItems) {
                    var result = await subtitleDownloader.DownloadForMovie(movie.OriginalTitle, movie.FilePath, languages, movie.ImdbId);
                    if (!result) continue;

                    Helper.SetSubtitleDownloadProperties(true, movie);
                    var activity = CreateActivity(string.Format(Resources.MovieSubtitleDownloaded, movie.Title), movie.FilePath);
                    OnMovieSubtitleDownloadCompleted(movie);
                    await SaveChanges(new[] { activity }, new[] { movie });
                    return true;
                }

                Helper.SetSubtitleDownloadProperties(false, movie);
                var activity1 = CreateActivity(string.Format(Resources.MovieSubtitleNotFound, movie.Title), string.Empty);
                await SaveChanges(new[] { activity1 }, new[] { movie });
                return false;
            }
            finally {
                _subtitleSemaphore.Release();
            }
        }

        public async Task<bool> DownloadSubtitleForTvShowEpisode(TvShowEpisode episode) {
            await Task.Run(() => _subtitleSemaphore.WaitOne());

            try {
                var season = episode.TvShowSeason;
                var show = season.TvShow;

                var fileInfo = await ValidateSubtitleDownload(episode.FilePath);
                if (fileInfo == null) return false;

                var languages = SubtitleLanguages.ToArray();
                foreach (var subtitleDownloader in Settings.SubtitleDownloaders.SelectedItems) {
                    var result = await subtitleDownloader.DownloadForTvShowEpisode(show.OriginalTitle, season.Season, episode.Episode, episode.FilePath, languages, show.ImdbId);
                    if (!result) continue;

                    Helper.SetSubtitleDownloadProperties(true, episode);
                    var description = string.Format(Resources.TvShowEpisodeSubtitleDownloaded, show.Title, season.Season, episode.Episode);
                    var activity = CreateActivity(description, episode.FilePath);
                    OnTvShowEpisodeSubtitleDownloadCompleted(episode);
                    await SaveChanges(new[] { activity }, new[] { episode.TvShowSeason.TvShow });
                    return true;
                }

                Helper.SetSubtitleDownloadProperties(false, episode);
                var activity1 = CreateActivity(string.Format(Resources.TvShowEpisodeSubtitleNotFound, show.Title, season.Season, episode.Episode), string.Empty);
                await SaveChanges(new[] { activity1 }, new[] { episode.TvShowSeason.TvShow });
                return false;
            }
            finally {
                _subtitleSemaphore.Release();
            }
        }

        public async Task UpdateTvShow(TvShow tvShow) {
            await UpdateMediaInfo(tvShow);
            var update = await GetTvShowUpdate(tvShow);
            if (update != null) {
                tvShow.Update(update);
                await UpdateEntity(tvShow);
            }
        }

        public async Task ExecuteDownloads() {
            var movies = await GetMoviesToDownload();
            await Task.WhenAll(movies.RunTasks(DownloadMovie, _exceptionHandler));

            var episodes = await GetTvShowEpisodesToDownload();
            await Task.WhenAll(episodes.RunTasks(DownloadTvShowEpisode, _exceptionHandler));

            if (SubtitlesEnabled) {
                var moviesSubtitles = await GetMoviesForSubtitleDownload();
                await Task.WhenAll(moviesSubtitles.RunTasks(DownloadSubtitleForMovie, _exceptionHandler));

                var episodesSubtitles = await GetTvShowEpisodesForSubtitleDownload();
                await Task.WhenAll(episodesSubtitles.RunTasks(DownloadSubtitleForTvShowEpisode, _exceptionHandler));
            }

            await Helper.RunTask(RefreshDownloaders, _exceptionHandler);
        }

        public void ExecuteDownloadJob() {
            _scheduler.TriggerJob(new JobKey(DownloadJobName));
        }

        public async Task ExecuteTvShowUpdates() {
            var tvShows = await GetTvshowsToUpdate();

            foreach (var tvShowTmp in tvShows.ToList()) {
                var tvShow = tvShowTmp;
                await Helper.RunTask(() => UpdateTvShow(tvShow), _exceptionHandler);
            }
        }

        public void ExecuteTvShowUpdateJob() {
            _scheduler.TriggerJob(new JobKey(TvShowUpdateJobName));
        }

        public async Task<IEnumerable<IDownloadSearchResult>> SearchForDownload(string searchQuery, VideoQuality videoQuality, string excludeKeywords) {
            await Task.Run(() => _downloadSemaphore.WaitOne());

            try {
                return await Settings.Downloader.SelectedItem.Search(searchQuery, videoQuality, excludeKeywords);
            }
            finally {
                _downloadSemaphore.Release();
            }
        }

        public async Task<string> Download(string directory, IDownloadSearchResult searchResult, IDownloadable downloadable = null) {
            await Task.Run(() => _downloadSemaphore.WaitOne());

            try {
                var downloadKey = await searchResult.Service.Download(directory, searchResult);
                if (downloadable != null)
                    Helper.SetDownloadProperties(downloadKey, downloadable);

                if (!string.IsNullOrEmpty(downloadKey)) {
                    var movie = downloadable as Movie;
                    if (movie != null) {
                        var activity = CreateActivity(string.Format(Resources.MovieDownloadStarted, movie.Title), movie.FilePath);
                        await InsertEntity(activity);
                    }
                    else {
                        var episode = downloadable as TvShowEpisode;
                        if (episode != null) {
                            var season = episode.TvShowSeason;
                            var show = episode.TvShowSeason.TvShow;
                            var description = string.Format(Resources.TvShowEpisodeDownloadStarted, show.Title, season.Season, episode.Episode);
                            var activity = CreateActivity(description, episode.FilePath);
                            await InsertEntity(activity);
                        }
                    }
                }

                return downloadKey;
            }
            finally {
                _downloadSemaphore.Release();
            }
        }

        public async Task<IEnumerable<ISubtitleSearchResult>> SearchForSubtitleDownload(string searchQuery, Language[] languages = null) {
            if (languages == null)
                languages = SubtitleLanguages.ToArray();
            if (!languages.Any()) return Enumerable.Empty<ISubtitleSearchResult>();

            await Task.Run(() => _subtitleSemaphore.WaitOne());

            try {
                var results = new List<ISubtitleSearchResult>();
                await Task.WhenAll(
                    Settings
                        .SubtitleDownloaders
                        .SelectedItems
                        .RunTasks(sd => sd
                            .Search(searchQuery, languages)
                            .ContinueWith(t => results.AddRange(t.Result)),
                    _exceptionHandler)
                );

                return results;
            }
            finally {
                _subtitleSemaphore.Release();
            }
        }

        public async Task<bool> DownloadSubtitle(string filePath, ISubtitleSearchResult searchResult, IDownloadable downloadable) {
            await Task.Run(() => _subtitleSemaphore.WaitOne());

            try {
                var result = await searchResult.Service.Download(filePath, searchResult);
                if (downloadable != null)
                    Helper.SetSubtitleDownloadProperties(result, downloadable);

                if (result) {
                    var movie = downloadable as Movie;
                    if (movie != null) {
                        var activity = CreateActivity(string.Format(Resources.MovieSubtitleDownloaded, movie.Title), movie.FilePath);
                        await InsertEntity(activity);

                        OnMovieSubtitleDownloadCompleted(movie);
                    }
                    else {
                        var episode = downloadable as TvShowEpisode;
                        if (episode != null) {
                            var season = episode.TvShowSeason;
                            var show = episode.TvShowSeason.TvShow;
                            var activity = CreateActivity(string.Format(Resources.TvShowEpisodeSubtitleDownloaded, show.Title, season.Season, episode.Episode), episode.FilePath);
                            await InsertEntity(activity);

                            OnTvShowEpisodeSubtitleDownloadCompleted(episode);
                        }
                    }
                }

                return result;
            }
            finally {
                _subtitleSemaphore.Release();
            }
        }

        public async Task<bool> DownloadSubtitle(string filePath) {
            await Task.Run(() => _subtitleSemaphore.WaitOne());

            try {
                var fileInfo = await ValidateSubtitleDownload(filePath);
                if (fileInfo == null) return false;

                var languages = SubtitleLanguages.ToArray();
                foreach (var subtitleDownloader in Settings.SubtitleDownloaders.SelectedItems) {
                    var result = await subtitleDownloader.Download(filePath, languages);
                    if (!result) continue;

                    return true;
                }

                return false;
            }
            finally {
                _subtitleSemaphore.Release();
            }
        }

        public async Task BackupDatabase(string path) {
            using (var context = _contextFactory.CreateContext()) {
                await context.Backup(path);
            }
        }

        public Task ClearActivities() {
            return Task.Run(() => {
                using (var context = _contextFactory.CreateContext()) {
                    var activities = context.Activities.ToList();
                    activities.ForEach(context.Delete);
                    context.SaveChanges();

                    OnActivitiesChanged(false, true);
                }
            });
        }

        public bool SubtitlesEnabled {
            get { return SubtitleLanguages.Any() && Settings.SubtitleDownloaders.SelectedItems.Any(); }
        }

        public ObservableCollection<string> MediaGenres {
            get { return _mediaGenres; }
        }

        public string MovieDirectory {
            get { return _settings.MovieDirectory.Path; }
        }

        public string TvShowDirectory {
            get { return _settings.TvShowDirectory.Path; }
        }

        public string TvShowSeasonDirectoryTemplate {
            get { return _settings.TvShowSeasonDirectoryTemplate; }
        }

        public Language Language {
            get { return Settings.LanguageSelection.SelectedItem.Item; }
        }

        public IEnumerable<Language> SubtitleLanguages {
            get { return Settings.SubtitleLanguages.SelectedItems.Select(l => l.Item); }
        }

        public IEnumerable<INovaromaService> Services {
            get { return _services; }
        }

        public bool IsInitialized {
            get { return _isInitialized; }
        }

        public event EventHandler<PathInfoEventArgs> DirectoryAdded;
        protected virtual void OnDirectoryAdded(string path) {
            var handler = DirectoryAdded;
            if (handler != null) handler(this, new PathInfoEventArgs(path));
        }

        public event EventHandler<PathRenamedEventArgs> DirectoryRenamed;
        protected virtual void OnDirectoryRenamed(string path, string oldPath) {
            var handler = DirectoryRenamed;
            if (handler != null) handler(this, new PathRenamedEventArgs(path, oldPath));
        }

        public event EventHandler<PathInfoEventArgs> DirectoryDeleted;
        protected virtual void OnDirectoryDeleted(string path) {
            var handler = DirectoryDeleted;
            if (handler != null) handler(this, new PathInfoEventArgs(path));
        }

        public event EventHandler<MovieDownloadCompletedEventArgs> MovieDownloadCompleted;
        protected virtual void OnMovieDownloadCompleted(Movie movie) {
            var handler = MovieDownloadCompleted;
            if (handler != null) handler(this, new MovieDownloadCompletedEventArgs(movie));

            foreach (var downloadEventHandler in Settings.DownloadEventHandlers.SelectedItems)
                downloadEventHandler.MovieDownloaded(movie);
        }

        public event EventHandler<MovieDownloadCompletedEventArgs> MovieSubtitleDownloadCompleted;
        protected virtual void OnMovieSubtitleDownloadCompleted(Movie movie) {
            var handler = MovieSubtitleDownloadCompleted;
            if (handler != null) handler(this, new MovieDownloadCompletedEventArgs(movie));

            foreach (var downloadEventHandler in Settings.DownloadEventHandlers.SelectedItems)
                downloadEventHandler.MovieSubtitleDownloaded(movie);
        }

        public event EventHandler<TvShowEpisodeDownloadCompletedEventArgs> TvShowEpisodeDownloadCompleted;
        protected virtual void OnTvShowEpisodeDownloadCompleted(TvShowEpisode episode) {
            var handler = TvShowEpisodeDownloadCompleted;
            if (handler != null) handler(this, new TvShowEpisodeDownloadCompletedEventArgs(episode));

            foreach (var downloadEventHandler in Settings.DownloadEventHandlers.SelectedItems)
                downloadEventHandler.TvShowEpisodeDownloaded(episode);
        }

        public event EventHandler<TvShowEpisodeDownloadCompletedEventArgs> TvShowEpisodeSubtitleDownloadCompleted;
        protected virtual void OnTvShowEpisodeSubtitleDownloadCompleted(TvShowEpisode episode) {
            var handler = TvShowEpisodeSubtitleDownloadCompleted;
            if (handler != null) handler(this, new TvShowEpisodeDownloadCompletedEventArgs(episode));

            foreach (var downloadEventHandler in Settings.DownloadEventHandlers.SelectedItems)
                downloadEventHandler.TvShowEpisodeSubtitleDownloaded(episode);
        }

        public event EventHandler<EntityContainerChangeEventArgs> MoviesChanged;
        protected virtual void OnMoviesChanged(bool hasModified = true, bool hasAdded = false, bool hasDeleted = false) {
            var handler = MoviesChanged;
            if (handler != null) handler(this, new EntityContainerChangeEventArgs(hasAdded, hasModified, hasDeleted));
        }

        public event EventHandler<EntityContainerChangeEventArgs> TvShowsChanged;
        protected virtual void OnTvShowsChanged(bool hasModified = true, bool hasAdded = false, bool hasDeleted = false) {
            var handler = TvShowsChanged;
            if (handler != null) handler(this, new EntityContainerChangeEventArgs(hasAdded, hasModified, hasDeleted));
        }

        public event EventHandler<EntityContainerChangeEventArgs> ActivitiesChanged;
        protected virtual void OnActivitiesChanged(bool hasAdded = true, bool hasDeleted = false, bool hasModified = false) {
            var handler = ActivitiesChanged;
            if (handler != null) handler(this, new EntityContainerChangeEventArgs(hasAdded, hasModified, hasDeleted));
        }

        public event EventHandler<LanguageChangeEventArgs> LanguageChanged;
        protected virtual void OnLanguageChanged(Language language) {
            var handler = LanguageChanged;
            if (handler != null) handler(this, new LanguageChangeEventArgs(language));
        }

        #endregion

        #region IConfigurable Members

        string IConfigurable.SettingName {
            get { return "novaroma engine"; }
        }

        INotifyPropertyChanged IConfigurable.Settings {
            get { return Settings; }
        }

        string IConfigurable.SerializeSettings() {
            var o = new {
                Language = (int)Settings.LanguageSelection.SelectedItem.Item,
                MovieDirectory = Settings.MovieDirectory.Path,
                TvShowDirectory = Settings.TvShowDirectory.Path,
                Settings.TvShowSeasonDirectoryTemplate,
                Settings.DownloadInterval,
                SubtitleLanguages = Settings.SubtitleLanguages.SelectedItemNames,
                InfoProvider = Settings.InfoProvider.SelectedItemName,
                AdvancedInfoProvider = Settings.AdvancedInfoProvider.SelectedItemName,
                ShowTracker = Settings.ShowTracker.SelectedItemName,
                Downloader = Settings.Downloader.SelectedItemName,
                SubtitleDownloaders = Settings.SubtitleDownloaders.SelectedItemNames,
                DownloadEventHandlers = Settings.DownloadEventHandlers.SelectedItemNames,
                Settings.MakeSpecialFolder,
                Settings.DeleteDirectoriesAlso
            };

            return JsonConvert.SerializeObject(o);
        }

        void IConfigurable.DeserializeSettings(string settings) {
            var o = (JObject)JsonConvert.DeserializeObject(settings);

            var language = o["Language"];
            if (language != null) {
                var languageItem = (Language)Convert.ToInt32(language);
                Settings.LanguageSelection.SelectedItem = Settings.LanguageSelection.Items.First(l => l.Item == languageItem);
            }
            Settings.MovieDirectory.Path = (string)o["MovieDirectory"];
            Settings.TvShowDirectory.Path = (string)o["TvShowDirectory"];
            Settings.TvShowSeasonDirectoryTemplate = (string)o["TvShowSeasonDirectoryTemplate"];
            Settings.DownloadInterval = Convert.ToInt32(o["DownloadInterval"]);
            var subtitleLanguages = (JArray)o["SubtitleLanguages"];
            if (subtitleLanguages != null)
                Settings.SubtitleLanguages.SelectedItemNames = subtitleLanguages.Select(x => x.ToString());
            Settings.InfoProvider.SelectedItemName = (string)o["InfoProvider"];
            Settings.AdvancedInfoProvider.SelectedItemName = (string)o["AdvancedInfoProvider"];
            Settings.ShowTracker.SelectedItemName = (string)o["ShowTracker"];
            Settings.Downloader.SelectedItemName = (string)o["Downloader"];
            var subtitleDownloaders = (JArray)o["SubtitleDownloaders"];
            if (subtitleDownloaders != null)
                Settings.SubtitleDownloaders.SelectedItemNames = subtitleDownloaders.Select(x => x.ToString());
            var downloadEventHandlers = (JArray)o["DownloadEventHandlers"];
            if (downloadEventHandlers != null)
                Settings.DownloadEventHandlers.SelectedItemNames = downloadEventHandlers.Select(x => x.ToString());
            var makeSpecialFolder = o["MakeSpecialFolder"];
            if (makeSpecialFolder != null)
                Settings.MakeSpecialFolder = (bool)makeSpecialFolder;
            var deleteDirectoriesAlso = o["DeleteDirectoriesAlso"];
            if (deleteDirectoriesAlso != null)
                Settings.DeleteDirectoriesAlso = (bool)deleteDirectoriesAlso;
        }

        #endregion
    }
}
