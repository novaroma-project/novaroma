using Novaroma.Interface.Model;

namespace Novaroma.Model {
    
    public class Movie: Media, IDownloadable {
        private bool _backgroundDownload;
        private string _downloadKey;
        private bool _notFound;
        private string _filePath;
        private bool _backgroundSubtitleDownload;
        private bool _subtitleNotFound;
        private bool _subtitleDownloaded;
        private bool _isWatched;

        public Movie() {
            BackgroundDownload = true;
        }

        public bool BackgroundDownload {
            get { return _backgroundDownload; }
            set {
                if (_backgroundDownload == value) return;

                _backgroundDownload = value;
                RaisePropertyChanged("BackgroundDownload");
            }
        }

        public string DownloadKey {
            get { return _downloadKey; }
            set {
                if (_downloadKey == value) return;

                _downloadKey = value;
                RaisePropertyChanged("DownloadKey");
            }
        }

        public bool NotFound {
            get { return _notFound; }
            set {
                if (_notFound == value) return;

                _notFound = value;
                RaisePropertyChanged("NotFound");
            }
        }

        public string FilePath {
            get { return _filePath; }
            set {
                if (_filePath == value) return;

                _filePath = value;
                RaisePropertyChanged("FilePath");
            }
        }

        public bool BackgroundSubtitleDownload {
            get { return _backgroundSubtitleDownload; }
            set {
                if (_backgroundSubtitleDownload == value) return;

                _backgroundSubtitleDownload = value;
                RaisePropertyChanged("BackgroundSubtitleDownload");
            }
        }

        public bool SubtitleNotFound {
            get { return _subtitleNotFound; }
            set {
                if (_subtitleNotFound == value) return;

                _subtitleNotFound = value;
                RaisePropertyChanged("SubtitleNotFound");
            }
        }

        public bool SubtitleDownloaded {
            get { return _subtitleDownloaded; }
            set {
                if (_subtitleDownloaded == value) return;

                _subtitleDownloaded = value;
                RaisePropertyChanged("SubtitleDownloaded");
            }
        }

        public bool IsWatched {
            get { return _isWatched; }
            set {
                if (_isWatched == value) return;

                _isWatched = value;
                RaisePropertyChanged("IsWatched");

                if (_isWatched) {
                    BackgroundDownload = false;
                    BackgroundSubtitleDownload = false;
                }
            }
        }

        protected override void CopyFrom(IEntity entity) {
            var external = Helper.ConvertTo<Movie>(entity);

            CopyFrom(external);

            BackgroundDownload = external.BackgroundDownload;
            DownloadKey = external.DownloadKey;
            NotFound = external.NotFound;
            FilePath = external.FilePath;
            BackgroundSubtitleDownload = external.BackgroundSubtitleDownload;
            SubtitleNotFound = external.SubtitleNotFound;
            SubtitleDownloaded = external.SubtitleDownloaded;
            IsWatched = external.IsWatched;
        }

        #region IDownloadable Members

        string IDownloadable.GetSearchQuery() {
            return Helper.PopulateMovieSearchQuery(this);
        }

        public Media Media {
            get { return this; }
        }

        #endregion
    }
}
