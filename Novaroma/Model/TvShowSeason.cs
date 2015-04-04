using System;
using System.Collections.Generic;
using System.Linq;
using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class TvShowSeason : ModelBase {
        private Guid _tvShowId;
        private TvShow _tvShow;
        private int _season;
        private ICollection<TvShowEpisode> _episodes;

        public TvShowSeason() {
            Episodes = new List<TvShowEpisode>();
        }

        public Guid TvShowId {
            get { return _tvShowId; }
            set {
                if (_tvShowId == value) return;

                _tvShowId = value;
                RaisePropertyChanged("TvShowId");
            }
        }

        public TvShow TvShow {
            get { return _tvShow; }
            set {
                if (_tvShow == value) return;

                _tvShow = value;
                RaisePropertyChanged("TvShow");
            }
        }

        public int Season {
            get { return _season; }
            set {
                if (_season == value) return;

                _season = value;
                RaisePropertyChanged("Season");
            }
        }

        public ICollection<TvShowEpisode> Episodes {
            get { return _episodes; }
            set {
                if (Equals(_episodes, value)) return;

                _episodes = value;
                RaisePropertyChanged("Episodes");
            }
        }

        public bool? AllBackgroundDownload {
            get {
                var downloadCount = Episodes.Count(e => e.BackgroundDownload);
                if (downloadCount == 0) return false;
                if (downloadCount == Episodes.Count) return true;
                return null;
            }
            set {
                Episodes.ToList().ForEach(e => e.BackgroundDownload = value.HasValue && value.Value);
                RaisePropertyChanged("AllBackgroundDownload");
                if (TvShow != null)
                    TvShow.OnSeasonBackgroundDownloadChange();
            }
        }

        public bool? AllBackgroundSubtitleDownload {
            get {
                var downloadCount = Episodes.Count(e => e.BackgroundSubtitleDownload);
                if (downloadCount == 0) return false;
                if (downloadCount == Episodes.Count) return true;
                return null;
            }
            set {
                Episodes.ToList().ForEach(e => e.BackgroundSubtitleDownload = value.HasValue && value.Value);
                RaisePropertyChanged("AllBackgroundSubtitleDownload");
                if (TvShow != null)
                    TvShow.OnSeasonBackgroundSubtitleDownloadChange();
            }
        }

        public bool? AllWatched {
            get {
                var watchedCount = Episodes.Count(e => e.IsWatched);
                if (watchedCount == 0) return false;
                if (watchedCount == Episodes.Count) return true;
                return null;
            }
            set {
                Episodes.ToList().ForEach(e => e.IsWatched = value.HasValue && value.Value);
                RaisePropertyChanged("AllWatched");
                if (TvShow != null) {
                    TvShow.OnSeasonIsWatchedChange();
                    if (value.HasValue && value.Value) {
                        TvShow.OnSeasonBackgroundDownloadChange();
                        TvShow.OnSeasonBackgroundSubtitleDownloadChange();
                    }
                }
            }
        }

        protected override void RaisePropertyChanged(string propertyName = null) {
            base.RaisePropertyChanged(propertyName);

            if (TvShow != null)
                TvShow.IsModified = true;
        }

        internal void OnEpisodeBackgroundDownloadChange() {
            RaisePropertyChanged("AllBackgroundDownload");
            if (TvShow != null)
                TvShow.OnSeasonBackgroundDownloadChange();
        }

        internal void OnEpisodeBackgroundSubtitleDownloadChange() {
            RaisePropertyChanged("AllBackgroundSubtitleDownload");
            if (TvShow != null)
                TvShow.OnSeasonBackgroundSubtitleDownloadChange();
        }

        internal void OnEpisodeIsWatchedChange() {
            RaisePropertyChanged("AllWatched");
            if (TvShow != null)
                TvShow.OnSeasonIsWatchedChange();
        }
    }
}
