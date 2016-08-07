using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.DOM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Info;

namespace Novaroma.Services.Imdb {

    public class ImdbInfoProvider : IAdvancedInfoProvider, IConfigurable {
        private static readonly IEnumerable<string> _genres = new[] {
                                                                        "Action", "Adventure", "Animation", "Biography", "Comedy", "Crime", "Documentary", "Drama",
                                                                        "Family", "Fantasy", "Film-Noir", "Game-Show", "History", "Horror", "Music", "Musical", "Mystery",
                                                                        "News", "Reality-TV", "Romance", "Sci-Fi", "Sport", "Talk-Show", "Thriller", "War", "Western"
                                                                    };
        private readonly ImdbSettings _settings = new ImdbSettings();
        private const string BASE_URL = @"http://www.imdb.com/";
        private const string BASIC_SEARCH_URL = @"http://www.imdb.com/find?q={0}&s=tt&ttype=ft,tv&ref_=fn_ft";
        private const string ADVANCED_SEARCH_URL = @"http://www.imdb.com/search/title?count={0}&{1}";
        private const string DEFAULT_POSTER_URL = @"http://ia.media-imdb.com/images/G/01/imdb/images/nopicture/large/film-184890147._CB282517447_.png";
        private const string TITLE_URL = @"http://www.imdb.com/title/{0}/";

        public async Task<IEnumerable<IInfoSearchResult>> Search(string query, Language language = Language.English) {
            if (Settings.UseAdvancedSearch) {
                var retVal = await AdvancedSearch(query);
                return retVal;
            }
            else {
                var retVal = await BasicSearch(query);
                return retVal;
            }
        }

        public Task<IEnumerable<ImdbInfoSearchResult>> BasicSearch(string query, Language language = Language.English) {
            return Task.Run(async () => {
                query = HttpUtility.UrlEncode(query, Encoding.GetEncoding("ISO-8859-1"));

                var urlStr = string.Format(BASIC_SEARCH_URL, query);
                var url = new Uri(urlStr);
                string documentStr;
                using (var client = new NovaromaWebClient()) {
                    if (!Settings.UseLocalTitles)
                        client.Headers.Add("X-FORWARDED-FOR", "199.254.254.254");
                    documentStr = await client.DownloadStringTaskAsync(url);
                }
                var document = DocumentBuilder.Html(documentStr);

                var items = document.All.Where(n => n.TagName == "TR" && (n.ClassName == "findResult even" || n.ClassName == "findResult odd"));

                var results = new List<ImdbInfoSearchResult>();
                foreach (var item in items) {
                    var itemText = item.TextContent.Trim();
                    if (itemText.Contains(" (TV Episode) ")) continue;

                    var linkNode = item.Children[0].Children[0];
                    var link = linkNode.Attributes.First(a => a.Name == "href").Value;
                    var resultUrl = Helper.CombineUrls(BASE_URL, link);
                    var imdbId = Regex.Match(link, @"\/.*\/(.*)\/").Groups[1].Value;
                    var posterUrl = linkNode.Children[0].Attributes.First(a => a.Name == "src").Value;

                    var titleNode = item.Children[1];
                    var title = titleNode.QuerySelector("a").TextContent.Trim();

                    var yearStr = Regex.Match(itemText, @"\((\d{4})\)").Groups[1].Value;
                    var isTvShow = itemText.Contains("TV Series)") || itemText.Contains("TV Mini-Series)");

                    int? year = null;
                    int yearTmp;
                    if (int.TryParse(yearStr, out yearTmp))
                        year = yearTmp;

                    byte[] poster = null;
                    using (var client = new NovaromaWebClient()) {
                        if (posterUrl != DEFAULT_POSTER_URL)
                            poster = await client.DownloadDataTaskAsync(posterUrl);
                    }

                    var result = new ImdbInfoSearchResult(this, imdbId, resultUrl, title, poster, string.Empty, year, isTvShow);
                    results.Add(result);
                }

                return results.AsEnumerable();
            });
        }

