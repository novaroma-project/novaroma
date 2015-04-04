using System.ComponentModel.DataAnnotations;
using Novaroma.Interface.Model;
using Novaroma.Properties;

namespace Novaroma.Services {

    public abstract class TorrentProviderSettingsBase : ModelBase {
        private string _baseUrl;
        private string _movieSearchPattern = Constants.DefaultMovieSearchPattern;
        private string _tvShowEpisodeSearchPattern = Constants.DefaultTvShowEpisodeSearchPattern;

        protected TorrentProviderSettingsBase(string baseUrl) {
            _baseUrl = baseUrl;
        }

        [Display(Name = "BaseUrl", ResourceType = typeof(Resources))]
        public string BaseUrl {
            get { return _baseUrl; }
            set {
                if (_baseUrl == value) return;

                _baseUrl = value;
                RaisePropertyChanged("BaseUrl");
            }
        }

        [Display(Name = "MovieSearchPattern", Description = "MovieSearchPatternDescription", ResourceType = typeof(Resources))]
        public string MovieSearchPattern {
            get { return _movieSearchPattern; }
            set {
                if (_movieSearchPattern == value) return;

                _movieSearchPattern = value;
                RaisePropertyChanged("MovieSearchPattern");
            }
        }

        [Display(Name = "TvShowEpisodeSearchPattern", Description = "TvShowEpisodeSearchPatternDescription", ResourceType = typeof(Resources))]
        public string TvShowEpisodeSearchPattern {
            get { return _tvShowEpisodeSearchPattern; }
            set {
                if (_tvShowEpisodeSearchPattern == value) return;

                _tvShowEpisodeSearchPattern = value;
                RaisePropertyChanged("TvShowEpisodeSearchPattern");
            }
        }
    }
}
