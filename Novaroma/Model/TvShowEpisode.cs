using System;
using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class TvShowEpisode : ModelBase, IDownloadable {
        private TvShowSeason _tvShowSeason;
        private int _episode;
        private string _name;
        private string _overview;
        private DateTime? _airDate;
        private bool _backgroundDownload;
        private string _downloadKey;
        private bool _notFound;
        private string _filePath;
        private bool _backgroundSubtitleDownload;
        private bool _subtitleNotFound;
        private bool _subtitleDownloaded;
        private bool _isWatched;

        public TvShowEpisode() {
            BackgroundDownload = true;
        }

        public TvShowSeason TvShowSeason {
            get { return _tvShowSeason; }
            set {
                if (_tvShowSeason == value) return;

                _tvShowSeason = value;
                RaisePropertyChanged("TvShowSeason");
            }
        }

        public int Episode {
            get { return _episode; }
            set {
                if (_episode == value) return;

                _episode = value;
                RaisePropertyChanged("Episode");
            }
        }

        public string Name {
            get { return _name; }
            set {
                if (_name == value) return;

                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string Overview {
            get { return _overview; }
            set {
                if (_overview == value) return;

                _overview = value;
                RaisePropertyChanged("Overview");
            }
        }

        public DateTime? AirDate {
            get { return _airDate; }
            set {
                if (_airDate == value) return;

                _airDate = value;
                RaisePropertyChanged("AirDate");
            }
        }

        public bool BackgroundDownload {
            get { return _backgroundDownload; }
            set {
                if (_backgroundDownload == value) return;

                _backgroundDownload = value;
                RaisePropertyChanged("BackgroundDownload");
                if (TvShowSeason != null)
                    TvShowSeason.OnEpisodeBackgroundDownloadChange();
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
            get {
                return _backgroundSubtitleDownload;
            }
            set {
                if (_backgroundSubtitleDownload == value) return;

                _backgroundSubtitleDownload = value;
                RaisePropertyChanged("BackgroundSubtitleDownload");
                if (TvShowSeason != null)
                    TvShowSeason.OnEpisodeBackgroundSubtitleDownloadChange();
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
                if (TvShowSeason != null)
                    TvShowSeason.OnEpisodeIsWatchedChange();

                if (_isWatched) {
                    BackgroundDownload = false;
                    NotFound = false;
                    BackgroundSubtitleDownload = false;
                    SubtitleNotFound = false;
                }
            }
        }

        protected override void RaisePropertyChanged(string propertyName = null) {
            base.RaisePropertyChanged(propertyName);

            if (TvShowSeason != null && TvShowSeason.TvShow != null)
                TvShowSeason.TvShow.IsModified = true;
        }

        internal void CopyFrom(TvShowEpisode episode) {
            Episode = episode.Episode;
            Name = episode.Name;
            Overview = episode.Overview;
            AirDate = episode.AirDate;
            BackgroundDownload = episode.BackgroundDownload;
            DownloadKey = episode.DownloadKey;
            NotFound = episode.NotFound;
            FilePath = episode.FilePath;
            BackgroundSubtitleDownload = episode.BackgroundSubtitleDownload;
            SubtitleNotFound = episode.SubtitleNotFound;
            SubtitleDownloaded = episode.SubtitleDownloaded;
            IsWatched = episode.IsWatched;
        }

        #region IDownloadable Members

        VideoQuality IDownloadable.VideoQuality {
            get { return TvShowSeason.TvShow.VideoQuality; }
        }

        string IDownloadable.ExtraKeywords {
            get { return TvShowSeason.TvShow.ExtraKeywords; }
        }

        string IDownloadable.ExcludeKeywords {
            get { return TvShowSeason.TvShow.ExcludeKeywords; }
        }

        int? IDownloadable.MinSize {
            get { return TvShowSeason.TvShow.MinSize; }
        }

        int? IDownloadable.MaxSize {
            get { return TvShowSeason.TvShow.MaxSize; }
        }

        string IDownloadable.GetSearchQuery() {
            return Helper.PopulateTvShowEpisodeSearchQuery(this);
        }

        Media IDownloadable.Media {
            get { return TvShowSeason.TvShow; }
        }

        #endregion
    }
}
