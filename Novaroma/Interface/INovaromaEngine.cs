using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Novaroma.Interface.Download;
using Novaroma.Interface.Info;
using Novaroma.Interface.Model;
using Novaroma.Interface.Subtitle;
using Novaroma.Model;
using Novaroma.Model.Search;

namespace Novaroma.Interface {

    // ReSharper disable once UnusedTypeParameter
    public interface INovaromaEngine<TContext> : INovaromaEngine where TContext : INovaromaContext {
    }

    public interface INovaromaEngine: IConfigurable {
        Task<DirectoryWatchStatus> GetDirectoryWatchStatus(string directory);
        Task WatchDirectory(string directory);
        Task StopWatching(string directory);

        Task<IEnumerable<IInfoSearchResult>> SearchInfo(string query);
        Task<IMovieInfo> GetMovieInfo(IInfoSearchResult searchResult);
        Task<ITvShowInfo> GetTvShowInfo(IInfoSearchResult searchResult);
        IEnumerable<string> GetAdvancedInfoProviderGenres();
        Task<IEnumerable<IAdvancedInfoSearchResult>> AdvancedSearchInfo(
            string query, MediaTypes mediaTypes = MediaTypes.All, int? releaseYearStart = null, int? releaseYearEnd = null,
            float? ratingMin = null, float? ratingMax = null, int? numberOfVotesMin = null, int? numberOfVotesMax = null,
            int? runtimeMin = null, int? runtimeMax = null, IEnumerable<string> genres = null);

        Task<IDictionary<string, object>> ConvertImdbId(Media media);

        Task<Media> GetMedia(IInfoSearchResult searchResult);
        Task<Media> GetImdbMedia(string imdbId);
        Task UpdateMediaInfo(Media media);

        Task InsertEntity(IEntity entity);
        Task UpdateEntity(IEntity entity);
        Task DeleteEntity(IEntity entity);
        Task SaveChanges(IEnumerable<IEntity> add, IEnumerable<IEntity> update, IEnumerable<IEntity> delete = null);
        Task SaveSettings(string settingName, string settingsJson);
        Task<string> LoadSettings(string settingName);

        Task<IEnumerable<ScriptService>> GetScriptServices();

        Task<Media> GetMedia(string directory);
        Task<Movie> GetMovie(string directory);
        Task<TvShow> GetTvShow(string directory);
        Task<IDownloadable> GetDownloadable(string path);
        Task<Movie> GetMovieByFile(string filePath);
        Task<TvShowEpisode> GetTvShowEpisode(string filePath);
        Task<IEnumerable<Media>> GetMediaList(IEnumerable<string> directories);
        Task<IEnumerable<Media>> GetMedias(IEnumerable<string> imdbIds);
        Task<QueryResult<Media>> GetMedias(MediaSearchModel searchModel);
        Task<QueryResult<Movie>> GetMovies(MovieSearchModel searchModel);
        Task<QueryResult<TvShow>> GetTvShows(TvShowSearchModel searchModel);
        Task<QueryResult<Activity>> GetActivities(ActivitySearchModel searchModel);

        Task<string> DownloadMovie(Movie movie);
        Task<string> DownloadTvShowEpisode(TvShowEpisode episode);
        Task RefreshDownloaders();

        Task<bool> DownloadSubtitleForMovie(Movie movie);
        Task<bool> DownloadSubtitleForTvShowEpisode(TvShowEpisode episode);

        Task UpdateTvShow(TvShow tvShow);

        Task ExecuteDownloads();
        void ExecuteDownloadJob();

        Task ExecuteSubtitleDownloads();
        void ExecuteSubtitleDownloadJob();

        Task ExecuteTvShowUpdates();
        void ExecuteTvShowUpdateJob();

        Task<IEnumerable<IDownloadSearchResult>> SearchForDownload(string searchQuery, VideoQuality videoQuality = VideoQuality.Any, 
                                                                   string excludeKeywords = null, int? minSize = null, int? maxSize = null);
        Task<string> Download(string directory, IDownloadSearchResult searchResult, IDownloadable downloadable = null);