        public Task<IEnumerable<ImdbAdvancedInfoSearchResult>> AdvancedSearch(
                string query, MediaTypes mediaTypes = MediaTypes.All, int? releaseYearStart = null, int? releaseYearEnd = null,
                float? ratingMin = null, float? ratingMax = null, int? voteCountMin = null, int? voteCountMax = null,
                int? runtimeMin = null, int? runtimeMax = null, IEnumerable<string> genres = null, Language language = Language.English) {
            return Task.Run(async () => {
                query = HttpUtility.UrlEncode(query, Encoding.GetEncoding("ISO-8859-1"));

                var searchParams = new List<string>();
                searchParams.Add("title=" + query);
                var types = new List<string>();
                if ((mediaTypes & MediaTypes.Movie) == MediaTypes.Movie)
                    types.AddRange(new[] { "feature", "tv_movie" });
                if ((mediaTypes & MediaTypes.TvShow) == MediaTypes.TvShow)
                    types.AddRange(new[] { "tv_series", "mini_series" });
                if ((mediaTypes & MediaTypes.Documentary) == MediaTypes.Documentary)
                    types.AddRange(new[] { "documentary" });
                searchParams.Add("title_type=" + string.Join(",", types));

                AddLimitParameter(searchParams, "release_date", releaseYearStart, releaseYearEnd);
                AddLimitParameter(searchParams, "user_rating", ratingMin, ratingMax);
                AddLimitParameter(searchParams, "num_votes", voteCountMin, voteCountMax);
                AddLimitParameter(searchParams, "runtime", runtimeMin, runtimeMax);

                if (genres != null)
                    searchParams.Add("genres=" + string.Join(",", genres.Select(g => g.ToLowerInvariant())));

                var queryString = string.Join("&", searchParams);

                var urlStr = string.Format(ADVANCED_SEARCH_URL, Settings.AdvancedSearchResultCount, queryString);
                var url = new Uri(urlStr);
                string documentStr;
                using (var client = new NovaromaWebClient()) {
                    if (!Settings.UseLocalTitles)
                        client.Headers.Add("X-FORWARDED-FOR", "199.254.254.254");
                    documentStr = await client.DownloadStringTaskAsync(url);
                }
                var document = DocumentBuilder.Html(documentStr);

                var items = document.All.Where(n => n.ClassName == "lister-item mode-advanced");

                var results = new List<ImdbAdvancedInfoSearchResult>();
                foreach (var item in items) {
                    var linkNode = item.QuerySelector("div[class='lister-item-image float-left'] a");
                    var link = linkNode.Attributes.First(a => a.Name == "href").Value;
                    var resultUrl = Helper.CombineUrls(BASE_URL, link);
                    var imdbId = Regex.Match(link, @"\/.*\/(.*)\/").Groups[1].Value;
                    var posterUrl = linkNode.QuerySelector("img").Attributes.First(a => a.Name == "loadlate").Value;

                    var contentNode = item.QuerySelector("div[class='lister-item-content']");
                    var titleNode = contentNode.QuerySelector("h3[class='lister-item-header']");
                    var title = titleNode.QuerySelector("a").TextContent.Trim();
                    var yearType = titleNode.QuerySelector("span[class='lister-item-year text-muted unbold']").TextContent.Trim();

                    int? year = null;
                    var isTvShow = false;
                    if (yearType.Length > 4) {
                        isTvShow = yearType.Contains("–");

                        int yearTmp;
                        if (int.TryParse(yearType.Substring(1, 4), out yearTmp))
                            year = yearTmp;
                    }
                    var textNodes = contentNode.QuerySelectorAll("p");
                    var textNodesTextMuted = textNodes.Where(x => x.ClassName == "text-muted" || x.ClassName == "text-muted ").ToArray();

                    var genreStr = string.Empty;
                    var outline = string.Empty;
                    int? runtime = null;

                    if (textNodesTextMuted.Length > 0) {
                        var genreNode = textNodesTextMuted[0].QuerySelector("span[class='genre']");
                        if (genreNode != null)
                            genreStr = genreNode.TextContent.Trim();

                        var runtimeNode = textNodesTextMuted[0].QuerySelector("span[class='runtime']");
                        if (runtimeNode != null) {
                            var runtimeStr = runtimeNode.TextContent.Trim();
                            var match = Regex.Match(runtimeStr, @"(\d.*?) ");
                            if (match.Groups.Count == 2) {
                                int runtimeTmp;
                                if (int.TryParse(match.Groups[1].Value, out runtimeTmp))
                                    runtime = runtimeTmp;
                            }
                        }

                        if (textNodesTextMuted.Length > 1) {
                            var outlineNode = textNodesTextMuted[1];
                            outline = outlineNode.TextContent.Trim();
                        }
                    }

                    var textNodesOther = textNodes.Where(x => x.ClassName != "text-muted").ToArray();
                    var credits = string.Empty;
                    if (textNodesOther.Length > 1)
                        credits = textNodesOther[1].TextContent.Replace("\r","").Replace("\n","").Replace("  ","");

                    int? voteCount = null;
                    var voteNode = textNodes.FirstOrDefault(x => x.ClassName == "sort-num_votes-visible");
                    if (voteNode != null) {
                        var voteStr = voteNode.QuerySelector("span[name='nv']").Attributes.First(x => x.Name == "data-value").Value;
                        voteCount = int.Parse(voteStr);
                    }

                    float? rating = null;
                    var ratingNode = contentNode.QuerySelector("div[class='ratings-bar'] strong");
                    if (ratingNode != null)
                        rating = float.Parse(ratingNode.TextContent.Trim(), new NumberFormatInfo { CurrencyDecimalSeparator = "." });


                    byte[] poster = null;
                    using (var client = new NovaromaWebClient()) {
                        if (posterUrl != DEFAULT_POSTER_URL)
                            poster = await client.DownloadDataTaskAsync(posterUrl);
                    }

                    var result = new ImdbAdvancedInfoSearchResult(this, imdbId, resultUrl, title, poster, year, isTvShow, outline, credits, rating, voteCount, runtime, genreStr);
                    results.Add(result);
                }

                return results.AsEnumerable();
            });
        }

