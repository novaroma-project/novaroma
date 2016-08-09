using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Download;
using Novaroma.Interface.EventHandler;
using Novaroma.Interface.Info;
using Novaroma.Interface.Model;
using Novaroma.Interface.Subtitle;
using Novaroma.Interface.Track;
using Novaroma.Properties;
using Novaroma.Services.UTorrent;

namespace Novaroma.Engine {

    public class EngineSettings : ModelBase {
        private readonly SettingSingleSelection<EnumInfo<Language>> _languageSelection;
        private readonly DirectorySelection _movieDirectory;
        private readonly DirectorySelection _tvShowDirectory;
        private bool _useTorrentDirectory;
        private string _movieFileNameTemplate;
        private string _tvShowSeasonDirectoryTemplate;
        private string _tvShowEpisodeFileNameTemplate;
        private readonly SettingMultiSelection<EnumInfo<Language>> _subtitleLanguages;
        private bool _deleteDirectoriesAlso;
        private readonly SettingSingleSelection<IInfoProvider> _infoProvider;
        private readonly SettingSingleSelection<IAdvancedInfoProvider> _advancedInfoProvider;
        private readonly SettingSingleSelection<IShowTracker> _showTracker;
        private readonly SettingSingleSelection<IDownloader> _downloader;
        private readonly SettingMultiSelection<ISubtitleDownloader> _subtitleDownloaders;
        private readonly SettingMultiSelection<IDownloadEventHandler> _downloadEventHandlers;
        private bool _makeSpecialFolder = true;
        private int _downloadInterval;
        private int _subtitleDownloadInterval;
        private string _deleteExtensions = ".nfo;.srt";
        private int _tvShowUpdateInterval;

