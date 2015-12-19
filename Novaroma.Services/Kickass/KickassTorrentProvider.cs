using System;
using System.Collections.Generic;
using System.Globalization;
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

        public override string ToString() {
            return ServiceName;
        }

        public KickassSettings Settings {
            get { return _settings; }
        }

        #region ITorrentMovieProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
                                                                   string extraKeywords = null, string excludeKeywords = null, 
                                                                   int? minSize = null, int? maxSize = null, int? minSeed = null, ITorrentDownloader service = null) {
            var query = Settings.MovieSearchPattern;

            if (!string.IsNullOrEmpty(imdbId))
                imdbId = imdbId.Replace("tt", "");

            query = Helper.PopulateMovieSearchQuery(query, name, year, imdbId, extraKeywords);
            query += " category:movies";
            return Search(query, videoQuality, excludeKeywords, minSize, maxSize, minSeed, service);
        }

        #endregion

        #region ITorrentTvShowProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
                                                                           string extraKeywords = null, string excludeKeywords = null,
                                                                           int? minSize = null, int? maxSize = null, int? minSeed = null, ITorrentDownloader service = null) {
            var query = Settings.TvShowEpisodeSearchPattern;
            query = Helper.PopulateTvShowEpisodeSearchQuery(query, name, season, episode, imdbId, extraKeywords);
            query += " category:tv";
            return Search(query, videoQuality, excludeKeywords, minSize, maxSize, minSeed, service);
        }

        #endregion

        #region ITorrentProvider Members

        public async Task<IEnumerable<ITorrentSearchResult>> Search(string search, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null,
                                                                    int? minSize = null, int? maxSize = null, int? minSeed = null, ITorrentDownloader service = null) {
            if (videoQuality != VideoQuality.Any) {
                switch (videoQuality) {
                    case VideoQuality.P720:
                        search += " 720p";
                        break;
                    case VideoQuality.P1080:
                        search += " 1080p";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(excludeKeywords)) {
                excludeKeywords = " " + excludeKeywords;
                search += excludeKeywords.Replace(" ", " -");
            }
            
            var url = Helper.CombineUrls(Settings.BaseUrl, "usearch", search);
            using (var client = new NovaromaWebClient()) {
                string html;
                try {
                    html = await client.DownloadStringTaskAsync(url);
                }
                catch (WebException ex) {
                    var errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.NotFound)
                        return Enumerable.Empty<TorrentSearchResult>();

                    throw;
                }

                var document = DocumentBuilder.Html(html);
                var items = document.All
                    .Where(n => n.TagName == "TR" && (n.ClassName == "even" || n.ClassName == "odd"));

                var results = new List<TorrentSearchResult>();
                foreach (var item in items) {
                    var tds = item.QuerySelectorAll("td");

                    var torrentDiv = tds[0].QuerySelector("div[class='iaconbox center floatright']");
                    var torrentLinks = torrentDiv.QuerySelectorAll("a");
                    var tlc = torrentLinks.Length;
                    var magnetUri = torrentLinks[tlc - 2].Attributes.First(a => a.Name == "href").Value;
                    var torrentUrl = torrentLinks[tlc - 1].Attributes.First(a => a.Name == "href").Value;

                    var torrentNameDiv = tds[0].QuerySelector("div[class='torrentname']");
                    var torrentName = torrentNameDiv.QuerySelectorAll("a").First(n => n.ClassName == "cellMainLink").TextContent;

                    var sizeParts = tds[1].TextContent.Split(' ');
                    var sizeStr = sizeParts[0];
                    var sizeType = sizeParts[1];
                    var size = double.Parse(sizeStr, new NumberFormatInfo {CurrencyDecimalSeparator = "."});
                    if (sizeType == "KB")
                        size = Math.Round(size/1024, 2);
                    else if (sizeType == "GB")
                        size = size*1024;
                    if (minSize.HasValue && size < minSize.Value) continue;
                    if (maxSize.HasValue && size > maxSize.Value) continue;

                    var seed = int.Parse(tds[4].TextContent);
                    if (minSeed.HasValue && seed < minSeed.Value) continue;

                    int? files = null;
                    int filesTmp;
                    var filesStr = tds[2].TextContent;
                    if (int.TryParse(filesStr, out filesTmp))
                        files = filesTmp;
                    var age = tds[3].TextContent;
                    var leech = int.Parse(tds[5].TextContent);

                    results.Add(new TorrentSearchResult(service, this, torrentUrl, torrentName, seed, leech, size, files, age, magnetUri));
                }

                return results;
            }
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