        public async Task<ImdbMovieInfo> GetMovie(string id, Language language = Language.English) {
            return await GetTitle(id)
                .ContinueWith(t => {
                    var m = t.Result;
                    return new ImdbMovieInfo(this, m.Id, m.Url, m.ImdbId, m.Title, m.OriginalTitle, m.Outline, m.Poster, m.Year,
                                             m.Credits, m.Rating, m.VoteCount, m.Runtime, m.Language, m.Genres, m.ServiceIds);
                });
        }

        public async Task<ImdbTvShowInfo> GetTvShow(string id, Language language = Language.English) {
            return await GetTitle(id)
                .ContinueWith(t => {
                    var m = t.Result;
                    return new ImdbTvShowInfo(this, m.Id, m.Url, m.ImdbId, m.Title, m.OriginalTitle, m.Outline, m.Poster, m.Year,
                                              m.Credits, m.Rating, m.VoteCount, m.Runtime, m.Language, m.Genres, m.ServiceIds);
                });
        }

        public Task<IMediaInfo> GetTitle(string id, Language language = Language.English) {
            return Task.Run(async () => {
                var urlStr = string.Format(TITLE_URL, id);
                var url = new Uri(urlStr);
                string documentStr;
                using (var client = new NovaromaWebClient()) {
                    if (!Settings.UseLocalTitles)
                        client.Headers.Add("X-FORWARDED-FOR", "199.254.254.254");
                    documentStr = await client.DownloadStringTaskAsync(url);
                }
                var document = DocumentBuilder.Html(documentStr);

                using (var client = new NovaromaWebClient()) {
                    return await GetTitle(client, document, id);
                }
            });
        }

