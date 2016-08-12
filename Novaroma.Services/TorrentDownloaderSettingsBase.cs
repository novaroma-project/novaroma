using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Novaroma.Interface.Download.Torrent.Provider;
using Novaroma.Interface.Model;
using Novaroma.Properties;

namespace Novaroma.Services {

    public abstract class TorrentDownloaderSettingsBase : ModelBase {
        private string _userName = "novaroma";
        private string _password = "421337";
        private int? _port = 8080;
        private string _allowedExtensions;
        private string _bannedExtensions;
        private bool _deleteCompletedTorrents = true;
        private readonly SettingMultiSelection<ITorrentMovieProvider> _movieProviderSelection;
        private readonly SettingMultiSelection<ITorrentTvShowProvider> _tvShowProviderSelection;
        private readonly SettingSingleSelection<EnumInfo<VideoQuality>> _defaultMovieVideoQuality;
        private string _defaultMovieExtraKeywords;
        private string _defaultMovieExcludeKeywords;
        private readonly SettingSingleSelection<EnumInfo<VideoQuality>> _defaultTvShowVideoQuality;
        private string _defaultTvShowExtraKeywords;
        private string _defaultTvShowExcludeKeywords;
        private int? _defaultMinSize;
        private int? _defaultMaxSize;
        private int? _defaultMinSeed;
        

