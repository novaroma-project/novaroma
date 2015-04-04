using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Interface.Info;
using Novaroma.Model;
using Novaroma.Properties;
using Novaroma.Win.Infrastructure;
using Novaroma.Win.Utilities;

namespace Novaroma.Win.ViewModels {

    public class NewMediaWizardViewModel : ViewModelBase {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private IEnumerable<NewMediaViewModel> _searches;
        private IEnumerable<NewMediaViewModel> _selectedSearches;
        private IEnumerable<IInfoSearchMediaViewModel<IInfoSearchResult>> _editList;
        private IEnumerable<Media> _saveList;
        private int _selectedTabIndex;
        private bool _directorySelectionAvailable;
        private bool _selectionAvailable;
        private bool _searchAvailable;
        private bool _discoverAvailable;
        private string _selectedDirectory;
        private string _lastSelectedDirectory;
        private readonly RelayCommand _movieDownloadCommand;
        private readonly RelayCommand _movieSubtitleDownloadCommand;
        private readonly RelayCommand _movieDiscoverFilesCommand;
        private readonly RelayCommand _movieGoToDirectoryCommand;
        private readonly RelayCommand _tvShowEpisodeDownloadCommand;
        private readonly RelayCommand _tvShowEpisodeSubtitleDownloadCommand;
        private readonly RelayCommand _tvShowDiscoverFilesCommand;
        private readonly RelayCommand _tvShowGoToDirectoryCommand;
        private readonly RelayCommand _tvShowEpisodeDeleteCommand;
        private readonly RelayCommand _tvShowEpisodePlayCommand;
        private readonly RelayCommand _allTvDownloadCheckCommand;
        private readonly RelayCommand _allSeasonDownloadCheckCommand;
        private readonly RelayCommand _episodeDownloadCheckCommand;
        private readonly RelayCommand _movieDownloadCheckCommand;
        private bool _canSave;

        public NewMediaWizardViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService)
            : base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;

