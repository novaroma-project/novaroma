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

    public class SimpleInfoSearchViewModel : ViewModelBase, IInfoSearchViewModel<IInfoSearchResult> {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private string _searchQuery;
        private readonly string _directory;
        private readonly bool _isParentDirectory;

        private readonly ICommand _searchCommand;
        private IEnumerable<IInfoSearchMediaViewModel<IInfoSearchResult>> _results;
        private readonly ObservableCollection<IInfoSearchMediaViewModel<IInfoSearchResult>> _observableResults;
        private readonly MultiCheckSelection<IInfoSearchMediaViewModel<IInfoSearchResult>> _resultSelections;

        private IInfoSearchMediaViewModel<IInfoSearchResult> _selectedResult;
        private bool _isBusy;
        private bool _searchInitialized;

        public SimpleInfoSearchViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string searchQuery, string directory, bool isParentDirectory = false) 
                : base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _searchQuery = searchQuery;
            _directory = directory;
            _isParentDirectory = isParentDirectory;

            _searchCommand = new RelayCommand(DoSearch, CanSearch);
            _observableResults = new NovaromaObservableCollection<IInfoSearchMediaViewModel<IInfoSearchResult>>();
            _resultSelections = new MultiCheckSelection<IInfoSearchMediaViewModel<IInfoSearchResult>>(_observableResults);
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

            var searchViewModel = new InfoSearchMediaViewModel<IInfoSearchResult>(_engine, _exceptionHandler, DialogService, _directory, _isParentDirectory);
            _observableResults.Clear();
            _observableResults.Add(searchViewModel);
            Results = _observableResults;
            SelectedResult = searchViewModel;

            await searchViewModel.InitFromImdbId(imdbId);
            _searchQuery = searchViewModel.Media.Title;
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

            await Novaroma.Helper.RunTask(async () => {
                await _engine.SearchInfo(SearchQuery)
                    .ContinueWith(t => {
                        t.Result.ToList()
                            .ForEach(r => _observableResults.Add(new InfoSearchMediaViewModel<IInfoSearchResult>(_engine, _exceptionHandler, DialogService, r, _directory, _isParentDirectory)));
                        Results = _observableResults;

                        Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);

                        if (_results.Any())
                            SelectedResult = _results.First();
                    });
            }, _exceptionHandler);

            IsBusy = false;
        }

        private bool CanSearch() {
            return !IsBusy && SearchQuery != null && SearchQuery.Length > 1;
        }

        public ICommand SearchCommand {
            get { return _searchCommand; }
        }

        public string SearchQuery {
            get { return _searchQuery; }
            set {
                if (_searchQuery == value) return;

                _searchQuery = value;
                RaisePropertyChanged("SearchQuery");
                RaisePropertyChanged("SearchCommand");
            }
        }

        public IEnumerable<IInfoSearchMediaViewModel<IInfoSearchResult>> Results {
            get { return _results; }
            private set {
                if (Equals(_results, value)) return;

                _results = value;
                RaisePropertyChanged("Results");
            }
        }

        public IInfoSearchMediaViewModel<IInfoSearchResult> SelectedResult {
            get { return _selectedResult; }
            set {
                if (Equals(_selectedResult, value)) return;

                foreach (var resultSelection in ResultSelections.Selections)
                    resultSelection.IsSelected = Equals(resultSelection.Item, value);

                _selectedResult = value;
                RaisePropertyChanged("SelectedResult");
            }
        }

        public IMultiCheckSelection<IInfoSearchMediaViewModel<IInfoSearchResult>> ResultSelections {
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

        public bool SearchInitialized {
            get { return _searchInitialized; }
        }
    }
}
