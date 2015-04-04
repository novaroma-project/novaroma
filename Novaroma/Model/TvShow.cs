using System;
using System.Collections.Generic;
using System.Linq;
using Novaroma.Interface.Track;

namespace Novaroma.Model {

    public class TvShow : Media {
        private bool _autoDownload = true;
        private bool _isActive;
        private string _status;
        private ICollection<TvShowSeason> _seasons;
        private DateTime _lastUpdateDate;

        public TvShow() {
            Seasons = new List<TvShowSeason>();
            AutoDownload = true;
        }

        public bool AutoDownload {
            get { return _autoDownload; }
            set {
                if (_autoDownload == value) return;

                _autoDownload = value;
                RaisePropertyChanged("AutoDownload");
            }
        }

        public bool IsActive {
            get { return _isActive; }
            set {
                if (_isActive == value) return;

                _isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        public string Status {
            get { return _status; }
            set {
                if (_status == value) return;

                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        public ICollection<TvShowSeason> Seasons {
            get { return _seasons; }
            set {
                if (Equals(_seasons, value)) return;

                _seasons = value;
                RaisePropertyChanged("Seasons");
            }
        }

        public DateTime LastUpdateDate {
            get { return _lastUpdateDate; }
            set {
                if (_lastUpdateDate == value) return;

                _lastUpdateDate = value;
                RaisePropertyChanged("LastUpdateDate");
            }
        }

        public TvShowEpisode UnseenEpisode {
            get {
                return Seasons.SelectMany(s => s.Episodes).FirstOrDefault(e => !e.IsWatched);
            }
        }

        public bool? AllBackgroundDownload {
            get {
                var downloadCount = Seasons.Count(s => s.AllBackgroundDownload.HasValue && !s.AllBackgroundDownload.Value);
                if (downloadCount == 0) return true;
                if (downloadCount == Seasons.Count) return false;
                return null;
            }
            set {
                Seasons.ToList().ForEach(e => e.AllBackgroundDownload = value.HasValue && value.Value);
                RaisePropertyChanged("AllBackgroundDownload");
            }
        }

        public bool? AllBackgroundSubtitleDownload {
            get {
                var downloadCount = Seasons.Count(s => s.AllBackgroundSubtitleDownload.HasValue && !s.AllBackgroundSubtitleDownload.Value);
                if (downloadCount == 0) return true;
                if (downloadCount == Seasons.Count) return false;
                return null;
            }
            set {
                Seasons.ToList().ForEach(e => e.AllBackgroundSubtitleDownload = value.HasValue && value.Value);
                RaisePropertyChanged("AllBackgroundSubtitleDownload");
            }
        }

        public bool? AllWatched {
            get {
                var watchedCount = Seasons.Count(e => e.AllWatched.HasValue && !e.AllWatched.Value);
                if (watchedCount == 0) return true;
                if (watchedCount == Seasons.Count) return false;
                return null;
            }
            set {
                Seasons.ToList().ForEach(e => e.AllWatched = value.HasValue && value.Value);
                RaisePropertyChanged("AllWatched");
            }
        }

        protected override void RaisePropertyChanged(string propertyName = null) {
            base.RaisePropertyChanged(propertyName);
            base.RaisePropertyChanged("UnseenEpisode");
        }

        public void MergeEpisodes(IEnumerable<ITvShowEpisodeInfo> episodes) {
            episodes = episodes.OrderBy(e => e.Season, new SeasonComparer()).ThenBy(e => e.Episode);

            foreach (var episodeInfo in episodes) {
                var season = Seasons.FirstOrDefault(s => s.Season == episodeInfo.Season);
                if (season == null) {
                    season = new TvShowSeason { TvShowId = Id, TvShow = this, Season = episodeInfo.Season };
                    Seasons.Add(season);
                }

                var episode = season.Episodes.FirstOrDefault(e => e.Episode == episodeInfo.Episode);
                if (episode == null) {
                    episode = new TvShowEpisode { TvShowSeason = season };
                    season.Episodes.Add(episode);
                    episode.Episode = episodeInfo.Episode;
                    episode.BackgroundDownload = AutoDownload;
                }

                episode.AirDate = episodeInfo.AirDate;
                episode.Name = episodeInfo.Name;
                episode.Overview = episodeInfo.Overview;
            }
        }

        public void Update(ITvShowUpdate tvShowUpdate) {
            if (tvShowUpdate == null) return;

            IsActive = tvShowUpdate.IsActive;
            Status = tvShowUpdate.Status;
            LastUpdateDate = DateTime.UtcNow;

            MergeEpisodes(tvShowUpdate.UpdateEpisodes);
        }

        internal void OnSeasonBackgroundDownloadChange() {
            RaisePropertyChanged("AllBackgroundDownload");
        }

        internal void OnSeasonBackgroundSubtitleDownloadChange() {
            RaisePropertyChanged("AllBackgroundSubtitleDownload");
        }

        internal void OnSeasonIsWatchedChange() {
            RaisePropertyChanged("AllWatched");
        }
    }
}
