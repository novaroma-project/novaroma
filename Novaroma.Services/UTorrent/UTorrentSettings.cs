using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Novaroma.Interface.Download.Torrent.Provider;
using Novaroma.Interface.Model;
using Novaroma.Properties;

namespace Novaroma.Services.UTorrent {

    public class UTorrentSettings : ModelBase {
        private string _userName = "novaroma";
        private string _password = "421337";
        private int? _port = 8080;
        private readonly SettingMultiSelection<ITorrentMovieProvider> _movieProviderSelection;
        private readonly SettingMultiSelection<ITorrentTvShowProvider> _tvShowProviderSelection;
        private readonly SettingSingleSelection<EnumInfo<VideoQuality>> _defaultMovieVideoQuality;
        private string _defaultMovieExtraKeywords;
        private string _defaultMovieExcludeKeywords;
        private readonly SettingSingleSelection<EnumInfo<VideoQuality>> _defaultTvShowVideoQuality;
        private string _defaultTvShowExtraKeywords;
        private string _defaultTvShowExcludeKeywords;

        public UTorrentSettings(IEnumerable<ITorrentMovieProvider> movieProviders, IEnumerable<ITorrentTvShowProvider> tvShowProviders) {
            _movieProviderSelection = new SettingMultiSelection<ITorrentMovieProvider>(movieProviders);
            _tvShowProviderSelection = new SettingMultiSelection<ITorrentTvShowProvider>(tvShowProviders);
            var videoQualityInfo = Constants.VideoQualityEnumInfo;
            _defaultMovieVideoQuality = new SettingSingleSelection<EnumInfo<VideoQuality>>(videoQualityInfo);
            _defaultTvShowVideoQuality = new SettingSingleSelection<EnumInfo<VideoQuality>>(videoQualityInfo);
        }

        [Display(Name = "UserName", ResourceType = typeof(Resources))]
        public string UserName {
            get { return _userName; }
            set {
                if (_userName == value) return;

                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        [Display(Name = "Password", ResourceType = typeof(Resources))]
        public string Password {
            get { return _password; }
            set {
                if (_password == value) return;

                _password = value;
                RaisePropertyChanged("Password");
            }
        }

        [Display(Name = "Port", ResourceType = typeof(Resources))]
        public int? Port {
            get { return _port; }
            set {
                if (_port == value) return;

                _port = value;
                RaisePropertyChanged("Port");
            }
        }

        [Display(Name = "MovieProviders", ResourceType = typeof(Resources))]
        public SettingMultiSelection<ITorrentMovieProvider> MovieProviderSelection {
            get { return _movieProviderSelection; }
        }

        [Display(Name = "TvShowProviders", ResourceType = typeof(Resources))]
        public SettingMultiSelection<ITorrentTvShowProvider> TvShowProviderSelection {
            get { return _tvShowProviderSelection; }
        }

        [Display(Name = "DefaultMovieVideoQuality", GroupName = "Search", ResourceType = typeof(Resources))]
        public SettingSingleSelection<EnumInfo<VideoQuality>> DefaultMovieVideoQuality {
            get { return _defaultMovieVideoQuality; }
        }

        [Display(Name = "DefaultMovieExtraKeywords", Description = "ExtraKeywordsDescription", GroupName = "Search", ResourceType = typeof(Resources))]
        public string DefaultMovieExtraKeywords {
            get { return _defaultMovieExtraKeywords; }
            set {
                if (_defaultMovieExtraKeywords == value) return;

                _defaultMovieExtraKeywords = value;
                RaisePropertyChanged("DefaultMovieExtraKeywords");
            }
        }

        [Display(Name = "DefaultMovieExcludeKeywords", Description = "ExcludeKeywordsDescription", GroupName = "Search", ResourceType = typeof(Resources))]
        public string DefaultMovieExcludeKeywords {
            get { return _defaultMovieExcludeKeywords; }
            set {
                if (_defaultMovieExcludeKeywords == value) return;

                _defaultMovieExcludeKeywords = value;
                RaisePropertyChanged("DefaultMovieExcludeKeywords");
            }
        }

        [Display(Name = "DefaultTvShowVideoQuality", GroupName = "Search", ResourceType = typeof(Resources))]
        public SettingSingleSelection<EnumInfo<VideoQuality>> DefaultTvShowVideoQuality {
            get { return _defaultTvShowVideoQuality; }
        }

        [Display(Name = "DefaultTvShowExtraKeywords", Description = "ExtraKeywordsDescription", GroupName = "Search", ResourceType = typeof(Resources))]
        public string DefaultTvShowExtraKeywords {
            get { return _defaultTvShowExtraKeywords; }
            set {
                if (_defaultTvShowExtraKeywords == value) return;

                _defaultTvShowExtraKeywords = value;
                RaisePropertyChanged("DefaultTvShowExtraKeywords");
            }
        }

        [Display(Name = "DefaultTvShowExcludeKeywords", Description = "ExcludeKeywordsDescription", GroupName = "Search", ResourceType = typeof(Resources))]
        public string DefaultTvShowExcludeKeywords {
            get { return _defaultTvShowExcludeKeywords; }
            set {
                if (_defaultTvShowExcludeKeywords == value) return;

                _defaultTvShowExcludeKeywords = value;
                RaisePropertyChanged("DefaultTvShowExcludeKeywords");
            }
        }
    }
}
