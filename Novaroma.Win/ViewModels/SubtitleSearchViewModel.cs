using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Interface.Model;
using Novaroma.Interface.Subtitle;
using Novaroma.Win.Utilities;

namespace Novaroma.Win.ViewModels {

    public class SubtitleSearchViewModel : ViewModelBase {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IDownloadable _downloadable;
        private readonly FileInfo _fileInfo;
        private readonly MultiCheckSelection<EnumInfo<Language>> _subtitleLanguages;
        private readonly RelayCommand _searchCommand;
        private string _searchQuery;
        private IEnumerable<ISubtitleSearchResult> _results;
        private ISubtitleSearchResult _selectedResult;
        private bool _isBusy;

        public SubtitleSearchViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, IDownloadable downloadable, FileInfo fileInfo)
            : base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _downloadable = downloadable;
            _fileInfo = fileInfo;
            _subtitleLanguages = new MultiCheckSelection<EnumInfo<Language>>(Constants.LanguagesEnumInfo);
            foreach (var subtitleLanguage in engine.SubtitleLanguages)
                _subtitleLanguages.Selections.First(s => s.Item.Item == subtitleLanguage).IsSelected = true;
            _searchCommand = new RelayCommand(DoSearch, CanSearch);
        }

        public Task InitSearch(string searchQuery) {
            SearchQuery = searchQuery;

            return Search();
        }

        private async void DoSearch() {
            await Search();
        }

        private async Task Search() {
            Results = null;
            if (!string.IsNullOrWhiteSpace(SearchQuery)) {
                IsBusy = true;
                Results = await Novaroma.Helper.RunTask(() =>
                    _engine.SearchForSubtitleDownload(SearchQuery, SubtitleLanguages.SelectedItems.Select(si => si.Item).ToArray()),
                    _exceptionHandler
                );
                IsBusy = false;
            }
        }

        private bool CanSearch() {
            return !IsBusy;
        }

        public async Task Download() {
            if (SelectedResult == null) return;

            await Novaroma.Helper.RunTask(() =>
                _engine.DownloadSubtitle(_fileInfo.FullName, SelectedResult, _downloadable), _exceptionHandler
            );
        }

        public string SearchQuery {
            get { return _searchQuery; }
            set {
                if (Equals(_searchQuery, value)) return;

                _searchQuery = value;
                RaisePropertyChanged("SearchQuery");
            }
        }

        public MultiCheckSelection<EnumInfo<Language>> SubtitleLanguages {
            get { return _subtitleLanguages; }
        }

        public RelayCommand SearchCommand {
            get { return _searchCommand; }
        }

        public IEnumerable<ISubtitleSearchResult> Results {
            get { return _results; }
            set {
                if (Equals(_results, value)) return;

                _results = value;
                RaisePropertyChanged("Results");
            }
        }

        public ISubtitleSearchResult SelectedResult {
            get { return _selectedResult; }
            set {
                if (Equals(_selectedResult, value)) return;

                _selectedResult = value;
                RaisePropertyChanged("SelectedResult");
            }
        }

        public bool IsBusy {
            get { return _isBusy; }
            set {
                _isBusy = value;

                RaisePropertyChanged("IsBusy");
            }
        }
    }
}
