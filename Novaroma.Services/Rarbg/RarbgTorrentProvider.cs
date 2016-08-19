using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Download.Torrent;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.Services.Rarbg {
    public class RarbgTorrentProvider : ITorrentMovieProvider, ITorrentTvShowProvider, IConfigurable {
        private readonly RarbgSettings _settings = new RarbgSettings();

        public override string ToString() {
            return ServiceName;
        }

        public RarbgSettings Settings {
            get { return _settings; }
        }

        #region ITorrentMovieProvider Members

        public async Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
                                                                         string extraKeywords = null, string excludeKeywords = null,
                                                                         int? minSize = null, int? maxSize = null, int? minSeed = null, ITorrentDownloader service = null) {
            var query = Settings.MovieSearchPattern;

            IEnumerable<ITorrentSearchResult> results;
            if (query == RarbgSettings.ImdbSearchQuery)
                results = await Search(imdbId, VideoQuality.Any, excludeKeywords, minSize, maxSize, minSeed, service);
            else {
                query = Helper.PopulateMovieSearchQuery(query, name, year, imdbId, extraKeywords);
                results = await Search(query, videoQuality, excludeKeywords, minSize, maxSize, minSeed, service);
            }

            if (query == RarbgSettings.ImdbSearchQuery) {
                string filter;
                switch (videoQuality) {
                    case VideoQuality.P720:
                        filter = "720p";
                        break;
                    case VideoQuality.P1080:
                        filter = "1080p";
                        break;
                    default:
                        filter = string.Empty;
                        break;
                }
                if (extraKeywords != null)
                    filter += " " + extraKeywords;

                if (filter != string.Empty) {
                    var filters = filter.Split(' ');
                    results = results.Where(r => filters.All(e => r.Name.Contains(e)));
                }
            }

            return results;
        }

        #endregion

        #region ITorrentTvShowProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId = null,
                                                                           VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null,
                                                                           int? minSize = null, int? maxSize = null, int? minSeed = null, ITorrentDownloader service = null) {
            var query = Settings.TvShowEpisodeSearchPattern;
            query = Helper.PopulateTvShowEpisodeSearchQuery(query, name, season, episode, imdbId, extraKeywords);
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

            var excludeList = string.IsNullOrEmpty(excludeKeywords)
                ? Enumerable.Empty<string>().ToList()
                : excludeKeywords.Split(' ').Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)).Distinct().ToList();

            var results = new List<TorrentSearchResult>();
            var url = Helper.CombineUrls(Settings.BaseUrl, "search.php?search=", search);
            using (var client = new NovaromaWebClient()) {
                var html = await client.DownloadStringTaskAsync(url);

                var document = DocumentBuilder.Html(html);
                var items = document.QuerySelectorAll("table[class='lista2t'] tr[class='lista2']");

                foreach (var item in items) {
                    var tds = item.QuerySelectorAll("td");

                    var mainTd = tds[1];
                    var anchor = mainTd.QuerySelector("a");
                    var torrentUrl = Helper.CombineUrls(Settings.BaseUrl, anchor.Attributes.First(a => a.Name == "href").Value);
                    var torrentName = anchor.TextContent.Trim();
                    if (excludeList.Any(e => torrentName.IndexOf(e, StringComparison.OrdinalIgnoreCase) > 0)) continue;

                    var detailHtml = await client.DownloadStringTaskAsync(torrentUrl);
                    var detailDocument = DocumentBuilder.Html(detailHtml);
                    var detailTr = detailDocument.QuerySelector("table[class='lista'] tr");
                    var detailMagnet = detailTr.QuerySelector("td[class='lista'] a");

                    var magnetUri = detailMagnet.Attributes.First(a => a.Name == "href").Value;

                    var addedDateString = tds[2].TextContent.Trim();
                    var age = string.Empty;
                    if (!string.IsNullOrEmpty(addedDateString)) {
                        var addedDate = Convert.ToDateTime(addedDateString);
                        age = DateTime.Now.Subtract(addedDate).Days.ToString();
                    }

                    var sizeParts = tds[3].TextContent.Trim().Split(' ');
                    var sizeStr = sizeParts[0];
                    var sizeType = sizeParts[1];
                    var size = double.Parse(sizeStr, new NumberFormatInfo { CurrencyDecimalSeparator = "." });
                    if (sizeType == "KB")
                        size = Math.Round(size / 1024, 2);
                    else if (sizeType == "GB")
                        size = size * 1024;
                    if (minSize.HasValue && size < minSize.Value) continue;
                    if (maxSize.HasValue && size > maxSize.Value) continue;

                    var seed = Convert.ToInt32(tds[4].TextContent);
                    if (minSeed.HasValue && seed < minSeed.Value) continue;

                    var leech = Convert.ToInt32(tds[5].TextContent);

                    results.Add(new TorrentSearchResult(service, this, torrentUrl, torrentName, seed, leech, size, null, age, magnetUri));
                }
            }

            return results;
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ServiceNames.Rarbg; }
        }

        #endregion

        #region IConfigurable Members

        string IConfigurable.SettingName {
            get { return ServiceName; }
        }

        INotifyPropertyChanged IConfigurable.Settings {
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
