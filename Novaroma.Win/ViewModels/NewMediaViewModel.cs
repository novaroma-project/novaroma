using System.IO;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Interface.Info;
using Novaroma.Model;
using Novaroma.Win.Infrastructure;
using Novaroma.Win.Utilities;
using Novaroma.Win.ViewModels.SearchMedia;

namespace Novaroma.Win.ViewModels {

    public sealed class NewMediaViewModel : ViewModelBase {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;

        private DirectoryInfo _directoryInfo;
        private Media _originalMedia;

        private IInfoSearchViewModel<IInfoSearchResult> _search;
        private bool _isSelected;

        public NewMediaViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService): base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
        }

        public void AddFromDirectory(DirectoryInfo directory, Media originalMedia) {
            _directoryInfo = directory;
            _originalMedia = originalMedia;

            var search = Novaroma.Helper.GetDirectorySearchQuery(directory.Name);
            _search = new SimpleInfoSearchViewModel(_engine, _exceptionHandler, DialogService, search, directory.FullName);

            _isSelected = originalMedia == null;
        }

        public async Task AddFromSearch(string searchQuery, string parentDirectory) {
            _search = new SimpleInfoSearchViewModel(_engine, _exceptionHandler, DialogService, searchQuery, parentDirectory, true);
            await InitSearch();

            _isSelected = true;
        }

        public void AddFromDiscover(string parentDirectory) {
            _search = new AdvancedInfoSearchViewModel(_engine, _exceptionHandler, DialogService, string.Empty, parentDirectory, true);

            _isSelected = true;
        }

        public async Task AddFromImdbId(string imdbId, string directory) {
            _search = new SimpleInfoSearchViewModel(_engine, _exceptionHandler, DialogService, string.Empty, directory);

            _isSelected = true;

            await _search.InitFromImdbId(imdbId);
        }

        public Task InitSearch() {
            return Search.InitSearch();
        }

        public DirectoryInfo Directory {
            get { return _directoryInfo; }
        }

        public Media OriginalMedia {
            get { return _originalMedia; }
        }

        public IInfoSearchViewModel<IInfoSearchResult> Search {
            get { return _search; }
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;

                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
    }
}
