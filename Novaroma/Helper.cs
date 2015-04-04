using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Newtonsoft.Json;
using Novaroma.Interface;
using Novaroma.Interface.Info;
using Novaroma.Interface.Model;
using Novaroma.Model;
using Novaroma.Properties;

namespace Novaroma {

    public static class Helper {
        public static string[] VideoExtensions = { ".avi", ".mkv", ".mp4", ".m4p", ".m4v", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".m2v", ".wmv" };
        public static string[] SubtitleExtensions = { ".srt", ".sub" };

        public static void SetCulture(Language language) {
            var cultureCode = GetTwoLetterLanguageCode(language);
            var cultureInfo = CultureInfo.GetCultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }

        public static bool IsVideoFile(string filePath) {
            return IsVideoFile(new FileInfo(filePath));
        }

        public static bool IsVideoFile(FileInfo fileInfo) {
            return fileInfo.Exists && VideoExtensions.Contains(fileInfo.Extension.ToLowerInvariant());
        }

        public static bool IsSubtitleFile(string filePath) {
            return IsSubtitleFile(new FileInfo(filePath));
        }

        public static bool IsSubtitleFile(FileInfo fileInfo) {
            return fileInfo.Exists && SubtitleExtensions.Contains(fileInfo.Extension.ToLowerInvariant());
        }

        public static string ReplaceExtension(FileInfo fileInfo, string newExtension) {
            if (string.IsNullOrEmpty(fileInfo.Extension))
                return fileInfo.FullName + newExtension;

            return fileInfo.FullName.Remove(fileInfo.FullName.LastIndexOf(fileInfo.Extension, StringComparison.OrdinalIgnoreCase)) + newExtension;
        }

        public static string GetFirstVideoFileName(string directory) {
            var directoryInfo = new DirectoryInfo(directory);
            if (!directoryInfo.Exists) return string.Empty;

            var videoFiles = directoryInfo.GetFiles().Where(IsVideoFile).OrderBy(fi => fi.Name).ToList();
            var firstVideoFile = videoFiles.FirstOrDefault(vf => vf.Name.IndexOf("sample", StringComparison.OrdinalIgnoreCase) < 0)
                ?? videoFiles.FirstOrDefault();

            return firstVideoFile == null ? string.Empty : firstVideoFile.Name;
        }

        public static string GetSubtitleFilePath(FileInfo videoFile) {
            if (videoFile.Directory == null) return string.Empty;
            var files = videoFile.Directory.GetFiles().ToList();
            return files
                .Where(f => string.Equals(f.NameWithoutExtension(), videoFile.NameWithoutExtension(), StringComparison.OrdinalIgnoreCase) && IsSubtitleFile(f))
                .Select(f => f.FullName)
                .FirstOrDefault();
        }

        public static void DetectEpisodeInfo(FileInfo fileInfo, out int? season, out int? episode) {
            season = null;
            episode = null;

            var match = Regex.Match(fileInfo.Name, @"(\d{1,2}).*?(\d{1,2})");
            int tmpSeason, tmpEpisode;
            string tmpSeasonStr = null, tmpEpisodeStr = null;
            if (match.Groups.Count == 3 || match.Groups.Count == 2) {
                var result = string.Empty;
                for (var i = 1; i < match.Groups.Count; i++)
                    result += match.Groups[i];

                if (result.Length >= 4) {
                    tmpSeasonStr = match.Groups[1].Value;
                    tmpEpisodeStr = result.Substring(tmpSeasonStr.Length);
                }
                else if (result.Length == 3) {
                    tmpSeasonStr = result.Substring(0, 1);
                    tmpEpisodeStr = result.Substring(1, 2);
                }
                else if (result.Length < 3) {
                    if (!string.IsNullOrEmpty(fileInfo.DirectoryName)) {
                        var seasonResult = Regex.Match(fileInfo.DirectoryName, @"(\d{1,2})");
                        if (seasonResult.Groups.Count == 1)
                            tmpSeasonStr = seasonResult.Groups[0].Value;
                    }
                    tmpEpisodeStr = result;
                }
            }

            if (!string.IsNullOrEmpty(tmpSeasonStr) && int.TryParse(tmpSeasonStr, out tmpSeason))
                season = tmpSeason;
            if (!string.IsNullOrEmpty(tmpEpisodeStr) && int.TryParse(tmpEpisodeStr, out tmpEpisode))
                episode = tmpEpisode;
        }