            _movieDownloadCommand = new RelayCommand(DoDownloadMovie);
            _movieSubtitleDownloadCommand = new RelayCommand(DoDownloadMovieSubtitle);
            _movieDiscoverFilesCommand = new RelayCommand(DiscoverMovieFiles);
            _movieGoToDirectoryCommand = new RelayCommand(GoToMovieDirectory);
            _tvShowEpisodeDownloadCommand = new RelayCommand(DoDownloadTvShowEpisode);
            _tvShowEpisodeSubtitleDownloadCommand = new RelayCommand(DoDownloadTvShowEpisodeSubtitle);
            _tvShowDiscoverFilesCommand = new RelayCommand(DiscoverTvShowFiles);
            _tvShowGoToDirectoryCommand = new RelayCommand(GoToTvShowDirectory);
            _tvShowEpisodeDeleteCommand = new RelayCommand(DeleteTvShowEpisode, CanDeleteTvShowEpisode);
            _tvShowEpisodePlayCommand = new RelayCommand(PlayTvShowEpisode, CanPlayTvShowEpisode);
            _allTvDownloadCheckCommand = new RelayCommand(o => Helper.AllTvDownloadCheck(o, _engine));
            _allSeasonDownloadCheckCommand = new RelayCommand(o => Helper.AllSeasonDownloadCheck(o, _engine));
            _episodeDownloadCheckCommand = new RelayCommand(o => Helper.EpisodeDownloadCheck(o, _engine));
            _movieDownloadCheckCommand = new RelayCommand(o => Helper.MovieDownloadCheck(o, _engine));
        }

        #region Methods

        public void WatchDirectory(string path = null) {
            DirectorySelectionAvailable = true;
            SelectionAvailable = true;
            SearchAvailable = true;
            SelectedDirectory = path;

            if (!string.IsNullOrEmpty(path))
                SelectedTabIndex = 1;
        }

        public async Task AddFromDirectories(string[] directories) {
            SelectionAvailable = true;
            SearchAvailable = true;

            await AddDirectories(directories);

            SelectedTabIndex = 2;
        }

        public async Task AddFromSearch(string searchQuery, string parentDirectory = null) {
            SelectedTabIndex = 2;
            SearchAvailable = true;

            var search = new NewMediaViewModel(_engine, _exceptionHandler, DialogService);
            var t = search.AddFromSearch(searchQuery, parentDirectory);
            SelectedSearches = Searches = new[] { search };

            await t;
            SubscribeSearches();
        }

        public void AddFromDiscover(string parentDirectory = null) {
            SelectedTabIndex = 3;
            DiscoverAvailable = true;

            var search = new NewMediaViewModel(_engine, _exceptionHandler, DialogService);
            search.AddFromDiscover(parentDirectory);

            Searches = new[] { search };
            SubscribeSearches();
            SelectedSearches = Searches;
        }

        public async Task AddFromImdbId(string imdbId, string directory = null) {
            SelectedTabIndex = 4;

            var search = new NewMediaViewModel(_engine, _exceptionHandler, DialogService);
            var t = search.AddFromImdbId(imdbId, directory);

            Searches = new[] { search };
            SubscribeSearches();
            SelectedSearches = Searches;
            EditList = new[] { search.Search.SelectedResult };

            await t;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private async Task AddDirectories(string[] directories) {
            Searches = null;

            var directoryInfoes = directories.Select(d => new DirectoryInfo(d)).Where(di => di.Exists).ToList();
            var filterDirectories = directoryInfoes.Select(di => di.FullName).ToList();

            var medias = await _engine.GetMediaList(filterDirectories);
            var mediaList = medias.ToList();

            Searches = directoryInfoes
                .Select(di => {
                    var search = new NewMediaViewModel(_engine, _exceptionHandler, DialogService);
                    search.AddFromDirectory(di, mediaList.FirstOrDefault(m => string.Equals(m.Directory, di.FullName, StringComparison.OrdinalIgnoreCase)));
                    return search;
                })
                .ToList();
            SubscribeSearches();

            _lastSelectedDirectory = SelectedDirectory;
        }

        private void SubscribeSearches() {
            Searches.ToList().ForEach(s => s.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "IsSelected")
                    RaisePropertyChanged("AllSelected");
            });

            RaisePropertyChanged("AllSelected");
        }

        private async Task PrepareDirectories() {
            if (!DirectorySelectionAvailable) return;
            if (_lastSelectedDirectory == SelectedDirectory) return;

            if (!Directory.Exists(SelectedDirectory)) {
                Searches = Enumerable.Empty<NewMediaViewModel>();
                return;
            }

            var directories = Directory.GetDirectories(SelectedDirectory).ToArray();
            await AddDirectories(directories);
        }

        private async Task PrepareSearch() {
            await PrepareDirectories();

            if (Searches == null || !Searches.Any())
                SelectedSearches = Enumerable.Empty<NewMediaViewModel>();
            else {
                var selectedList = Searches.Where(s => s.IsSelected).ToList();
                var t = Task.WhenAll(selectedList.RunTasks(s => s.InitSearch(), _exceptionHandler));
                SelectedSearches = selectedList;
                await t;
            }
        }

        private static void PrepareDiscover() {
        }

        private async Task PrepareEdit() {
            await PrepareSearch();

            EditList = SelectedSearches.SelectMany(ss => ss.Search.ResultSelections.SelectedItems).ToList();
            var tasks = EditList.RunTasks(es => es.DownloadMedia(), _exceptionHandler);
            await Task.WhenAll(tasks);
        }

        private async Task PrepareSave() {
            await PrepareEdit();

            SaveList = EditList == null ? Enumerable.Empty<Media>() : EditList.Where(el => el.SearchResult != null && el.Media != null).Select(el => el.Media).ToList();
            CanSave = SaveList.Any() || !string.IsNullOrEmpty(SelectedDirectory);
        }

        public async Task Save() {
            if (!string.IsNullOrEmpty(SelectedDirectory))
                await _engine.WatchDirectory(SelectedDirectory);

            var toDelete = new List<Media>();
            var toInsert = new List<Media>();
            foreach (var newMediaViewModel in SelectedSearches.Where(s => s.Search.ResultSelections.SelectedItems.Any())) {
                var newMedias = newMediaViewModel.Search.ResultSelections.SelectedItems.Select(si => si.Media).Where(m => m != null).ToList();
                if (!newMedias.Any()) continue;

                if (newMediaViewModel.OriginalMedia != null)
                    toDelete.Add(newMediaViewModel.OriginalMedia);

                toInsert.AddRange(newMedias);
            }

            await _engine.SaveChanges(toInsert, null, toDelete);
            _engine.ExecuteDownloadJob();
        }

        private async void TabChanged(int tabIndex) {
            switch (tabIndex) {
                case 1:
                    await PrepareDirectories();
                    break;
                case 2:
                    await PrepareSearch();
                    break;
                case 3:
                    PrepareDiscover();
                    break;
                case 4:
                    await PrepareEdit();
                    break;
                case 5:
                    await PrepareSave();
                    break;
            }
        }

        #region Movie

        private async void DoDownloadMovie(object oMovie) {
            var movie = oMovie as Movie;
            if (movie != null)
                await DownloadMovie(movie);
        }

        private async Task DownloadMovie(Movie movie) {
            await Helper.ManualDownload(_engine, _exceptionHandler, DialogService, movie);
        }

        private async void DoDownloadMovieSubtitle(object oMovie) {
            var movie = oMovie as Movie;
            if (movie != null)
                await DownloadMovieSubtitle(movie);
        }

        private async Task DownloadMovieSubtitle(Movie movie) {
            await Helper.ManualSubtitleDownload(_engine, _exceptionHandler, DialogService, movie);
        }

        private void DiscoverMovieFiles(object oMovie) {
            var movie = oMovie as Movie;
            if (movie != null)
                DiscoverMovieFiles(movie);
        }

        private void DiscoverMovieFiles(Movie movie) {
            Novaroma.Helper.InitMovie(movie, _engine);
        }

        private static void GoToMovieDirectory(object oMovie) {
            var movie = oMovie as Movie;
            if (movie != null
                && !string.IsNullOrEmpty(movie.Directory)
                && Directory.Exists(movie.Directory))
                Process.Start(movie.Directory);
        }

        #endregion

        #region Tv Show

        private async void DoDownloadTvShowEpisode(object oTvShowEpisode) {
            var tvShowEpisode = oTvShowEpisode as TvShowEpisode;
            if (tvShowEpisode == null) return;

            await DownloadTvShowEpisode(tvShowEpisode);
        }

        private async Task DownloadTvShowEpisode(TvShowEpisode tvShowEpisode) {
            await Helper.ManualDownload(_engine, _exceptionHandler, DialogService, tvShowEpisode);
        }

        private async void DoDownloadTvShowEpisodeSubtitle(object oTvShowEpisode) {
            var tvShowEpisode = oTvShowEpisode as TvShowEpisode;
            if (tvShowEpisode == null) return;

            await DownloadTvShowEpisodeSubtitle(tvShowEpisode);
        }

        private async Task DownloadTvShowEpisodeSubtitle(TvShowEpisode tvShowEpisode) {
            await Helper.ManualSubtitleDownload(_engine, _exceptionHandler, DialogService, tvShowEpisode);
        }

        private void DiscoverTvShowFiles(object oTvShow) {
            var tvShow = oTvShow as TvShow;
            if (tvShow == null) return;

            DiscoverTvShowFiles(tvShow);
        }

        private void DiscoverTvShowFiles(TvShow tvShow) {
            Novaroma.Helper.InitTvShow(tvShow, _engine);
        }

        private static void GoToTvShowDirectory(object oTvShow) {
            var tvShow = oTvShow as TvShow;
            if (tvShow == null) return;

            if (!string.IsNullOrEmpty(tvShow.Directory) && Directory.Exists(tvShow.Directory))
                Process.Start(tvShow.Directory);
        }

        private void DeleteTvShowEpisode(object prm) {
            DeleteTvShowEpisode((TvShowEpisode)prm);
        }

        private async void DeleteTvShowEpisode(TvShowEpisode episode) {
            var result = await DialogService.Confirm(Resources.MontyNi, Resources.AreYouSure);
            if (!result) return;

            var fileInfo = new FileInfo(episode.FilePath);

            var subtitleFilePath = Novaroma.Helper.GetSubtitleFilePath(fileInfo);
            if (File.Exists(subtitleFilePath))
                File.Delete(subtitleFilePath);
            fileInfo.Delete();

            episode.FilePath = string.Empty;
            episode.SubtitleDownloaded = false;
        }

        private static bool CanDeleteTvShowEpisode(object prm) {
            return CanDeleteTvShowEpisode(prm as TvShowEpisode);
        }

        private static bool CanDeleteTvShowEpisode(TvShowEpisode episode) {
            return episode != null && !string.IsNullOrEmpty(episode.FilePath) && File.Exists(episode.FilePath);
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

        #endregion

        #endregion

        #region Properties

        public IEnumerable<NewMediaViewModel> Searches {
            get { return _searches; }
            private set {
                if (Equals(_searches, value)) return;

                _searches = value;
                RaisePropertyChanged("Searches");
            }
        }

        public IEnumerable<NewMediaViewModel> SelectedSearches {
            get { return _selectedSearches; }
            private set {
                if (Equals(_selectedSearches, value)) return;

                _selectedSearches = value;
                RaisePropertyChanged("SelectedSearches");
            }
        }

        public IEnumerable<IInfoSearchMediaViewModel<IInfoSearchResult>> EditList {
            get { return _editList; }
            private set {
                if (Equals(_editList, value)) return;

                _editList = value;
                RaisePropertyChanged("EditList");
            }
        }

        public IEnumerable<Media> SaveList {
            get { return _saveList; }
            private set {
                if (Equals(_saveList, value)) return;

                _saveList = value;
                RaisePropertyChanged("SaveList");
            }
        }

        public bool DirectorySelectionAvailable {
            get { return _directorySelectionAvailable; }
            private set {
                if (_directorySelectionAvailable == value) return;

                _directorySelectionAvailable = value;
                RaisePropertyChanged("DirectorySelectionAvailable");
            }
        }

        public bool SelectionAvailable {
            get { return _selectionAvailable; }
            private set {
                if (_selectionAvailable == value) return;

                _selectionAvailable = value;
                RaisePropertyChanged("SelectionAvailable");
            }
        }

        public bool SearchAvailable {
            get { return _searchAvailable; }
            private set {
                if (_searchAvailable == value) return;

                _searchAvailable = value;
                RaisePropertyChanged("SearchAvailable");
            }
        }

        public bool DiscoverAvailable {
            get { return _discoverAvailable; }
            private set {
                if (_discoverAvailable == value) return;

                _discoverAvailable = value;
                RaisePropertyChanged("DiscoverAvailable");
            }
        }

        public bool SearchDiscoverAvailable {
            get { return SearchAvailable || DiscoverAvailable; }
        }

        public bool? AllSelected {
            get {
                if (_searches == null) return false;
                var selectedCount = _searches.Count(s => s.IsSelected);
                if (selectedCount == 0) return false;
                if (selectedCount == _searches.Count()) return true;
                return null;
            }
            set {
                _searches.ToList().ForEach(s => s.IsSelected = value.HasValue && value.Value);
                RaisePropertyChanged("AllSelected");
            }
        }

        public int SelectedTabIndex {
            get { return _selectedTabIndex; }
            set {
                if (_selectedTabIndex == value) return;

                _selectedTabIndex = value;
                RaisePropertyChanged("SelectedTabIndex");

                TabChanged(value);
            }
        }

        public string SelectedDirectory {
            get { return _selectedDirectory; }
            set {
                if (_selectedDirectory == value) return;

                _selectedDirectory = value;
                RaisePropertyChanged("SelectedDirectory");
                RaisePropertyChanged("AddedDirectory");
            }
        }

        public string AddedDirectory {
            get {
                return string.Format(Resources.DirectoryToAdd, SelectedDirectory);
            }
        }

        public bool CanSave {
            get { return _canSave; }
            set {
                _canSave = value;
                RaisePropertyChanged("CanSave");
            }
        }

        #region Commands

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

        public RelayCommand TvShowEpisodeDeleteCommand {
            get { return _tvShowEpisodeDeleteCommand; }
        }

        public RelayCommand TvShowEpisodePlayCommand {
            get { return _tvShowEpisodePlayCommand; }
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

        #endregion
    }
}
