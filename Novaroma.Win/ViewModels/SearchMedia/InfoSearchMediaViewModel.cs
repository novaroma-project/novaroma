using System.IO;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Interface.Info;
using Novaroma.Model;
using Novaroma.Win.Infrastructure;
using Novaroma.Win.Utilities;

namespace Novaroma.Win.ViewModels.SearchMedia {

    public class InfoSearchMediaViewModel<TInfoSearchResult> : ViewModelBase, IInfoSearchMediaViewModel<TInfoSearchResult> where TInfoSearchResult : IInfoSearchResult {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly TInfoSearchResult _searchResult;
        private readonly string _directory;
        private readonly bool _isParentDirectory;
        private Media _media;
        private bool _downloadInitialized;

        public InfoSearchMediaViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, string directory, bool isParentDirectory)
                : base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _directory = directory;
            _isParentDirectory = isParentDirectory;
        }

        public InfoSearchMediaViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, TInfoSearchResult searchResult, string directory, bool isParentDirectory)
                : this(engine, exceptionHandler, dialogService, directory, isParentDirectory) {
            _searchResult = searchResult;
        }

        public async Task InitFromImdbId(string imdbId) {
            _downloadInitialized = true;

            var media = await _engine.GetImdbMedia(imdbId);
            Media = InitMedia(media);
        }

        private Media InitMedia(Media media) {
            if (!string.IsNullOrEmpty(_directory) && !_isParentDirectory)
                media.Directory = _directory;

            var tvShow = media as TvShow;
            if (tvShow != null)
                Novaroma.Helper.InitTvShow(tvShow, _engine);
            else {
                var movie = media as Movie;
                if (movie != null)
                    Novaroma.Helper.InitMovie(movie, _engine);
            }

            return media;
        }

        #region IInfoSearchMedia<IInfoSearchResult> Members

        public TInfoSearchResult SearchResult {
            get { return _searchResult; }
        }

        public Media Media {
            get { return _media; }
            private set {
                if (Equals(_media, value)) return;

                _media = value;
                RaisePropertyChanged("Media");
            }
        }

        public async Task DownloadMedia() {
            if (_downloadInitialized) return;
            if (Equals(SearchResult, null)) return;

            var media = await Novaroma.Helper.RunTask(() => _engine.GetMedia(SearchResult), _exceptionHandler);
            Media = InitMedia(media);

            _downloadInitialized = true;
        }

        #endregion
    }
}