        private async Task<IMediaInfo> GetTitle(NovaromaWebClient client, IDocument document, string id) {
            var tableNode = document.QuerySelector("table[id='title-overview-widget-layout']");
            IMediaInfo title;
            if (tableNode == null) {
                title = await GetTitleNew(client, document, id);
            }
            else {
                title = await GetTitleOld(client, document, id);
            }
            return title;
        }

        private async Task<IMediaInfo> GetTitleNew(NovaromaWebClient client, IDocument document, string id) {
            byte[] poster = null;
            var posterNode = document.QuerySelector("div[class='poster'] a img");
            Task<byte[]> posterTask = null;
            if (posterNode != null) {
                var posterUrl = posterNode.Attributes.First(a => a.Name == "src").Value;
                posterTask = client.DownloadDataTaskAsync(posterUrl);
            }

            var overviewNode = document.QuerySelector("div[class='title-overview']");

            var titleBarNode = overviewNode.QuerySelector("div[class='title_bar_wrapper']");
            var title = titleBarNode.QuerySelector("h1[itemprop='name']").TextContent.Trim();
            var originalTitleNode = titleBarNode.QuerySelector("div[class='originalTitle']");
            string originalTitle = null;
            if (originalTitleNode != null) {
                var matches = Regex.Match(originalTitleNode.TextContent.Trim(), @"\""(.*?)\""");
                if (matches.Groups.Count > 1)
                    originalTitle = matches.Groups[1].Value;
            }
            if (originalTitle == null)
                originalTitle = title;

            var plotNode = overviewNode.QuerySelector("div[class='plot_summary_wrapper']");
            var director = string.Join(", ", plotNode.QuerySelectorAll("span[itemprop='director'] a span").Select(n => n.TextContent));
            var actors = string.Join(", ", plotNode.QuerySelectorAll("span[itemprop='actors'] a span").Select(n => n.TextContent));
            var credits = Helper.JoinStrings(" - ", director, actors);

            int? year = null;
            var ratingWidgetTitleNode = document.QuerySelector("div[id='ratingWidget'] p");
            var yearText = ratingWidgetTitleNode.ChildNodes[2];
            if (yearText != null) {
                var yearStr = yearText.TextContent.Trim();
                if (yearStr.Length > 4) {
                    int yearTmp;
                    if (int.TryParse(yearStr.Substring(1, 4), out yearTmp))
                        year = yearTmp;
                }
            }

            float? rating = null;
            int? voteCount = null;
            var ratingNode = overviewNode.QuerySelector("div[class='ratingValue']");
            if (ratingNode != null) {
                var ratingValueNode = ratingNode.QuerySelector("span[itemprop='ratingValue']");
                if (ratingValueNode != null) {
                    var ratingStr = ratingValueNode.TextContent.Replace(",", ".");
                    rating = float.Parse(ratingStr, new NumberFormatInfo { CurrencyDecimalSeparator = "." });
                }

                var voteNode = ratingNode.QuerySelector("span[itemprop='ratingCount']");
                if (voteNode != null) {
                    var voteCountStr = voteNode.TextContent;
                    voteCountStr = voteCountStr.Replace(",", "").Replace(".", "");
                    voteCount = int.Parse(voteCountStr);
                }
            }

            var description = string.Empty;
            var descriptionNode = plotNode.QuerySelector("div[itemprop='description']");
            if (descriptionNode != null) {
                var fullDescLinkNode = descriptionNode.Children.FirstOrDefault(c => c.TagName == "A" && c.TextContent.Contains("See full summary"));
                if (fullDescLinkNode != null) {
                    var descUrl = new Url(Helper.CombineUrls(BASE_URL, fullDescLinkNode.Attributes.First().Value));
                    string descriptionHtmlStr;
                    using (var descriptionClient = new NovaromaWebClient()) {
                        if (!Settings.UseLocalTitles)
                            descriptionClient.Headers.Add("X-FORWARDED-FOR", "199.254.254.254");
                        descriptionHtmlStr = await descriptionClient.DownloadStringTaskAsync(descUrl);
                    }
                    var descriptionHtml = DocumentBuilder.Html(descriptionHtmlStr);
                    var plotSummaryNode = descriptionHtml.QuerySelectorAll("div[class='plotSummary']").FirstOrDefault();
                    if (plotSummaryNode != null)
                        description = plotSummaryNode.TextContent.Trim();
                }
                else
                    description = descriptionNode.TextContent.Trim();
            }

            int? runtime = 0;
            var isTvShow = false;
            IEnumerable<string> genres = null;
            var runtimeNode = titleBarNode.QuerySelector("time[itemprop='duration']");
            if (runtimeNode != null) {
                var runtimeStr = runtimeNode.Attributes.First(a => a.Name == "datetime").Value;
                if (runtimeStr.Length > 3)
                    runtime = int.Parse(runtimeStr.Substring(2, runtimeStr.Length - 3).Replace(",", ""));
            }
            isTvShow = titleBarNode.TextContent.Trim().Contains("TV Series");

            genres = titleBarNode.QuerySelectorAll("span[itemprop='genre']").Select(n => n.TextContent.Trim());

            if (posterTask != null)
                poster = await posterTask;

            Language? titleLanguage = null;
            var detailsNode = document.QuerySelectorAll("div[id='titleDetails']").FirstOrDefault();
            if (detailsNode != null) {
                var languageLabel = detailsNode.QuerySelectorAll("h4[class='inline']").FirstOrDefault(h => h.TextContent == "Language:");
                if (languageLabel != null)
                    titleLanguage = GetLanguage(languageLabel.NextElementSibling.Text());
            }

            var mediaUrl = string.Format(TITLE_URL, id);
            IMediaInfo mediaInfo;
            if (isTvShow)
                mediaInfo = new ImdbTvShowInfo(this, id, mediaUrl, id, title, originalTitle, description, poster, year, credits, rating, voteCount, runtime, titleLanguage, genres, null);
            else
                mediaInfo = new ImdbMovieInfo(this, id, mediaUrl, id, title, originalTitle, description, poster, year, credits, rating, voteCount, runtime, titleLanguage, genres, null);

            return mediaInfo;
        }

        private async Task<IMediaInfo> GetTitleOld(NovaromaWebClient client, IDocument document, string id) {
            byte[] poster = null;
            var posterNode = document.QuerySelector("td[id='img_primary'] div a img");
            Task<byte[]> posterTask = null;
            if (posterNode != null) {
                var posterUrl = posterNode.Attributes.First(a => a.Name == "src").Value;
                posterTask = client.DownloadDataTaskAsync(posterUrl);
            }

            var overviewNode = document.QuerySelector("td[id='overview-top']");

            var title = overviewNode.QuerySelector("span[itemprop='name']").TextContent;
            var originalTitleNode = overviewNode.QuerySelector("span[class='title-extra']");
            string originalTitle = null;
            if (originalTitleNode != null) {
                var matches = Regex.Match(originalTitleNode.TextContent, @"\""(.*?)\""");
                if (matches.Groups.Count > 1)
                    originalTitle = matches.Groups[1].Value;
            }
            if (originalTitle == null)
                originalTitle = title;

            var director = string.Join(", ", overviewNode.QuerySelectorAll("div[itemprop='director'] a span").Select(n => n.TextContent));
            var actors = string.Join(", ", overviewNode.QuerySelectorAll("div[itemprop='actors'] a span").Select(n => n.TextContent));
            var credits = Helper.JoinStrings(" - ", director, actors);

            var yearStr = overviewNode.QuerySelector("span[class='nobr']").TextContent;
            int? year = null;
            if (yearStr.Length > 4) {
                int yearTmp;
                if (int.TryParse(yearStr.Substring(1, 4), out yearTmp))
                    year = yearTmp;
            }

            float? rating = null;
            int? voteCount = null;
            var ratingNode = overviewNode.QuerySelector("div[itemprop='aggregateRating']");
            if (ratingNode != null) {
                var ratingValueNode = ratingNode.QuerySelector("span[itemprop='ratingValue']");
                if (ratingValueNode != null) {
                    var ratingStr = ratingValueNode.TextContent.Replace(",", ".");
                    rating = float.Parse(ratingStr, new NumberFormatInfo { CurrencyDecimalSeparator = "." });
                }

                var voteNode = ratingNode.QuerySelector("span[itemprop='ratingCount']");
                if (voteNode != null) {
                    var voteCountStr = voteNode.TextContent;
                    voteCountStr = voteCountStr.Replace(",", "").Replace(".", "");
                    voteCount = int.Parse(voteCountStr);
                }
            }

            var description = string.Empty;
            var descriptionNode = overviewNode.QuerySelector("p[itemprop='description']");
            if (descriptionNode != null) {
                var fullDescLinkNode = descriptionNode.Children.FirstOrDefault(c => c.TagName == "A" && c.TextContent.Contains("See full summary"));
                if (fullDescLinkNode != null) {
                    var descUrl = new Url(Helper.CombineUrls(BASE_URL, fullDescLinkNode.Attributes.First().Value));
                    string descriptionHtmlStr;
                    using (var descriptionClient = new NovaromaWebClient()) {
                        if (!Settings.UseLocalTitles)
                            descriptionClient.Headers.Add("X-FORWARDED-FOR", "199.254.254.254");
                        descriptionHtmlStr = await descriptionClient.DownloadStringTaskAsync(descUrl);
                    }
                    var descriptionHtml = DocumentBuilder.Html(descriptionHtmlStr);
                    var plotSummaryNode = descriptionHtml.QuerySelectorAll("p[class='plotSummary']").FirstOrDefault();
                    if (plotSummaryNode != null)
                        description = plotSummaryNode.TextContent.Trim();
                }
                else
                    description = descriptionNode.TextContent.Trim();
            }

            int? runtime = 0;
            var isTvShow = false;
            IEnumerable<string> genres = null;
            var infobarNode = overviewNode.QuerySelector("div[class='infobar']");
            if (infobarNode != null) {
                var runtimeNode = infobarNode.QuerySelector("time[itemprop='duration']");
                if (runtimeNode != null) {
                    var runtimeStr = runtimeNode.Attributes.First(a => a.Name == "datetime").Value;
                    if (runtimeStr.Length > 3)
                        runtime = int.Parse(runtimeStr.Substring(2, runtimeStr.Length - 3).Replace(",", ""));
                }
                isTvShow = infobarNode.TextContent.Trim().Contains("TV Series");

                genres = infobarNode.QuerySelectorAll("span[itemprop='genre']").Select(n => n.TextContent.Trim());
            }

            if (posterTask != null)
                poster = await posterTask;

            Language? titleLanguage = null;
            var detailsNode = document.QuerySelectorAll("div[id='titleDetails']").FirstOrDefault();
            if (detailsNode != null) {
                var languageLabel = detailsNode.QuerySelectorAll("h4[class='inline']").FirstOrDefault(h => h.TextContent == "Language:");
                if (languageLabel != null)
                    titleLanguage = GetLanguage(languageLabel.NextElementSibling.Text());
            }

            var mediaUrl = string.Format(TITLE_URL, id);
            IMediaInfo mediaInfo;
            if (isTvShow)
                mediaInfo = new ImdbTvShowInfo(this, id, mediaUrl, id, title, originalTitle, description, poster, year, credits, rating, voteCount, runtime, titleLanguage, genres, null);
            else
                mediaInfo = new ImdbMovieInfo(this, id, mediaUrl, id, title, originalTitle, description, poster, year, credits, rating, voteCount, runtime, titleLanguage, genres, null);

            return mediaInfo;
        }

        private static void AddLimitParameter<T>(ICollection<string> searchParams, string paramName, T? min, T? max) where T : struct {
            var prm = string.Empty;
            if (min.HasValue)
                prm += AsString(min.Value);
            prm += ",";
            if (max.HasValue)
                prm += AsString(max.Value);

            if (prm != ",")
                searchParams.Add(paramName + "=" + prm);
        }

        private static Language? GetLanguage(string text) {
            switch (text) {
                case "Dutch":
                    return Language.Dutch;
                case "English":
                    return Language.English;
                case "French":
                    return Language.French;
                case "German":
                    return Language.German;
                case "Italian":
                    return Language.Italian;
                case "Russian":
                    return Language.Russian;
                case "Turkish":
                    return Language.Turkish;
                default:
                    return null;
            }
        }

        public string ServiceName {
            get { return ServiceNames.Imdb; }
        }

        public ImdbSettings Settings {
            get { return _settings; }
        }

        private static string AsString(object o) {
            if (o == null) return string.Empty;
            if (o is float || o is double || o is decimal)
                return ((float)o).ToString(CultureInfo.InvariantCulture);
            return o.ToString();
        }

        #region IInfoProvider Members

        async Task<IEnumerable<IInfoSearchResult>> IInfoProvider.Search(string query, Language language) {
            var r = await Search(query, language);
            return r;
        }

        public async Task<IMovieInfo> GetMovie(IInfoSearchResult searchResult, Language language = Language.English) {
            var r = await GetMovie(searchResult.Id, language);
            return r;
        }

        async Task<IMovieInfo> IInfoProvider.GetMovie(string id, Language language) {
            var r = await GetMovie(id, language);
            return r;
        }

        public async Task<ITvShowInfo> GetTvShow(IInfoSearchResult searchResult, Language language = Language.English) {
            var r = await GetTvShow(searchResult.Id, language);
            return r;
        }

        async Task<ITvShowInfo> IInfoProvider.GetTvShow(string id, Language language) {
            var r = await GetTvShow(id, language);
            return r;
        }

        #endregion

        #region IAdvancedInfoProvider Members

        public IEnumerable<string> Genres {
            get {
                return _genres;
            }
        }

        async Task<IEnumerable<IAdvancedInfoSearchResult>> IAdvancedInfoProvider.AdvancedSearch(
                string query, MediaTypes mediaTypes, int? releaseYearStart, int? releaseYearEnd,
                float? ratingMin, float? ratingMax, int? voteCountMin, int? voteCountMax,
                int? runtimeMin, int? runtimeMax, IEnumerable<string> genres, Language language) {
            var t = await AdvancedSearch(query, mediaTypes, releaseYearStart, releaseYearEnd, ratingMin, ratingMax,
                                         voteCountMin, voteCountMax, runtimeMin, runtimeMax, genres, language);
            return t;
        }

        public async Task<IMovieInfo> GetMovie(IAdvancedInfoSearchResult searchResult, Language language = Language.English) {
            var r = await GetMovie(searchResult.Id, language);
            return r;
        }

        public async Task<ITvShowInfo> GetTvShow(IAdvancedInfoSearchResult searchResult, Language language = Language.English) {
            var r = await GetTvShow(searchResult.Id, language);
            return r;
        }

        #endregion

        #region IConfigurable Members

        string IConfigurable.SettingName {
            get { return ServiceName; }
        }

        INotifyPropertyChanged IConfigurable.Settings {
            get { return Settings; }
        }

        public string SerializeSettings() {
            return JsonConvert.SerializeObject(Settings);
        }

        public void DeserializeSettings(string settings) {
            var o = (JObject)JsonConvert.DeserializeObject(settings);
            Settings.UseAdvancedSearch = Convert.ToBoolean(o["UseAdvancedSearch"]);
            Settings.AdvancedSearchResultCount = Convert.ToInt32(o["AdvancedSearchResultCount"]);

            var useLocalTitles = o["UseLocalTitles"];
            if (useLocalTitles != null)
                Settings.UseLocalTitles = Convert.ToBoolean(useLocalTitles);
        }

        #endregion
    }
}
