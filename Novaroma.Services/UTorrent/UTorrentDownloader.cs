using System;
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
using UTorrent.Api;

namespace Novaroma.Services.UTorrent {

    public class UTorrentDownloader : TorrentDownloaderBase, IConfigurable {
        private readonly UTorrentSettings _settings;

        public UTorrentDownloader(IExceptionHandler exceptionHandler, IEnumerable<ITorrentMovieProvider> movieProviders, IEnumerable<ITorrentTvShowProvider> tvShowProviders)
            : base(exceptionHandler) {
            _settings = new UTorrentSettings(movieProviders, tvShowProviders);
        }

        public override async Task<string> Download(string path, ITorrentSearchResult searchResult) {
            var client = CreateClient();
            var uri = new Uri(searchResult.MagnetUri);

            var result = await client.AddUrlTorrentAsync(uri, searchResult.Name);
            if (result.Error != null) throw result.Error;

            return result.AddedTorrent.Hash;
        }

        public override async Task Refresh() {
            var client = CreateClient();
            var torrents = await client.GetListAsync();

            var completeds = torrents.Result.Torrents.Where(t => t.Downloaded > 0 && t.Remaining == 0);
            foreach (var completed in completeds) {
                var hash = completed.Hash;
                var files = (await client.GetFilesAsync(hash)).Result.Files;
                var fileNames = files.SelectMany(fc => fc.Value.Select(f => f.Name));
                var sourcePath = completed.Path;

                var args = new DownloadCompletedEventArgs(hash, sourcePath, fileNames);
                OnDownloadCompleted(args);
                if (args.Found && Settings.DeleteCompletedTorrents) {
                    await client.DeleteTorrentAsync(hash);
                    if (args.Moved)
                        Helper.DeleteDirectory(sourcePath);
                }
            }
        }

        private static readonly object _processCheckLocker = new object();
        protected virtual UTorrentClient CreateClient() {
            lock (_processCheckLocker) {
                if (!Process.GetProcessesByName("uTorrent").Any() && !Process.GetProcessesByName("BitTorrent").Any()) {
                    var installPath = InstallPath;
                    if (string.IsNullOrEmpty(InstallPath)) {
                        var currentDirectory = Directory.GetCurrentDirectory();
                        installPath = Path.Combine(currentDirectory, "uTorrent\\uTorrent.exe");
                    }

                    Process.Start(installPath, "/minimized");
                    Thread.Sleep(5000);
                } 
            }

            return Settings.Port.HasValue
                ? new UTorrentClient("127.0.0.1", Settings.Port.Value, Settings.UserName, Settings.Password)
                : new UTorrentClient(Settings.UserName, Settings.Password);
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
            get { return ServiceNames.UTorrent; }
        }

        protected override IEnumerable<ITorrentMovieProvider> MovieProviders {
            get { return Settings.MovieProviderSelection.SelectedItems; }
        }

        protected override IEnumerable<ITorrentTvShowProvider> TvShowProviders {
            get { return Settings.TvShowProviderSelection.SelectedItems; }
        }

        public string InstallPath {
            get {
                var registryExePath = (Registry.GetValue(@"HKEY_CLASSES_ROOT\uTorrent\shell\open\command", "", null)
                                      ?? Registry.GetValue(@"HKEY_CLASSES_ROOT\bittorrent\shell\open\command", "", null)) as string; 
                if (registryExePath == null) return string.Empty;

                var idx1 = registryExePath.IndexOf('"') + 1;
                var idx2 = registryExePath.IndexOf('"', idx1);
                if (idx2 == -1) idx2 = registryExePath.Length;
                return registryExePath.Substring(idx1, idx2 - idx1);
            }
        }

        public UTorrentSettings Settings {
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
