using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Novaroma.Interface;
using Novaroma.Interface.Info;
using Novaroma.Win.Infrastructure;
using Novaroma.Win.Utilities;

namespace Novaroma.Win.ViewModels.SearchMedia {

    public class AdvancedInfoSearchViewModel : ViewModelBase, IInfoSearchViewModel<IAdvancedInfoSearchResult>, IInfoSearchViewModel<IInfoSearchResult> {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private string _query;
        private readonly string _directory;
        private readonly bool _isParentDirectory;

        private readonly ICommand _searchCommand;
        private readonly ICommand _clearFiltersCommand;
        private IEnumerable<IInfoSearchMediaViewModel<IAdvancedInfoSearchResult>> _results;
        private readonly ObservableCollection<IInfoSearchMediaViewModel<IAdvancedInfoSearchResult>> _observableResults;
        private readonly MultiCheckSelection<IInfoSearchMediaViewModel<IAdvancedInfoSearchResult>> _resultSelections;

        private readonly MultiCheckSelection<EnumInfo<MediaTypes>> _mediaTypes;
        private readonly MultiCheckSelection<string> _genres;
        private IInfoSearchMediaViewModel<IAdvancedInfoSearchResult> _selectedResult;
        private bool _isBusy;
        private bool _searchInitialized;

        private int? _releaseYearStart;
        private int? _releaseYearEnd;
        private float? _ratingMin = 0;
        private float? _ratingMax = 10;
        private int? _voteCountMin;
        private int? _voteCountMax;
        private int? _runtimeMin;
        private int? _runtimeMax;
        private bool _excludeExisting = true;

        public AdvancedInfoSearchViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string searchQuery, string directory, bool isParentDirectory)
                : base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _query = searchQuery;
            _directory = directory;
            _isParentDirectory = isParentDirectory;

            _searchCommand = new RelayCommand(DoSearch, CanSearch);
            _clearFiltersCommand = new RelayCommand(ClearFilters);
            _observableResults = new NovaromaObservableCollection<IInfoSearchMediaViewModel<IAdvancedInfoSearchResult>>();
            _resultSelections = new MultiCheckSelection<IInfoSearchMediaViewModel<IAdvancedInfoSearchResult>>(_observableResults);

