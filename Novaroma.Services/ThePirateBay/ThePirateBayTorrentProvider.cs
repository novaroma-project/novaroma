using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Download.Torrent;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.Services.ThePirateBay {

    public class ThePirateBayTorrentProvider : ITorrentMovieProvider, ITorrentTvShowProvider, IConfigurable {
        private readonly ThePirateBaySettings _settings = new ThePirateBaySettings();

        private static Task<byte[]> DownloadTorrent(ITorrentSearchResult searchResult) {
            using (var client = new NovaromaWebClient()) {
                return client.DownloadDataTaskAsync(searchResult.Url);
            }
        }

        public override string ToString() {
            return ServiceName;
        }

        public ThePirateBaySettings Settings {
            get { return _settings; }
        }

        #region ITorrentMovieProvider Members

        public async Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
                                                                         string extraKeywords = null, string excludeKeywords = null, ITorrentDownloader service = null) {
            var query = Settings.MovieSearchPattern;

            IEnumerable<ITorrentSearchResult> results;
            if (query == ThePirateBaySettings.ImdbSearchQuery)
                results = await Search(imdbId, VideoQuality.Any, excludeKeywords, service);
            else {
                query = Helper.PopulateMovieSearchQuery(query, name, year, imdbId, extraKeywords);
                results = await Search(query, videoQuality, excludeKeywords, service);
            }

            if (query == ThePirateBaySettings.ImdbSearchQuery && extraKeywords != null) {
                switch (videoQuality) {
                    case VideoQuality.P720:
                        extraKeywords += " 720";
                        break;
                    case VideoQuality.P1080:
                        extraKeywords += " 1080";
                        break;
                }
                var extras = extraKeywords.Split(' ');
                results = results.Where(r => extras.All(e => r.Name.Contains(e)));
            }

            return results;
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
            var url = Helper.CombineUrls(Settings.BaseUrl, "search", search);
            using (var client = new NovaromaWebClient()) {
                var html = await client.DownloadStringTaskAsync(url);

                var document = DocumentBuilder.Html(html);
                var items = document.QuerySelectorAll("table[id='searchResult'] tr");

                foreach (var item in items) {
                    var tds = item.QuerySelectorAll("td");
                    if (tds.Length < 4) continue;

                    var mainTd = tds[1];
                    var anchor = mainTd.Children[0].Children[0];
                    var torrentUrl = anchor.Attributes.First(a => a.Name == "href").Value;
                    var torrentName = anchor.TextContent.Trim();
                    if (excludeList.Any(e => torrentName.IndexOf(e, StringComparison.OrdinalIgnoreCase) > 0)) continue;

                    var magnetUri = tds[1].Children[1].Attributes.First(a => a.Name == "href").Value;
                    var detDescNode = tds[1].QuerySelectorAll("font[class='detDesc']").First();
                    var detDesc = detDescNode.TextContent;
                    var idx1 = detDesc.IndexOf(",", StringComparison.OrdinalIgnoreCase);
                    var idx2 = detDesc.IndexOf(",", idx1 + 1, StringComparison.Ordinal);
                    var age = detDesc.Substring(0, idx1).Replace("Uploaded", string.Empty).Trim();
                    if (idx2 == -1) idx2 = detDesc.Length;
                    var size = detDesc.Substring(idx1 + 1, idx2 - idx1 - 1).Replace("Size", string.Empty).Trim();
                    var seed = Convert.ToInt32(tds[2].TextContent);
                    var leech = Convert.ToInt32(tds[3].TextContent);

                    results.Add(new TorrentSearchResult(service, this, torrentUrl, torrentName, seed, leech, size, null, age, magnetUri, DownloadTorrent));
                }
            }

            return results;
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ServiceNames.ThePirateBay; }
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
