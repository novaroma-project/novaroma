using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Download.Torrent;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.Services.Kickass {

    public class KickassTorrentProvider : ITorrentMovieProvider, ITorrentTvShowProvider, IConfigurable {
        private static readonly KickassSettings _settings = new KickassSettings();

        private static Task<byte[]> DownloadTorrent(ITorrentSearchResult searchResult) {
            using (var client = new NovaromaWebClient()) {
                return client.DownloadDataTaskAsync(searchResult.Url);
            }
        }

        public override string ToString() {
            return ServiceName;
        }

        public KickassSettings Settings {
            get { return _settings; }
        }

        #region ITorrentMovieProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
                                                                   string extraKeywords = null, string excludeKeywords = null, ITorrentDownloader service = null) {
            var query = Settings.MovieSearchPattern;

            if (!string.IsNullOrEmpty(imdbId))
                imdbId = imdbId.Replace("tt", "");

            query = Helper.PopulateMovieSearchQuery(query, name, year, imdbId, extraKeywords);
            return Search(query, videoQuality, excludeKeywords, service);
        }

        #endregion

        #region ITorrentTvShowProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
                                                                           string extraKeywords = null, string excludeKeywords = null, ITorrentDownloader service = null) {
            var query = Settings.TvShowEpisodeSearchPattern;
            query = Helper.PopulateTvShowEpisodeSearchQuery(query, name, season, episode, imdbId, extraKeywords);
            return Search(query, videoQuality, excludeKeywords, service);
        }

        #endregion

        #region ITorrentProvider Members

        public async Task<IEnumerable<ITorrentSearchResult>> Search(string search, VideoQuality videoQuality = VideoQuality.Any,
                                                                    string excludeKeywords = null, ITorrentDownloader service = null) {
            if (videoQuality != VideoQuality.Any) {
                switch (videoQuality) {
                    case VideoQuality.P1080:
                        search += " 1080p";
                        break;
                    case VideoQuality.P720:
                        search += " 720p";
                        break;
                }
            }

            var excludeList = string.IsNullOrEmpty(excludeKeywords)
                ? Enumerable.Empty<string>().ToList()
                : excludeKeywords.Split(' ').Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)).Distinct().ToList();

            var results = new List<TorrentSearchResult>();
            var url = Helper.CombineUrls(Settings.BaseUrl, "usearch", search);
            using (var client = new NovaromaWebClient()) {
                string html;
                try {
                    html = await client.DownloadStringTaskAsync(url);
                }
                catch (WebException ex) {
                    var errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.NotFound)
                        return results;

                    throw;
                }

                var document = DocumentBuilder.Html(html);
                var items = document.All
                    .Where(n => n.TagName == "TR" && (n.ClassName == "even" || n.ClassName == "odd"));

                foreach (var item in items) {
                    var tds = item.QuerySelectorAll("td");
                    var divs = tds[0].QuerySelectorAll("div");

                    var torrentDiv = divs[0];
                    var torrentLinks = torrentDiv.QuerySelectorAll("a");
                    var magnetUri = torrentLinks.First(n => n.ClassName == "imagnet icon16").Attributes.First(a => a.Name == "href").Value;
                    var torrentUrl = torrentLinks.First(n => n.ClassName == "idownload icon16").Attributes.First(a => a.Name == "href").Value;

                    var torrentNameDiv = divs[1];
                    var torrentName = torrentNameDiv.QuerySelectorAll("a").First(n => n.ClassName == "cellMainLink").TextContent;
                    if (excludeList.Any(e => torrentName.IndexOf(e, StringComparison.OrdinalIgnoreCase) > 0)) continue;

                    var size = tds[1].TextContent;
                    int? files = null;
                    int filesTmp;
                    var filesStr = tds[2].TextContent;
                    if (int.TryParse(filesStr, out filesTmp))
                        files = filesTmp;
                    var age = tds[3].TextContent;
                    var seed = int.Parse(tds[4].TextContent);
                    var leech = int.Parse(tds[5].TextContent);

                    results.Add(new TorrentSearchResult(service, this, torrentUrl, torrentName, seed, leech, size, files, age, magnetUri, DownloadTorrent));
                }
            }

            return results;
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ServiceNames.Kickass; }
        }

        #endregion

        #region IConfigurable Members

        string IConfigurable.SettingName {
            get { return ServiceName; }
        }

        System.ComponentModel.INotifyPropertyChanged IConfigurable.Settings {
            get { return _settings; }
        }

        string IConfigurable.SerializeSettings() {
            return JsonConvert.SerializeObject(Settings);
        }

        void IConfigurable.DeserializeSettings(string settings) {
            var o = (JObject)JsonConvert.DeserializeObject(settings);
            Settings.BaseUrl = (string)o["BaseUrl"];
            Settings.MovieSearchPattern = (string)o["MovieSearchPattern"];
            Settings.TvShowEpisodeSearchPattern = (string)o["TvShowEpisodeSearchPattern"];
        }

        #endregion
    }
}