        public static string PopulateMovieSearchQuery(Movie movie, string query = Constants.DefaultMovieSearchPattern) {
            return PopulateMovieSearchQuery(query, movie.OriginalTitle, movie.Year, movie.ImdbId, movie.ExtraKeywords);
        }

        public static string PopulateMovieSearchQuery(string query, string movieName, int? year, string imdbId, string extraKeywords) {
            if (!string.IsNullOrEmpty(extraKeywords))
                query += " " + extraKeywords;
            query = Regex.Replace(query, "%movieName%", movieName, RegexOptions.IgnoreCase);
            query = Regex.Replace(query, "%year%", year == null ? string.Empty : year.Value.ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
            query = Regex.Replace(query, "%imdbId%", imdbId, RegexOptions.IgnoreCase);

            return query;
        }

        public static string PopulateTvShowEpisodeSearchQuery(TvShowEpisode episode, string query = Constants.DefaultTvShowEpisodeSearchPattern) {
            var season = episode.TvShowSeason;
            var tvShow = episode.TvShowSeason.TvShow;
            var extraKeywords = ((IDownloadable)episode).ExtraKeywords;
            return PopulateTvShowEpisodeSearchQuery(query, tvShow.OriginalTitle, season.Season, episode.Episode, tvShow.ImdbId, extraKeywords);
        }

        public static string PopulateTvShowEpisodeSearchQuery(string query, string showName, int season, int episode, string imdbId, string extraKeywords) {
            if (!string.IsNullOrEmpty(extraKeywords))
                query += " " + extraKeywords;
            query = Regex.Replace(query, "%showName%", showName, RegexOptions.IgnoreCase);
            var seasonStr = season.ToString("D2");
            var episodeStr = episode.ToString("D2");
            query = Regex.Replace(query, "%season%", seasonStr, RegexOptions.IgnoreCase);
            query = Regex.Replace(query, "%episode%", episodeStr, RegexOptions.IgnoreCase);
            query = Regex.Replace(query, "%imdbId%", imdbId, RegexOptions.IgnoreCase);

            return query;
        }

        public static string GetTvShowSeasonDirectory(string template, TvShowEpisode episode) {
            var season = episode.TvShowSeason;
            var show = season.TvShow;

            if (!string.IsNullOrEmpty(template)) {
                var seasonFolder = Regex.Replace(template, "%season%", season.Season.ToString("D2"), RegexOptions.IgnoreCase);
                return Path.Combine(show.Directory, seasonFolder);
            }

            return show.Directory;
        }

        public static TMedia MapToModel<TMedia>(IMediaInfo mediaInfo)
            where TMedia : Media, new() {

            if (mediaInfo == null) return default(TMedia);

            var media = new TMedia();
            MapToModel(mediaInfo, media);

            return media;
        }

        public static void MapToModel(IMediaInfo mediaInfo, Media media) {
            media.Credits = mediaInfo.Credits;
            media.ImdbId = mediaInfo.ImdbId;
            media.Language = mediaInfo.Language;
            media.OriginalTitle = mediaInfo.OriginalTitle;
            media.Outline = mediaInfo.Outline;
            media.Poster = mediaInfo.Poster;
            media.Rating = mediaInfo.Rating;
            media.Runtime = mediaInfo.Runtime;
            media.ServiceName = mediaInfo.Service.ServiceName;
            media.ServiceId = mediaInfo.Id;
            media.ServiceUrl = mediaInfo.Url;
            media.Title = mediaInfo.Title;
            media.VoteCount = mediaInfo.VoteCount;
            media.Year = mediaInfo.Year;

            if (mediaInfo.Genres != null) {
                media.Genres.Clear();
                mediaInfo.Genres
                    .ToList()
                    .ForEach(g => media.Genres.Add(new MediaGenre { Name = g }));
            }

            if (mediaInfo.ServiceIds != null) {
                mediaInfo.ServiceIds
                    .Where(sm => media.ServiceMappings.All(msm => !string.Equals(msm.ServiceName, sm.Key, StringComparison.OrdinalIgnoreCase)))
                    .ToList()
                    .ForEach(si => media.ServiceMappings.Add(new ServiceMapping {
                        MediaId = media.Id,
                        ServiceName = si.Key,
                        ServiceId = si.Value.ToString()
                    }));
            }
        }

        public static void InitMovie(Movie movie, INovaromaEngine engine) {
            FileInfo firstVideoFile = null;
            if (!string.IsNullOrWhiteSpace(movie.Directory) && Directory.Exists(movie.Directory)) {
                var files = new DirectoryInfo(movie.Directory).GetFiles("*", SearchOption.AllDirectories).ToList();
                firstVideoFile = files.FirstOrDefault(IsVideoFile);
            }

            if (firstVideoFile != null) {
                movie.BackgroundDownload = false;
                movie.NotFound = false;
                movie.FilePath = firstVideoFile.FullName;
                var subtitleFilePath = GetSubtitleFilePath(firstVideoFile);
                if (!string.IsNullOrEmpty(subtitleFilePath)) {
                    movie.BackgroundSubtitleDownload = false;
                    movie.SubtitleNotFound = false;
                    movie.SubtitleDownloaded = true;
                }
                else {
                    movie.BackgroundSubtitleDownload = engine.SubtitlesEnabled && (movie.Language == null || !engine.SubtitleLanguages.Contains(movie.Language.Value));
                    movie.SubtitleDownloaded = false;
                }
            }
            else {
                movie.BackgroundDownload = true;
                movie.BackgroundSubtitleDownload = engine.SubtitlesEnabled && (movie.Language == null || !engine.SubtitleLanguages.Contains(movie.Language.Value));
                movie.SubtitleDownloaded = false;
            }
        }

        public static void InitTvShow(TvShow tvShow, INovaromaEngine engine) {
            var episodes = tvShow.Seasons.SelectMany(s => s.Episodes).ToList();
            episodes.ForEach(e => {
                if (e.IsWatched) return;

                e.BackgroundDownload = true;
                e.BackgroundSubtitleDownload = engine.SubtitlesEnabled && (tvShow.Language == null || !engine.SubtitleLanguages.Contains(tvShow.Language.Value));
                e.SubtitleDownloaded = false;
            });

            if (string.IsNullOrWhiteSpace(tvShow.Directory) || !Directory.Exists(tvShow.Directory)) return;

            var files = new DirectoryInfo(tvShow.Directory).GetFiles("*", SearchOption.AllDirectories).ToList();
            var videoFiles = files.Where(IsVideoFile).ToList();

            foreach (var videoFile in videoFiles) {
                int? season, episode;
                DetectEpisodeInfo(videoFile, out season, out episode);
                if (!season.HasValue || !episode.HasValue) continue;

                var tvEpisode = episodes.FirstOrDefault(e => e.TvShowSeason.Season == season && e.Episode == episode.Value);
                if (tvEpisode == null) continue;

                tvEpisode.BackgroundDownload = false;
                tvEpisode.NotFound = false;
                tvEpisode.FilePath = videoFile.FullName;
                var subtitleFilePath = GetSubtitleFilePath(videoFile);
                if (!string.IsNullOrEmpty(subtitleFilePath)) {
                    tvEpisode.BackgroundSubtitleDownload = false;
                    tvEpisode.SubtitleNotFound = false;
                    tvEpisode.SubtitleDownloaded = true;
                }
            }
        }

        public static void SetDownloadProperties(string downloadKey, IDownloadable downloadable) {
            if (!string.IsNullOrEmpty(downloadKey)) {
                downloadable.BackgroundDownload = false;
                downloadable.DownloadKey = downloadKey;
                downloadable.NotFound = false;
            }
            else
                downloadable.NotFound = true;
        }

        public static void SetSubtitleDownloadProperties(bool? isDownloaded, IDownloadable downloadable) {
            if (!isDownloaded.HasValue) return;

            if (isDownloaded.Value) {
                downloadable.BackgroundSubtitleDownload = false;
                downloadable.SubtitleNotFound = false;
                downloadable.SubtitleDownloaded = true;
            }
            else
                downloadable.SubtitleNotFound = true;
        }

        public static IEnumerable GetEnumInfo(Type enumType) {
            var method = typeof(Helper).GetMethod("GetEnumInfo", new Type[] { });
            return method.MakeGenericMethod(enumType).Invoke(null, null) as IEnumerable;
        }

        public static IEnumerable<EnumInfo<TEnum>> GetEnumInfo<TEnum>() {
            var enumType = typeof(TEnum);
            var memberNames = Enum.GetNames(enumType);
            var infos = new List<EnumInfo<TEnum>>();
            foreach (var memberName in memberNames) {
                var enumItem = (TEnum)Enum.Parse(enumType, memberName);
                var enumValue = Convert.ToInt32(enumItem);
                var member = enumType.GetMember(memberName).First();
                var display = member.GetAttribute<DisplayAttribute>();
                infos.Add(new EnumInfo<TEnum>(enumItem, enumValue, member.Name, display));
            }
            return infos;
        }

        public static string GetTwoLetterLanguageCode(Language language) {
            switch (language) {
                case Language.Turkish: return "tr";
                case Language.English: return "en";
                case Language.German: return "de";
                case Language.French: return "fr";
                case Language.Italian: return "it";
                case Language.Dutch: return "nl";
                case Language.Russian: return "ru";
                default: return "en";
            }
        }

        public static string GetThreeLetterLanguageCode(Language language) {
            switch (language) {
                case Language.Turkish: return "tur";
                case Language.English: return "eng";
                case Language.German: return "deu";
                case Language.French: return "fra";
                case Language.Italian: return "ita";
                case Language.Dutch: return "nld";
                case Language.Russian: return "rus";
                default: return "eng";
            }
        }

        public static void MoveDirectory(string source, string destination, string deleteExtensionsStr, IEnumerable<string> files = null) {
            var sourceInfo = new DirectoryInfo(source);
            if (!sourceInfo.Exists) return;

            var sourceFiles = sourceInfo.GetFiles().ToList();
            if (files != null) {
                var fileList = files.ToList();
                if (fileList.Any())
                    sourceFiles = sourceFiles.Where(fi => fileList.Any(f => string.Equals(f, fi.Name, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!string.IsNullOrEmpty(deleteExtensionsStr)) {
                var deleteExtensions = deleteExtensionsStr.Split(';');
                sourceFiles = sourceFiles.Where(fi => deleteExtensions.All(e => !string.Equals(e.Trim(), fi.Extension, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (var file in sourceFiles) {
                var destinationFilePath = Path.Combine(destination, file.Name);
                if (!File.Exists(destinationFilePath))
                    file.MoveTo(destinationFilePath);
            }
        }

        public static string CombineUrls(params string[] parts) {
            var sb = new StringBuilder();

            foreach (var partTmp in parts) {
                var part = partTmp;

                if (part.StartsWith("/"))
                    part = part.Substring(1);

                sb.Append(part);
                if (!part.EndsWith("/"))
                    sb.Append("/");
            }

            return sb.ToString();
        }

        public static string JoinStrings(string separator, params string[] strings) {
            return string.Join(separator, strings.Where(s => !string.IsNullOrEmpty(s)));
        }

        public static void MakeSpecialFolder(Media media) {
            if (media.Poster == null) return;

            var directoryInfo = new DirectoryInfo(media.Directory);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            Image image;
            using (var ms = new MemoryStream(media.Poster))
                image = Image.FromStream(ms);
            var bitmap = new Bitmap(image);
            var iconHandle = bitmap.GetHicon();
            var icon = Icon.FromHandle(iconHandle);

            var genres = string.Join(" | ", media.Genres.Select(g => g.Name));
            MakeSpecialFolder(directoryInfo, icon, genres + " - " + media.Rating + " - " + media.VoteCount + " " + Resources.Votes);
        }

        public static void MakeSpecialFolder(DirectoryInfo directoryInfo, Icon icon, string description) {
            var iconPath = Path.Combine(directoryInfo.FullName, "Folder.ico");
            if (File.Exists(iconPath))
                File.Delete(iconPath);

            using (var fs = new FileStream(iconPath, FileMode.Create))
                icon.Save(fs);
            File.SetAttributes(iconPath, FileAttributes.Hidden);

            MakeSpecialFolder(directoryInfo, description);
        }

        private static void MakeSpecialFolder(DirectoryInfo directoryInfo, string description, string iconPath = null) {
            if (iconPath == null) iconPath = @".\Folder.ico";
            var iniContent = string.Format(
@"[.ShellClassInfo]
ConfirmFileOp=0
IconResource={0}
IconIndex=0
InfoTip={1}", iconPath, description);

            var desktopIniPath = Path.Combine(directoryInfo.FullName, "desktop.ini");
            if (File.Exists(desktopIniPath))
                File.Delete(desktopIniPath);

            File.WriteAllText(desktopIniPath, iniContent);
            File.SetAttributes(directoryInfo.FullName, FileAttributes.System);
            File.SetAttributes(desktopIniPath, FileAttributes.System | FileAttributes.Hidden);
        }

        public static void CreateMediaInfo(Media media) {
            var directoryInfo = new DirectoryInfo(media.Directory);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            CreateMediaInfo(media, directoryInfo);
        }

        private static void CreateMediaInfo(Media media, DirectoryInfo directoryInfo) {
            var infoPath = Path.Combine(directoryInfo.FullName, "novaroma.info");
            var serializerSettings = new JsonSerializerSettings {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects
            };

            var serializedMedia = JsonConvert.SerializeObject(media, serializerSettings);
            if (File.Exists(infoPath))
                File.Delete(infoPath);
            File.WriteAllText(infoPath, serializedMedia, Encoding.UTF8);
            File.SetAttributes(infoPath, FileAttributes.Hidden);
        }

        public static IShellService CreateShellServiceClient() {
            return CreateShellServiceClient(TimeSpan.FromSeconds(5));
        }

        public static IShellService CreateShellServiceClient(TimeSpan timeout) {
            var binding = new NetNamedPipeBinding {
                OpenTimeout = timeout,
                MaxReceivedMessageSize = 20000000,
                MaxBufferPoolSize = 20000000,
                MaxBufferSize = 20000000
            };
            const string endpointAddress = Constants.NetPipeUri + Constants.NetPipeEndpointName;
            var endpoint = new EndpointAddress(endpointAddress);
            var channelFactory = new ChannelFactory<IShellService>(binding, endpoint);
            return channelFactory.CreateChannel();
        }

        public async static Task RunTask(Func<Task> taskGetter, IExceptionHandler exceptionHandler,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            Exception exception;
            try {
                await taskGetter();
                return;
            }
            catch (Exception ex) {
                exception = ex;
                if (exceptionHandler == null)
                    throw;
            }

            // ReSharper disable ExplicitCallerInfoArgument
            exceptionHandler.HandleException(exception, callerName, callerFilePath, callerLine);
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public async static Task<TResult> RunTask<TResult>(Func<Task<TResult>> taskGetter, IExceptionHandler exceptionHandler,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            Exception exception;
            try {
                var result = await taskGetter();
                return result;
            }
            catch (Exception ex) {
                exception = ex;
                if (exceptionHandler == null) throw;
            }

            // ReSharper disable ExplicitCallerInfoArgument
            exceptionHandler.HandleException(exception, callerName, callerFilePath, callerLine);
            // ReSharper restore ExplicitCallerInfoArgument
            return default(TResult);
        }

        public static CompilerResults CompileCode(string code) {
            var provider = new CSharpCodeProvider();
            var compilerparams = new CompilerParameters { GenerateExecutable = false, GenerateInMemory = true };

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                try {
                    var location = assembly.Location;
                    if (!string.IsNullOrEmpty(location))
                        compilerparams.ReferencedAssemblies.Add(location);
                }
                catch (NotSupportedException) {
                }
            }

            return provider.CompileAssemblyFromSource(compilerparams, code);
        }
    }
}