        public EngineSettings(IEnumerable<INovaromaService> services) {
            var serviceList = services as IList<INovaromaService> ?? services.ToList();

            var languages = Constants.LanguagesEnumInfo.ToList();
            var languageEnumType = typeof(Language);
            var uiLanguages = languages.Where(l => {
                var lanInfo = Helper.GetMemberAttribute<LanguageInfoAttribute>(languageEnumType, l.Name);
                return lanInfo != null && lanInfo.UISupported;
            });
            _languageSelection = new SettingSingleSelection<EnumInfo<Language>>(uiLanguages);

            _movieDirectory = new DirectorySelection();
            _movieDirectory.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), Resources.Movies);
            _tvShowDirectory = new DirectorySelection();
            _tvShowDirectory.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), Resources.TvShows);
            _tvShowSeasonDirectoryTemplate = Resources.Season + " %season%";

            _subtitleLanguages = new SettingMultiSelection<EnumInfo<Language>>(languages);
            _subtitleLanguages.SelectedItemNames = languages[0].Item == Language.English
                ? Enumerable.Empty<string>()
                : new[] { languages[0].DisplayName };

            _downloadInterval = 10;
            _subtitleDownloadInterval = 60;
            _tvShowUpdateInterval = 4;
            _infoProvider = new SettingSingleSelection<IInfoProvider>(serviceList.OfType<IInfoProvider>());
            _advancedInfoProvider = new SettingSingleSelection<IAdvancedInfoProvider>(serviceList.OfType<IAdvancedInfoProvider>());
            _showTracker = new SettingSingleSelection<IShowTracker>(serviceList.OfType<IShowTracker>());

            var downloaders = new List<IDownloader>();
            IDownloader selectedDownloader = null;
            foreach (var downloader in serviceList.OfType<IDownloader>()) {
                if (downloader.IsAvailable) {
                    selectedDownloader = downloader;
                    downloaders.Insert(0, downloader);
                }
                else {
                    downloaders.Add(downloader);
                    if (selectedDownloader == null && downloader is UTorrentDownloader)
                        selectedDownloader = downloader;
                }
            }
            _downloader = new SettingSingleSelection<IDownloader>(downloaders);
            _downloader.SelectedItem = selectedDownloader;
            _subtitleDownloaders = new SettingMultiSelection<ISubtitleDownloader>(serviceList.OfType<ISubtitleDownloader>());
            _downloadEventHandlers = new SettingMultiSelection<IDownloadEventHandler>(serviceList.OfType<IDownloadEventHandler>());
        }

        [Display(Name = "Language", GroupName = "Main", ResourceType = typeof(Resources))]
        public SettingSingleSelection<EnumInfo<Language>> LanguageSelection {
            get { return _languageSelection; }
        }

        [Display(Name = "MovieDirectory", GroupName = "Main", ResourceType = typeof(Resources))]
        public DirectorySelection MovieDirectory {
            get { return _movieDirectory; }
        }

        [Display(Name = "TvShowDirectory", GroupName = "Main", ResourceType = typeof(Resources))]
        public DirectorySelection TvShowDirectory {
            get { return _tvShowDirectory; }
        }

        [Display(Name = "UseTorrentDirectory", GroupName = "Main", ResourceType = typeof(Resources))]
        public bool UseTorrentDirectory {
            get { return _useTorrentDirectory; }
            set {
                if (_useTorrentDirectory == value) return;

                _useTorrentDirectory = value;
                RaisePropertyChanged("UseTorrentDirectory");
            }
        }

        [Display(Name = "TvShowSeasonDirectoryTemplate", GroupName = "Main", ResourceType = typeof(Resources))]
        public string TvShowSeasonDirectoryTemplate {
            get { return _tvShowSeasonDirectoryTemplate; }
            set {
                if (_tvShowSeasonDirectoryTemplate == value) return;

                _tvShowSeasonDirectoryTemplate = value;
                RaisePropertyChanged("TvShowSeasonDirectoryTemplate");
            }
        }

        [Display(Name = "MovieFileNameTemplate", Description = "MovieFileNameTemplateDescription", GroupName = "Main", ResourceType = typeof(Resources))]
        public string MovieFileNameTemplate {
            get { return _movieFileNameTemplate; }
            set {
                if (_movieFileNameTemplate == value) return;

                _movieFileNameTemplate = value;
                RaisePropertyChanged("MovieFileNameTemplate");
            }
        }

        [Display(Name = "TvShowEpisodeFileNameTemplate", Description = "TvShowEpisodeFileNameTemplateDescription", GroupName = "Main", ResourceType = typeof(Resources))]
        public string TvShowEpisodeFileNameTemplate {
            get { return _tvShowEpisodeFileNameTemplate; }
            set {
                if (_tvShowEpisodeFileNameTemplate == value) return;

                _tvShowEpisodeFileNameTemplate = value;
                RaisePropertyChanged("TvShowEpisodeFileNameTemplate");
            }
        }

        [Display(Name = "SubtitleLanguages", GroupName = "Main", ResourceType = typeof(Resources))]
        public SettingMultiSelection<EnumInfo<Language>> SubtitleLanguages {
            get { return _subtitleLanguages; }
        }

        [Display(Name = "DeleteDirectoriesAlso", Description = "DeleteDirectoriesAlsoDescription", GroupName = "Main", ResourceType = typeof(Resources))]
        public bool DeleteDirectoriesAlso {
            get { return _deleteDirectoriesAlso; }
            set {
                if (_deleteDirectoriesAlso == value) return;

                _deleteDirectoriesAlso = value;
                RaisePropertyChanged("DeleteFoldersAlso");
            }
        }

        [Display(Name = "InfoProvider", GroupName = "Services", ResourceType = typeof(Resources))]
        public SettingSingleSelection<IInfoProvider> InfoProvider {
            get { return _infoProvider; }
        }

        [Display(Name = "AdvancedInfoProvider", GroupName = "Services", ResourceType = typeof(Resources))]
        public SettingSingleSelection<IAdvancedInfoProvider> AdvancedInfoProvider {
            get { return _advancedInfoProvider; }
        }

        [Display(Name = "ShowTracker", GroupName = "Services", ResourceType = typeof(Resources))]
        public SettingSingleSelection<IShowTracker> ShowTracker {
            get { return _showTracker; }
        }

        [Display(Name = "Downloader", GroupName = "Services", ResourceType = typeof(Resources))]
        public SettingSingleSelection<IDownloader> Downloader {
            get { return _downloader; }
        }

        [Display(Name = "SubtitleDownloaders", GroupName = "Services", ResourceType = typeof(Resources))]
        public SettingMultiSelection<ISubtitleDownloader> SubtitleDownloaders {
            get { return _subtitleDownloaders; }
        }

        [Display(Name = "DownloadEventHandlers", GroupName = "Advanced", ResourceType = typeof(Resources))]
        public SettingMultiSelection<IDownloadEventHandler> DownloadEventHandlers {
            get { return _downloadEventHandlers; }
        }

        [Display(Name = "MakeSpecialFolder", Description = "MakeSpecialFolderDescription", GroupName = "Advanced", ResourceType = typeof(Resources))]
        public bool MakeSpecialFolder {
            get { return _makeSpecialFolder; }
            set {
                if (_makeSpecialFolder == value) return;

                _makeSpecialFolder = value;
                RaisePropertyChanged("MakeSpecialFolder");
            }
        }

        [Display(Name = "DownloadInterval", GroupName = "Advanced", ResourceType = typeof(Resources))]
        public int DownloadInterval {
            get { return _downloadInterval; }
            set {
                if (_downloadInterval == value) return;

                _downloadInterval = value;
                RaisePropertyChanged("DownloadInterval");
            }
        }

        [Display(Name = "SubtitleDownloadInterval", GroupName = "Advanced", ResourceType = typeof(Resources))]
        public int SubtitleDownloadInterval {
            get { return _subtitleDownloadInterval; }
            set {
                if (_subtitleDownloadInterval == value) return;

                _subtitleDownloadInterval = value;
                RaisePropertyChanged("SubtitleDownloadInterval");
            }
        }

        [Display(Name = "DeleteExtensions", Description = "DeleteExtensionsDescription", GroupName = "Advanced", ResourceType = typeof(Resources))]
        public string DeleteExtensions {
            get { return _deleteExtensions; }
            set {
                if (_deleteExtensions == value) return;

                _deleteExtensions = value;
                RaisePropertyChanged("DeleteExtensions");
            }
        }

        [Display(Name = "TvShowUpdateInterval", GroupName = "Advanced", ResourceType = typeof(Resources))]
        public int TvShowUpdateInterval {
            get { return _tvShowUpdateInterval; }
            set {
                if (_tvShowUpdateInterval == value) return;

                _tvShowUpdateInterval = value;
                RaisePropertyChanged("TvShowUpdateInterval");
            }
        }
    }
}