        Task<IEnumerable<ISubtitleSearchResult>> SearchForSubtitleDownload(string searchQuery, Language[] languages = null);
        Task<bool> DownloadSubtitle(string filePath, ISubtitleSearchResult searchResult, IDownloadable downloadable);
        Task<bool> DownloadSubtitle(string filePath);

        Task BackupDatabase(string path);
        Task ClearActivities();

        bool SubtitlesNeeded(Language? videoLanguage);
        bool SubtitlesEnabled { get; }
        
        ObservableCollection<string> MediaGenres { get; }
        string MovieDirectory { get; }
        string TvShowDirectory { get; }
        string TvShowSeasonDirectoryTemplate { get; }
        Language Language { get; }
        IEnumerable<Language> SubtitleLanguages { get; }
        IEnumerable<INovaromaService> Services { get; }
        bool IsInitialized { get; }

        event EventHandler<PathInfoEventArgs> DirectoryAdded;

        event EventHandler<PathRenamedEventArgs> DirectoryRenamed;

        event EventHandler<PathInfoEventArgs> DirectoryDeleted;

        event EventHandler<MovieDownloadCompletedEventArgs> MovieDownloadCompleted;

        event EventHandler<MovieDownloadCompletedEventArgs> MovieSubtitleDownloadCompleted;

        event EventHandler<TvShowEpisodeDownloadCompletedEventArgs> TvShowEpisodeDownloadCompleted;

        event EventHandler<TvShowEpisodeDownloadCompletedEventArgs> TvShowEpisodeSubtitleDownloadCompleted;

        event EventHandler<EntityContainerChangeEventArgs> MoviesChanged;

        event EventHandler<EntityContainerChangeEventArgs> TvShowsChanged;

        event EventHandler<EntityContainerChangeEventArgs> ActivitiesChanged;

        event EventHandler<LanguageChangeEventArgs> LanguageChanged;
    }

    public class PathInfoEventArgs : EventArgs {
        private readonly string _path;

        public PathInfoEventArgs(string path) {
            _path = path;
        }

        public string Path {
            get { return _path; }
        }
    }

    public class PathRenamedEventArgs : EventArgs {
        private readonly string _path;
        private readonly string _oldPath;

        public PathRenamedEventArgs() {
        }

        public PathRenamedEventArgs(string path, string oldPath) {
            _path = path;
            _oldPath = oldPath;
        }

        public string Path {
            get { return _path; }
        }

        public string OldPath {
            get { return _oldPath; }
        }
    }

    public class MovieDownloadCompletedEventArgs : EventArgs {
        private readonly Movie _movie;

        public MovieDownloadCompletedEventArgs(Movie movie) {
            _movie = movie;
        }

        public Movie Movie {
            get { return _movie; }
        }
    }

    public class TvShowEpisodeDownloadCompletedEventArgs : EventArgs {
        private readonly TvShowEpisode _tvShowEpisode;

        public TvShowEpisodeDownloadCompletedEventArgs(TvShowEpisode tvShowEpisode) {
            _tvShowEpisode = tvShowEpisode;
        }

        public TvShowEpisode TvShowEpisode {
            get { return _tvShowEpisode; }
        }
    }

    public class EntityContainerChangeEventArgs : EventArgs {
        private readonly bool _hasAdded;
        private readonly bool _hasModified;
        private readonly bool _hasDeleted;

        public EntityContainerChangeEventArgs(bool hasAdded, bool hasModified, bool hasDeleted) {
            _hasAdded = hasAdded;
            _hasModified = hasModified;
            _hasDeleted = hasDeleted;
        }

        public bool HasAdded {
            get { return _hasAdded; }
        }

        public bool HasModified {
            get { return _hasModified; }
        }

        public bool HasDeleted {
            get { return _hasDeleted; }
        }
    }

    public class LanguageChangeEventArgs : EventArgs {
        private readonly Language _language;

        public LanguageChangeEventArgs(Language language) {
            _language = language;
        }

        public Language Language {
            get { return _language; }
        }
    }
}