        protected TorrentDownloaderSettingsBase(IEnumerable<ITorrentMovieProvider> movieProviders, IEnumerable<ITorrentTvShowProvider> tvShowProviders) {
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

        [Display(Name = "AllowedExtensions", ResourceType = typeof(Resources))]
        public string AllowedExtensions {
            get { return _allowedExtensions; }
            set {
                if (_allowedExtensions == value) return;

                _allowedExtensions = value;
                RaisePropertyChanged("AllowedExtensions");
            }
        }

        [Display(Name = "BannedExtensions", ResourceType = typeof(Resources))]
        public string BannedExtensions {
            get { return _bannedExtensions; }
            set {
                if (_bannedExtensions == value) return;

                _bannedExtensions = value;
                RaisePropertyChanged("AllowedExtensions");
            }
        }

        [Display(Name = "DeleteCompletedTorrents", ResourceType = typeof(Resources))]
        public bool DeleteCompletedTorrents {
            get { return _deleteCompletedTorrents; }
            set {
                if (_deleteCompletedTorrents == value) return;

                _deleteCompletedTorrents = value;
                RaisePropertyChanged("DeleteCompletedTorrents");
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

        [Display(Name = "DefaultMovieVideoQuality", GroupName = "Searching", ResourceType = typeof(Resources))]
        public SettingSingleSelection<EnumInfo<VideoQuality>> DefaultMovieVideoQuality {
            get { return _defaultMovieVideoQuality; }
        }

        [Display(Name = "DefaultMovieExtraKeywords", Description = "ExtraKeywordsDescription", GroupName = "Searching", ResourceType = typeof(Resources))]
        public string DefaultMovieExtraKeywords {
            get { return _defaultMovieExtraKeywords; }
            set {
                if (_defaultMovieExtraKeywords == value) return;

                _defaultMovieExtraKeywords = value;
                RaisePropertyChanged("DefaultMovieExtraKeywords");
            }
        }

        [Display(Name = "DefaultMovieExcludeKeywords", Description = "ExcludeKeywordsDescription", GroupName = "Searching", ResourceType = typeof(Resources))]
        public string DefaultMovieExcludeKeywords {
            get { return _defaultMovieExcludeKeywords; }
            set {
                if (_defaultMovieExcludeKeywords == value) return;

                _defaultMovieExcludeKeywords = value;
                RaisePropertyChanged("DefaultMovieExcludeKeywords");
            }
        }

        [Display(Name = "DefaultTvShowVideoQuality", GroupName = "Searching", ResourceType = typeof(Resources))]
        public SettingSingleSelection<EnumInfo<VideoQuality>> DefaultTvShowVideoQuality {
            get { return _defaultTvShowVideoQuality; }
        }

        [Display(Name = "DefaultTvShowExtraKeywords", Description = "ExtraKeywordsDescription", GroupName = "Searching", ResourceType = typeof(Resources))]
        public string DefaultTvShowExtraKeywords {
            get { return _defaultTvShowExtraKeywords; }
            set {
                if (_defaultTvShowExtraKeywords == value) return;

                _defaultTvShowExtraKeywords = value;
                RaisePropertyChanged("DefaultTvShowExtraKeywords");
            }
        }

        [Display(Name = "DefaultTvShowExcludeKeywords", Description = "ExcludeKeywordsDescription", GroupName = "Searching", ResourceType = typeof(Resources))]
        public string DefaultTvShowExcludeKeywords {
            get { return _defaultTvShowExcludeKeywords; }
            set {
                if (_defaultTvShowExcludeKeywords == value) return;

                _defaultTvShowExcludeKeywords = value;
                RaisePropertyChanged("DefaultTvShowExcludeKeywords");
            }
        }

        [Display(Name = "DefaultMinSize", Description = "MinSizeDescription", GroupName = "Searching", ResourceType = typeof(Resources))]
        public int? DefaultMinSize {
            get { return _defaultMinSize; }
            set {
                if (_defaultMinSize == value) return;

                _defaultMinSize = value;
                RaisePropertyChanged("MinSize");
            }
        }

        [Display(Name = "DefaultMaxSize", Description = "MaxSizeDescription", GroupName = "Searching", ResourceType = typeof(Resources))]
        public int? DefaultMaxSize {
            get { return _defaultMaxSize; }
            set {
                if (_defaultMaxSize == value) return;

                _defaultMaxSize = value;
                RaisePropertyChanged("MaxSize");
            }
        }

        [Display(Name = "DefaultMinSeed", GroupName = "Searching", ResourceType = typeof(Resources))]
        public int? DefaultMinSeed {
            get { return _defaultMinSeed; }
            set {
                if (_defaultMinSeed == value) return;

                _defaultMinSeed = value;
                RaisePropertyChanged("DefaultMinSeed");
            }
        }

        protected override IEnumerable<ValidationResult> Validate() {
            var result = base.Validate() ?? Enumerable.Empty<ValidationResult>();

            if (DefaultMinSize.HasValue && DefaultMaxSize.HasValue) {
                var min = DefaultMinSize.Value;
                var max = DefaultMaxSize.Value;

                if (min > max)
                    result = result.Union(new[] { new ValidationResult(Resources.MinCannotExceedMax) });
            }

            return result;
        }

        protected internal virtual string Serialize() {
            var o = new {
                UserName,
                Password,
                Port,
                DeleteCompletedTorrents,
                MovieProviders = MovieProviderSelection.SelectedItemNames,
                TvShowProviders = TvShowProviderSelection.SelectedItemNames,
                DefaultMovieExcludeKeywords,
                DefaultMovieExtraKeywords,
                DefaultMovieVideoQuality = DefaultMovieVideoQuality.SelectedItem.Name,
                DefaultTvShowExcludeKeywords,
                DefaultTvShowExtraKeywords,
                DefaultTvShowVideoQuality = DefaultTvShowVideoQuality.SelectedItem.Name,
                DefaultMinSize,
                DefaultMaxSize,
                DefaultMinSeed
            };
            return JsonConvert.SerializeObject(o);
        }

        protected internal virtual void Deserialize(string settings) {
            var o = (JObject)JsonConvert.DeserializeObject(settings);

            UserName = o["UserName"].ToString();
            Password = o["Password"].ToString();
            Port = (int?)o["Port"];
            var deleteCompletedTorrents = o["DeleteCompletedTorrents"];
            if (deleteCompletedTorrents != null)
                DeleteCompletedTorrents = (bool)deleteCompletedTorrents;
            var movieProviders = (JArray)o["MovieProviders"];
            MovieProviderSelection.SelectedItemNames = movieProviders.Select(x => x.ToString());
            var tvShowProviders = (JArray)o["TvShowProviders"];
            TvShowProviderSelection.SelectedItemNames = tvShowProviders.Select(x => x.ToString());

            var defaultMovieExcludeKeywords = o["DefaultMovieExcludeKeywords"];
            if (defaultMovieExcludeKeywords != null)
                DefaultMovieExcludeKeywords = (string)defaultMovieExcludeKeywords;
            var defaultMovieExtraKeywords = o["DefaultMovieExtraKeywords"];
            if (defaultMovieExtraKeywords != null)
                DefaultMovieExtraKeywords = (string)defaultMovieExtraKeywords;
            var defaultMovieVideoQuality = o["DefaultMovieVideoQuality"];
            if (defaultMovieVideoQuality != null) {
                var defaultMovieVideoQualityStr = (string)defaultMovieVideoQuality;
                DefaultMovieVideoQuality.SelectedItem = DefaultMovieVideoQuality.Items.First(vq => vq.Name == defaultMovieVideoQualityStr);
            }

            var defaultTvShowExcludeKeywords = o["DefaultTvShowExcludeKeywords"];
            if (defaultTvShowExcludeKeywords != null)
                DefaultTvShowExcludeKeywords = (string)defaultTvShowExcludeKeywords;
            var defaultTvShowExtraKeywords = o["DefaultTvShowExtraKeywords"];
            if (defaultTvShowExtraKeywords != null)
                DefaultTvShowExtraKeywords = (string)defaultTvShowExtraKeywords;
            var defaultTvShowVideoQuality = o["DefaultTvShowVideoQuality"];
            if (defaultTvShowVideoQuality != null) {
                var defaultTvShowVideoQualityStr = defaultTvShowVideoQuality.ToString();
                DefaultTvShowVideoQuality.SelectedItem = DefaultTvShowVideoQuality.Items.First(vq => vq.Name == defaultTvShowVideoQualityStr);
            }

            DefaultMinSize = (int?)o["DefaultMinSize"];
            DefaultMaxSize = (int?)o["DefaultMaxSize"];
            DefaultMinSeed = (int?)o["DefaultMinSeed"];
        }
    }
}
