using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Novaroma.Interface;
using Novaroma.Interface.Download;
using Novaroma.Interface.Download.Torrent;
using Novaroma.Interface.Download.Torrent.Provider;
using Transmission.API.RPC.Entity;
using Trans = Transmission.API.RPC;
using Fields = Transmission.API.RPC.Entity.TorrentFields;

namespace Novaroma.Services.Transmission {

    public class TransmissionDownloader : TorrentDownloaderBase, IConfigurable {
        private readonly TransmissionSettings _settings;

        public TransmissionDownloader(IExceptionHandler exceptionHandler, IEnumerable<ITorrentMovieProvider> movieProviders, IEnumerable<ITorrentTvShowProvider> tvShowProviders)
            : base(exceptionHandler) {
            _settings = new TransmissionSettings(movieProviders, tvShowProviders);
        }

        public override Task<string> Download(string path, ITorrentSearchResult searchResult) {
            return Task.Run(() => {
                var client = CreateClient();

                var torrent = new NewTorrent { Filename = searchResult.MagnetUri, DownloadDirectory = path };
                var result = client.AddTorrent(torrent);
                if (!string.IsNullOrEmpty(result.ErrorString)) throw new NovaromaException(result.ErrorString + " ("+ result.Error +")");

                return result.HashString;
            });
        }

        public override Task Refresh() {
            return Task.Run(() => {
                var client = CreateClient();
                var torrents = client.GetTorrents(new[] {Fields.ID, Fields.HASH_STRING, Fields.PERCENT_DONE, Fields.DOWNLOAD_DIR, Fields.FILES});

                var completeds = torrents.Torrents.Where(t => t.PercentDone >= 1);
                foreach (var completedTmp in completeds) {
                    var completed = completedTmp;
                    var sourcePath = completed.DownloadDir;

                    var files = completed.Files.Select(f => Path.GetFileName(Path.Combine(completed.DownloadDir, f.Name)));
                    var args = new DownloadCompletedEventArgs(completed.HashString, sourcePath, files);
                    OnDownloadCompleted(args);
                    if (args.Found && Settings.DeleteCompletedTorrents)
                        client.RemoveTorrents(new[] {completed.ID}, args.Moved);
                }
            });
        }

        private static readonly object _processCheckLocker = new object();
        protected virtual Trans.Client CreateClient() {
            lock (_processCheckLocker) {
                var processes = Process.GetProcesses().Where(p => p.ProcessName.StartsWith("TRANS"));
                if (!Process.GetProcessesByName("TRANSM~1").Any()) {
                    var installPath = InstallPath;
                    if (!string.IsNullOrEmpty(installPath)) {
                        Process.Start(installPath, "--minimized");
                        Thread.Sleep(5000);
                    }
                } 
            }

            var client = new Trans.Client(string.Format("http://127.0.0.1:{0}/transmission/rpc", Settings.Port ?? 9091));
            if (!string.IsNullOrEmpty(Settings.UserName) || !string.IsNullOrEmpty(Settings.Password))
                client.SetAuth(Settings.UserName, Settings.Password);
            return client;
        }

        public override Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year, string imdbId, VideoQuality videoQuality = VideoQuality.Any,
                                                                            string extraKeywords = null, string excludeKeywords = null, int? minSize = null, int? maxSize = null) {
            if (videoQuality == VideoQuality.Any)
                videoQuality = Settings.DefaultMovieVideoQuality.SelectedItem.Item;
            if (string.IsNullOrEmpty(extraKeywords))
                extraKeywords = Settings.DefaultMovieExtraKeywords;
            if (string.IsNullOrEmpty(excludeKeywords))
                excludeKeywords = Settings.DefaultMovieExcludeKeywords;
            if (minSize == null)
                minSize = Settings.DefaultMinSize;
            if (maxSize == null)
                maxSize = Settings.DefaultMaxSize;

            return base.SearchMovie(name, year, imdbId, videoQuality, extraKeywords, excludeKeywords, minSize, maxSize);
        }

        public override Task<IEnumerable<ITorrentSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId,
                                                                                    VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null,
                                                                                    int? minSize = null, int? maxSize = null) {
            if (videoQuality == VideoQuality.Any)
                videoQuality = Settings.DefaultTvShowVideoQuality.SelectedItem.Item;
            if (string.IsNullOrEmpty(extraKeywords))
                extraKeywords = Settings.DefaultTvShowExtraKeywords;
            if (string.IsNullOrEmpty(excludeKeywords))
                excludeKeywords = Settings.DefaultTvShowExcludeKeywords;
            if (minSize == null)
                minSize = Settings.DefaultMinSize;
            if (maxSize == null)
                maxSize = Settings.DefaultMaxSize;

            return base.SearchTvShowEpisode(name, season, episode, episodeName, imdbId, videoQuality, extraKeywords, excludeKeywords, minSize, maxSize);
        }

        public override Task<IEnumerable<ITorrentSearchResult>> Search(string query, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null,
                                                                       int? minSize = null, int? maxSize = null) {
            if (videoQuality == VideoQuality.Any)
                videoQuality = Settings.DefaultTvShowVideoQuality.SelectedItem.Item;
            if (string.IsNullOrEmpty(excludeKeywords))
                excludeKeywords = Settings.DefaultTvShowExcludeKeywords;
            if (minSize == null)
                minSize = Settings.DefaultMinSize;
            if (maxSize == null)
                maxSize = Settings.DefaultMaxSize;

            return base.Search(query, videoQuality, excludeKeywords, minSize, maxSize);
        }

        public override bool IsAvailable {
            get {
                return !string.IsNullOrEmpty(InstallPath);
            }
        }

        protected override string ServiceName {
            get { return ServiceNames.Transmission; }
        }

        protected override IEnumerable<ITorrentMovieProvider> MovieProviders {
            get { return Settings.MovieProviderSelection.SelectedItems; }
        }

        protected override IEnumerable<ITorrentTvShowProvider> TvShowProviders {
            get { return Settings.TvShowProviderSelection.SelectedItems; }
        }

        public string InstallPath {
            get {
                var registryExePath = Registry.GetValue(@"HKEY_CLASSES_ROOT\transmission-qt\shell\open\command", "", null) as string;
                if (registryExePath == null) return string.Empty;

                var idx1 = registryExePath.IndexOf('"') + 1;
                var idx2 = registryExePath.IndexOf('"', idx1);
                if (idx2 == -1) idx2 = registryExePath.Length;
                return registryExePath.Substring(idx1, idx2 - idx1);
            }
        }

        public TransmissionSettings Settings {
            get { return _settings; }
        }

        #region IConfigurable Members

        public string SettingName {
            get { return ServiceName; }
        }

        INotifyPropertyChanged IConfigurable.Settings {
            get { return Settings; }
        }

        public string SerializeSettings() {
            return Settings.Serialize();
        }

        public void DeserializeSettings(string settings) {
            Settings.Deserialize(settings);
        }

        #endregion
    }
}
