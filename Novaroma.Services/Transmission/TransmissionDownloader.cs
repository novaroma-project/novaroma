using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                foreach (var completed in completeds) {
                    var sourcePath = completed.DownloadDir;

                    var args = new DownloadCompletedEventArgs(completed.HashString, sourcePath, completed.Files.Select(f => f.Name));
                    OnDownloadCompleted(args);
                    if (args.Found && Settings.DeleteCompletedTorrents)
                        client.RemoveTorrents(new[] {completed.ID}, args.Moved);
                }
            });
        }

        protected virtual Trans.Client CreateClient() {
            if (!Process.GetProcessesByName("transmission-qt").Any()) {
                var registryExePath = Registry.GetValue(@"HKEY_CLASSES_ROOT\transmission-qt\shell\open\command", "", null) as string;
                if (registryExePath != null) {
                    var idx1 = registryExePath.IndexOf('"') + 1;
                    var idx2 = registryExePath.IndexOf('"', idx1);
                    if (idx2 == -1) idx2 = registryExePath.Length;
                    var path = registryExePath.Substring(idx1, idx2 - idx1);

                    Process.Start(path, "/minimized");
                }
            }

            var client = new Trans.Client("http://127.0.0.1:9091/transmission/rpc", Settings.Port.ToString(CultureInfo.InvariantCulture));
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

        protected override string ServiceName {
            get { return ServiceNames.Transmission; }
        }

        protected override IEnumerable<ITorrentMovieProvider> MovieProviders {
            get { return Settings.MovieProviderSelection.SelectedItems; }
        }

        protected override IEnumerable<ITorrentTvShowProvider> TvShowProviders {
            get { return Settings.TvShowProviderSelection.SelectedItems; }
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
            var o = new {
                Settings.UserName,
                Settings.Password,
                Settings.Port,
                Settings.DeleteCompletedTorrents,
                MovieProviders = Settings.MovieProviderSelection.SelectedItemNames,
                TvShowProviders = Settings.TvShowProviderSelection.SelectedItemNames,
                Settings.DefaultMovieExcludeKeywords,
                Settings.DefaultMovieExtraKeywords,
                DefaultMovieVideoQuality = Settings.DefaultMovieVideoQuality.SelectedItem.Name,
                Settings.DefaultTvShowExcludeKeywords,
                Settings.DefaultTvShowExtraKeywords,
                DefaultTvShowVideoQuality = Settings.DefaultTvShowVideoQuality.SelectedItem.Name,
                Settings.DefaultMinSize,
                Settings.DefaultMaxSize
            };
            return JsonConvert.SerializeObject(o);
        }

        public void DeserializeSettings(string settings) {
            var o = (JObject)JsonConvert.DeserializeObject(settings);

            Settings.UserName = o["UserName"].ToString();
            Settings.Password = o["Password"].ToString();
            int port;
            if (Int32.TryParse(o["Port"].ToString(), out port))
                Settings.Port = port;
            var deleteCompletedTorrents = o["DeleteCompletedTorrents"];
            if (deleteCompletedTorrents != null)
                Settings.DeleteCompletedTorrents = (bool)deleteCompletedTorrents;
            var movieProviders = (JArray)o["MovieProviders"];
            Settings.MovieProviderSelection.SelectedItemNames = movieProviders.Select(x => x.ToString());
            var tvShowProviders = (JArray)o["TvShowProviders"];
            Settings.TvShowProviderSelection.SelectedItemNames = tvShowProviders.Select(x => x.ToString());

            var defaultMovieExcludeKeywords = o["DefaultMovieExcludeKeywords"];
            if (defaultMovieExcludeKeywords != null)
                Settings.DefaultMovieExcludeKeywords = (string)defaultMovieExcludeKeywords;
            var defaultMovieExtraKeywords = o["DefaultMovieExtraKeywords"];
            if (defaultMovieExtraKeywords != null)
                Settings.DefaultMovieExtraKeywords = (string)defaultMovieExtraKeywords;
            var defaultMovieVideoQuality = o["DefaultMovieVideoQuality"];
            if (defaultMovieVideoQuality != null) {
                var defaultMovieVideoQualityStr = (string)defaultMovieVideoQuality;
                Settings.DefaultMovieVideoQuality.SelectedItem = Settings.DefaultMovieVideoQuality.Items.First(vq => vq.Name == defaultMovieVideoQualityStr);
            }

            var defaultTvShowExcludeKeywords = o["DefaultTvShowExcludeKeywords"];
            if (defaultTvShowExcludeKeywords != null)
                Settings.DefaultTvShowExcludeKeywords = (string)defaultTvShowExcludeKeywords;
            var defaultTvShowExtraKeywords = o["DefaultTvShowExtraKeywords"];
            if (defaultTvShowExtraKeywords != null)
                Settings.DefaultTvShowExtraKeywords = (string)defaultTvShowExtraKeywords;
            var defaultTvShowVideoQuality = o["DefaultTvShowVideoQuality"];
            if (defaultTvShowVideoQuality != null) {
                var defaultTvShowVideoQualityStr = defaultTvShowVideoQuality.ToString();
                Settings.DefaultTvShowVideoQuality.SelectedItem = Settings.DefaultTvShowVideoQuality.Items.First(vq => vq.Name == defaultTvShowVideoQualityStr);
            }

            Settings.DefaultMinSize = (int?)o["DefaultMinSize"];
            Settings.DefaultMaxSize = (int?)o["DefaultMaxSize"];
        }

        #endregion
    }
}
