using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Novaroma.Interface.Subtitle;
using Novaroma.Properties;
using OSDBnet;

namespace Novaroma.Services.OpenSubtitles {

    public class OpenSubtitleDownloader : ISubtitleDownloader {
        public const string UserAgent = "novaroma v0.1";

        public Task<IEnumerable<OpenSubtitleSearchResult>> SearchForFile(string videoFilePath, Language[] languages, string imdbId = null) {
            if (videoFilePath == null)
                throw new ArgumentNullException("videoFilePath");

            var fileInfo = new FileInfo(videoFilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(string.Format(Resources.FileNotFound, videoFilePath), videoFilePath);

            return Task.Run(() => {
                try {
                    using (var client = Osdb.Login(UserAgent)) {
                        var languageIds = languages.Select(GetLanguageId).ToList();
                        var languageIdsPrm = string.Join(",", languages.Select(GetLanguageId));

                        var results = client.SearchSubtitlesFromFile(languageIdsPrm, videoFilePath);
                        results = results.OrderBy(s => s.LanguageId, new SubtitleLanguageComparer(languageIds)).ToList();

                        return results.Select(r => new OpenSubtitleSearchResult(this, r));
                    }
                }
                catch (Exception ex) {
                    if (ex.Message == "Unexpected error response 401 Unauthorized")
                        return Enumerable.Empty<OpenSubtitleSearchResult>();
                    throw;
                }
            });
        }

        public Task<IEnumerable<OpenSubtitleSearchResult>> Search(string query, Language[] languages, string imdbId = null) {
            return Task.Run(() => {
                try {
                    using (var client = Osdb.Login(UserAgent)) {
                        var languageIds = languages.Select(GetLanguageId).ToList();
                        var languageIdsPrm = string.Join(",", languages.Select(GetLanguageId));

                        var results = client.SearchSubtitlesFromQuery(languageIdsPrm, query);
                        results = results.OrderBy(s => s.LanguageId, new SubtitleLanguageComparer(languageIds)).ToList();
                        return results.Select(r => new OpenSubtitleSearchResult(this, r));
                    }
                }
                catch (Exception ex) {
                    if (ex.Message == "Unexpected error response 401 Unauthorized")
                        return Enumerable.Empty<OpenSubtitleSearchResult>();
                    throw;
                }
            });
        }

        public Task<bool> Download(string videoFilePath, OpenSubtitleSearchResult searchResult, bool downloadOnly) {
            if (videoFilePath == null)
                throw new ArgumentNullException("videoFilePath");

            var fileInfo = new FileInfo(videoFilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(string.Format(Resources.FileNotFound, videoFilePath), videoFilePath);

            return Task.Run(() => {
                try {
                    using (var client = Osdb.Login(UserAgent)) {
                        var subtitlePath = client.DownloadSubtitleToPath(fileInfo.DirectoryName, searchResult.Subtitle);
                        if (downloadOnly) {
                            var subtitleFileFullName = Path.Combine(subtitlePath, searchResult.Subtitle.SubtitleFileName);

                            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(subtitleFileFullName);
                            var ext = Path.GetExtension(subtitleFileFullName);

                            var newFileName = string.Format("[NOVAROMA]{0}.{1}", fileNameWithoutExt, ext);
                            var newFileFullName = Path.Combine(subtitlePath, newFileName);

                            File.Move(subtitleFileFullName, newFileFullName);
                        }
                        else {
                            var subtitleFile = new FileInfo(subtitlePath);
                            var newSubtitlePath = Helper.ReplaceExtension(fileInfo, subtitleFile.Extension);

                            if (!subtitleFile.Exists) return false;
                            if (string.Equals(subtitlePath, newSubtitlePath, StringComparison.OrdinalIgnoreCase)) return true;

                            Helper.DeleteFile(newSubtitlePath);
                            subtitleFile.MoveTo(newSubtitlePath);
                        }


                        return true;
                    }
                }
                catch (Exception ex) {
                    if (ex.Message == "Unexpected error response 401 Unauthorized")
                        return false;
                    throw;
                }
            });
        }

        public static string GetLanguageId(Language language) {
            return Helper.GetThreeLetterLanguageCode(language);
        }

        #region ISubtitleDownloader Members

        async Task<IEnumerable<ISubtitleSearchResult>> ISubtitleDownloader.SearchForMovie(string name, string videoFilePath, Language[] languages, string imdbId) {
            return await SearchForFile(videoFilePath, languages, imdbId);
        }

        async Task<bool> ISubtitleDownloader.DownloadForMovie(string name, string videoFilePath, Language[] languages, bool downloadOnly, string imdbId) {
            var results = await SearchForFile(videoFilePath, languages, imdbId);
            var result = results.FirstOrDefault();
            if (result == null) return false;

            return await Download(videoFilePath, result, downloadOnly);
        }

        async Task<IEnumerable<ISubtitleSearchResult>> ISubtitleDownloader.SearchForTvShowEpisode(string name, int season, int episode, string videoFilePath,
                                                                                                  Language[] languages, string imdbId) {
            return await SearchForFile(videoFilePath, languages, imdbId);
        }

        async Task<bool> ISubtitleDownloader.DownloadForTvShowEpisode(string name, int season, int episode, string videoFilePath, Language[] languages, bool downloadOnly, string imdbId) {
            var results = await SearchForFile(videoFilePath, languages, imdbId);
            var result = results.FirstOrDefault();
            if (result == null) return false;

            return await Download(videoFilePath, result, downloadOnly);
        }

        async Task<IEnumerable<ISubtitleSearchResult>> ISubtitleDownloader.Search(string query, Language[] languages, string imdbId) {
            return await Search(query, languages);
        }

        async Task<bool> ISubtitleDownloader.Download(string videoFilePath, ISubtitleSearchResult searchResult, bool downloadOnly) {
            var openSubtitleSearchResult = searchResult as OpenSubtitleSearchResult;
            if (openSubtitleSearchResult == null)
                throw new NovaromaException(Resources.UnsupportedSearchResult);

            return await Download(videoFilePath, openSubtitleSearchResult, downloadOnly);
        }

        async Task<bool> ISubtitleDownloader.Download(string videoFilePath, Language[] languages, bool downloadOnly, string imdbId) {
            var results = await SearchForFile(videoFilePath, languages);
            var result = results.FirstOrDefault();
            if (result != null)
                return await Download(videoFilePath, result, downloadOnly);

            return false;
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ServiceNames.OpenSubtitles; }
        }

        #endregion
    }

    internal class SubtitleLanguageComparer : IComparer<string> {
        private readonly IList<string> _languageIds;

        public SubtitleLanguageComparer(IEnumerable<string> languageIds) {
            _languageIds = languageIds.ToList();
        }

        public int Compare(string x, string y) {
            if (x == y) return 0;

            var idx1 = _languageIds.IndexOf(x);
            var idx2 = _languageIds.IndexOf(y);

            return idx1.CompareTo(idx2);
        }
    }
}