            var mediaTypeEnumInfo = Constants.MediaTypesEnumInfo;
            _mediaTypes = new MultiCheckSelection<EnumInfo<MediaTypes>>(mediaTypeEnumInfo.WithoutLast());
            _genres = new MultiCheckSelection<string>(_engine.GetAdvancedInfoProviderGenres());
        }

        public async Task InitSearch() {
            if (_searchInitialized) return;

            if (CanSearch()) {
                _searchInitialized = true;
                await Search();
            }
        }

        public async Task InitFromImdbId(string imdbId) {
            _searchInitialized = true;

            var searchViewModel = new InfoSearchMediaViewModel<IAdvancedInfoSearchResult>(_engine, _exceptionHandler, DialogService, _directory, _isParentDirectory);
            _observableResults.Clear();
            _observableResults.Add(searchViewModel);
            Results = _observableResults;

            await searchViewModel.InitFromImdbId(imdbId);
        }

        private async void DoSearch() {
            await Search();
        }

        private async Task Search() {
            if (IsBusy) return;

            IsBusy = true;
            _searchInitialized = true;
            _observableResults.Clear();
            Results = null;
            SelectedResult = null;

            var mediaTypes = MediaTypes.SelectedItems.Any()
                ? MediaTypes.SelectedItems.Select(mt => mt.Item).Aggregate((mt1, mt2) => mt1 | mt2)
                : Novaroma.MediaTypes.All;

            await Novaroma.Helper.RunTask(async () => {
                await _engine
                    .AdvancedSearchInfo(Query, mediaTypes, ReleaseYearStart, ReleaseYearEnd, RatingMin, RatingMax,
                                        VoteCountMin, VoteCountMax, RuntimeMin, RuntimeMax, Genres.SelectedItems)
                    .ContinueWith(async t => {
                        var results = new List<InfoSearchMediaViewModel<IAdvancedInfoSearchResult>>();
                        t.Result.ToList()
                            .ForEach(r => results.Add(new InfoSearchMediaViewModel<IAdvancedInfoSearchResult>(_engine, _exceptionHandler, DialogService, r, _directory, _isParentDirectory)));

                        if (ExcludeExisting) {
                            var existingMedias = await _engine.GetMedias(results.Select(r => r.SearchResult.ImdbId));
                            results = results.Where(r => existingMedias.All(em => em.ImdbId != r.SearchResult.ImdbId)).ToList();
                        }

                        results.ForEach(_observableResults.Add);
                        Results = _observableResults;

                        Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);

                        if (_results.Any())
                            SelectedResult = _results.First();
                    });
            }, _exceptionHandler);

            IsBusy = false;
        }

        private bool CanSearch() {
            return !IsBusy;
        }

        private void ClearFilters() {
            Query = string.Empty;
            MediaTypes.Selections.ToList().ForEach(s => s.IsSelected = false);
            ReleaseYearStart = null;
            ReleaseYearEnd = null;
            RatingMin = null;
            RatingMax = null;
            VoteCountMin = null;
            VoteCountMax = null;
            RuntimeMin = null;
            RuntimeMax = null;
            ExcludeExisting = true;
            Genres.Selections.ToList().ForEach(s => s.IsSelected = false);
        }

        public ICommand SearchCommand {
            get { return _searchCommand; }
        }

        public ICommand ClearFiltersCommand {
            get { return _clearFiltersCommand; }
        }

        public string Query {
            get { return _query; }
            set {
                if (_query == value) return;

                _query = value;
                RaisePropertyChanged("Query");
            }
        }

        public MultiCheckSelection<EnumInfo<MediaTypes>> MediaTypes {
            get { return _mediaTypes; }
        }

        public int? ReleaseYearStart {
            get { return _releaseYearStart; }
            set {
                if (_releaseYearStart == value) return;

                _releaseYearStart = value;
                RaisePropertyChanged("ReleaseYearStart");
                RaisePropertyChanged("MinimumYear");
            }
        }

        public int? ReleaseYearEnd {
            get { return _releaseYearEnd; }
            set {
                if (_releaseYearEnd == value) return;

                _releaseYearEnd = value;
                RaisePropertyChanged("ReleaseYearEnd");
                RaisePropertyChanged("MaximumYear");
            }
        }

        public float? RatingMin {
            get { return _ratingMin; }
            set {
                if (_ratingMin == value) return;

                _ratingMin = value;
                RaisePropertyChanged("RatingMin");
            }
        }

        public float? RatingMax {
            get { return _ratingMax; }
            set {
                if (_ratingMax == value) return;

                _ratingMax = value;
                RaisePropertyChanged("RatingMax");
            }
        }

        public int? VoteCountMin {
            get { return _voteCountMin; }
            set {
                if (_voteCountMin == value) return;

                _voteCountMin = value;
                RaisePropertyChanged("VoteCountMin");
                RaisePropertyChanged("MinimumVote");
            }
        }

        public int? VoteCountMax {
            get { return _voteCountMax; }
            set {
                if (_voteCountMax == value) return;

                _voteCountMax = value;
                RaisePropertyChanged("VoteCountMax");
                RaisePropertyChanged("MaximumVote");
            }
        }

        public int? RuntimeMin {
            get { return _runtimeMin; }
            set {
                if (_runtimeMin == value) return;

                _runtimeMin = value;
                RaisePropertyChanged("RuntimeMin");
                RaisePropertyChanged("MinimumRuntime");
            }
        }

        public int? RuntimeMax {
            get { return _runtimeMax; }
            set {
                if (_runtimeMax == value) return;

                _runtimeMax = value;
                RaisePropertyChanged("RuntimeMax");
                RaisePropertyChanged("MaximumRuntime");
            }
        }

        public bool ExcludeExisting {
            get { return _excludeExisting; }
            set {
                if (_excludeExisting == value) return;

                _excludeExisting = value;
                RaisePropertyChanged("ExcludeExisting");
            }
        }

        public MultiCheckSelection<string> Genres {
            get { return _genres; }
        }

        public int MinimumYear {
            get { return ReleaseYearStart.HasValue ? ReleaseYearStart.Value : 1886; }
        }

        public int MaximumYear {
            get { return ReleaseYearEnd.HasValue ? ReleaseYearEnd.Value : (DateTime.Now.Year + 1); }
        }

        public int MinimumVote {
            get { return VoteCountMin.HasValue ? VoteCountMin.Value : 0; }
        }

        public int? MaximumVote {
            get { return VoteCountMax.HasValue ? VoteCountMax.Value : (int?)null; }
        }

        public int MinimumRuntime {
            get { return RuntimeMin.HasValue ? RuntimeMin.Value : 0; }
        }

        public int? MaximumRuntime {
            get { return RuntimeMax.HasValue ? RuntimeMax.Value : (int?) null; }
        }

        public IEnumerable<IInfoSearchMediaViewModel<IAdvancedInfoSearchResult>> Results {
            get { return _results; }
            private set {
                if (Equals(_results, value)) return;

                _results = value;
                RaisePropertyChanged("Results");
            }
        }

        public IInfoSearchMediaViewModel<IAdvancedInfoSearchResult> SelectedResult {
            get { return _selectedResult; }
            set {
                if (Equals(_selectedResult, value)) return;

                _selectedResult = value;
                RaisePropertyChanged("SelectedResult");
            }
        }

        public IMultiCheckSelection<IInfoSearchMediaViewModel<IAdvancedInfoSearchResult>> ResultSelections {
            get {
                return _resultSelections;
            }
        }

        public bool IsBusy {
            get { return _isBusy; }
            private set {
                if (_isBusy == value) return;

                _isBusy = value;
                RaisePropertyChanged("IsBusy");
                RaisePropertyChanged("IsNotBusy");
            }
        }

        public bool IsNotBusy {
            get { return !IsBusy; }
        }

        #region IInfoSearchViewModel<IInfoSearchResult> Members

        IEnumerable<IInfoSearchMediaViewModel<IInfoSearchResult>> IInfoSearchViewModel<IInfoSearchResult>.Results {
            get { return Results; }
        }

        IInfoSearchMediaViewModel<IInfoSearchResult> IInfoSearchViewModel<IInfoSearchResult>.SelectedResult {
            get {
                return SelectedResult;
            }
            set {
                SelectedResult = value as IInfoSearchMediaViewModel<IAdvancedInfoSearchResult>;
            }
        }

        IMultiCheckSelection<IInfoSearchMediaViewModel<IInfoSearchResult>> IInfoSearchViewModel<IInfoSearchResult>.ResultSelections {
            get { return ResultSelections; }
        }

        public bool SearchInitialized {
            get { return _searchInitialized; }
        }

        #endregion
    }
}
