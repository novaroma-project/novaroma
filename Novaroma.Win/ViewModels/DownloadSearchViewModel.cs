using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Interface.Download;
using Novaroma.Interface.Model;
using Novaroma.Win.Utilities;

namespace Novaroma.Win.ViewModels {

    public class DownloadSearchViewModel : ViewModelBase {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IDownloadable _downloadable;
        private readonly string _directory;
        private readonly RelayCommand _searchCommand;
        private string _searchQuery;
        private VideoQuality _videoQuality;
        private string _excludeKeywords;
        private IEnumerable<IDownloadSearchResult> _results;
        private IDownloadSearchResult _selectedResult;
        private bool _isBusy;

        public DownloadSearchViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, IDownloadable downloadable, string directory)
            : base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _downloadable = downloadable;
            _directory = directory;
            _searchCommand = new RelayCommand(DoSearch);
        }

        public Task InitSearch(string searchQuery, VideoQuality videoQuality, string excludeKeywords) {
            SearchQuery = searchQuery;
            VideoQuality = videoQuality;
            ExcludeKeywords = excludeKeywords;

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
                    _engine.SearchForDownload(SearchQuery, VideoQuality, ExcludeKeywords),
                    _exceptionHandler
                );
                IsBusy = false;
            }
        }

        public async Task Download() {
            if (SelectedResult == null) return;

            await Novaroma.Helper.RunTask(() =>
                _engine.Download(_directory, SelectedResult, _downloadable), _exceptionHandler
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

        public VideoQuality VideoQuality {
            get { return _videoQuality; }
            set {
                if (Equals(_videoQuality, value)) return;

                _videoQuality = value;
                RaisePropertyChanged("VideoQuality");
            }
        }

        public string ExcludeKeywords {
            get { return _excludeKeywords; }
            set {
                if (Equals(_excludeKeywords, value)) return;

                _excludeKeywords = value;
                RaisePropertyChanged("ExcludeKeywords");
            }
        }

        public RelayCommand SearchCommand {
            get { return _searchCommand; }
        }

        public IEnumerable<IDownloadSearchResult> Results {
            get { return _results; }
            set {
                _results = value;
                RaisePropertyChanged("Results");
            }
        }

        public IDownloadSearchResult SelectedResult {
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
