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
        public static string[] SubtitleExtensions = { ".srt", ".sub", ".ass" };
        private static readonly IEnumerable<char> _paranthesis = new List<char> { '(', '[', '{' };

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

        public static void DetectEpisodeInfo(FileInfo fileInfo, TvShow tvShow, out int? season, out int? episode) {
            season = null;
            episode = null;

            var name = fileInfo.NameWithoutExtension();
            var titleRegex = tvShow.Title.Replace(" ", ".");
            name = Regex.Replace(name, titleRegex, string.Empty, RegexOptions.IgnoreCase);
            name = Regex.Replace(name, "480p|720p|1080p|x264", string.Empty, RegexOptions.IgnoreCase);

            Match match = null;
            var matches = Regex.Matches(name, @"(\d{1,2})\D*(\d{1,2})");
            if (matches.Count == 0)
                matches = Regex.Matches(name, @"(\d)");
            if (matches.Count > 1 && tvShow.Seasons.Max(s => s.Season) < 19) {
                for (var i = 0; i < matches.Count; i++) {
                    var r = matches[i].Groups[0].Value;
                    int y;
                    if (int.TryParse(r, out y) && (y > 1950 && y < DateTime.UtcNow.Year + 2)) continue;

                    match = matches[i];
                    break;
                }
                if (match == null) return;
            }
            else match = matches[0];

            int tmpSeason, tmpEpisode;
            var tmpSeasonStr = string.Empty;
            string tmpEpisodeStr;
            var matchStr = match.Groups[0].Value;

            if (matchStr.Length < 3) {
                if (fileInfo.Directory != null) {
                    var seasonResult = Regex.Match(fileInfo.Directory.Name, @"(\d{1,2})");
                    if (seasonResult.Success)
                        tmpSeasonStr = seasonResult.Groups[0].Value;
                    else season = 1;
                }
                tmpEpisodeStr = matchStr;
            }
            else if (matchStr.Length == 3) {
                tmpSeasonStr = matchStr.Substring(0, 1);
                tmpEpisodeStr = matchStr.Substring(1);
            }
            else if (matchStr.Length == 4) {
                tmpSeasonStr = matchStr.Substring(0, 2);
                tmpEpisodeStr = matchStr.Substring(2);
            }
            else {
                tmpSeasonStr = match.Groups[1].Value;
                tmpEpisodeStr = match.Groups[2].Value;
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

        public static string GetDirectorySearchQuery(string directory) {
            if (string.IsNullOrEmpty(directory)) return directory;

            var dir = directory;
            var minIdx = _paranthesis.Select(p => dir.IndexOf(p)).Where(i => i > -1).OrderBy(i => i).FirstOrDefault();
            if (minIdx > 0)
                directory = directory.Substring(0, minIdx);

            return directory.Replace(".", " ").Replace("_", " ");
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
                    movie.BackgroundSubtitleDownload = engine.SubtitlesNeeded(movie.Language);
                    movie.SubtitleDownloaded = false;
                }
            }
            else {
                movie.BackgroundDownload = true;
                movie.FilePath = string.Empty;
                movie.BackgroundSubtitleDownload = engine.SubtitlesNeeded(movie.Language);
                movie.SubtitleDownloaded = false;
            }
        }

        public static void InitTvShow(TvShow tvShow, INovaromaEngine engine) {
            var episodes = tvShow.Seasons.SelectMany(s => s.Episodes).ToList();
            episodes.ForEach(e => {
                if (!string.IsNullOrEmpty(e.FilePath) && !File.Exists(e.FilePath)) {
                    e.FilePath = string.Empty;
                    e.SubtitleDownloaded = false;
                }
                if (e.IsWatched) return;

                e.BackgroundDownload = true;
                e.BackgroundSubtitleDownload = engine.SubtitlesNeeded(tvShow.Language);
                e.SubtitleDownloaded = false;
            });

            if (string.IsNullOrWhiteSpace(tvShow.Directory) || !Directory.Exists(tvShow.Directory)) return;

            var files = new DirectoryInfo(tvShow.Directory).GetFiles("*", SearchOption.AllDirectories).ToList();
            var videoFiles = files.Where(IsVideoFile).ToList();

            foreach (var videoFile in videoFiles) {
                int? season, episode;
                DetectEpisodeInfo(videoFile, tvShow, out season, out episode);
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

        public static void RenameMovieFile(Movie movie, string template) {
            if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(movie.FilePath)) return;
            var fileInfo = new FileInfo(movie.FilePath);
            if (!fileInfo.Exists || fileInfo.DirectoryName == null) return;

            template = Regex.Replace(template, "%movieName%", movie.Title, RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%year%", movie.Year.ToString(), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%rating%", movie.Rating.ToString(), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%genres%", string.Join(", ", movie.Genres.Select(g => g.Name)), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%voteCount%", movie.VoteCount.ToString(), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%runtime%", movie.Runtime.ToString(), RegexOptions.IgnoreCase);
            var videQuality = movie.VideoQuality;
            if (videQuality != VideoQuality.Any) {
                var videoQualityStr = videQuality == VideoQuality.P720 ? Resources.P720 : Resources.P1080;
                template = Regex.Replace(template, "%videoQuality%", videoQualityStr, RegexOptions.IgnoreCase);
            }

            movie.FilePath = RenameVideoFile(fileInfo, template);
        }

        public static void RenameEpisodeFile(TvShowEpisode episode, string template) {
            if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(episode.FilePath)) return;
            var fileInfo = new FileInfo(episode.FilePath);
            if (!fileInfo.Exists) return;

            var season = episode.TvShowSeason;
            var tvShow = season.TvShow;
            template = Regex.Replace(template, "%showName%", tvShow.Title, RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%season%", season.Season.ToString("D2"), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%episode%", episode.Episode.ToString("D2"), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%episodeName%", episode.Name, RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%year%", episode.AirDate.HasValue ? episode.AirDate.Value.Year.ToString(CultureInfo.InvariantCulture) : string.Empty, RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%rating%", tvShow.Rating.ToString(), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%genres%", string.Join(", ", tvShow.Genres.Select(g => g.Name)), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%voteCount%", tvShow.VoteCount.ToString(), RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "%runtime%", tvShow.Runtime.ToString(), RegexOptions.IgnoreCase);
            var videQuality = tvShow.VideoQuality;
            if (videQuality != VideoQuality.Any) {
                var videoQualityStr = videQuality == VideoQuality.P720 ? Resources.P720 : Resources.P1080;
                template = Regex.Replace(template, "%videoQuality%", videoQualityStr, RegexOptions.IgnoreCase);
            }

            episode.FilePath = RenameVideoFile(fileInfo, template);
        }

        private static string RenameVideoFile(FileInfo videoFileInfo, string template) {
            if (videoFileInfo.DirectoryName == null) return videoFileInfo.FullName;

            template = MakeValidFileName(template);

            RenameSubtitleFile(videoFileInfo, template);

            var newFilePath = Path.Combine(videoFileInfo.DirectoryName, template) + videoFileInfo.Extension;
            if (File.Exists(newFilePath))
                File.Delete(newFilePath);
            videoFileInfo.MoveTo(newFilePath);
            return newFilePath;
        }

        private static void RenameSubtitleFile(FileInfo videoFileInfo, string template) {
            if (videoFileInfo.DirectoryName == null) return;

            var subtitleFilePath = GetSubtitleFilePath(videoFileInfo);
            if (string.IsNullOrEmpty(subtitleFilePath)) return;

            var subtitleFileInfo = new FileInfo(subtitleFilePath);
            if (!subtitleFileInfo.Exists) return;

            var newSubtitleFilePath = Path.Combine(videoFileInfo.DirectoryName, template) + subtitleFileInfo.Extension;
            if (File.Exists(newSubtitleFilePath))
                File.Delete(newSubtitleFilePath);
            subtitleFileInfo.MoveTo(newSubtitleFilePath);
        }

        public static string MakeValidFileName(string path, char replaceChar = '-') {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(path.Select(c => invalidChars.Contains(c) ? replaceChar : c).ToArray());
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
                var display = GetMemberAttribute<DisplayAttribute>(enumType, memberName);
                infos.Add(new EnumInfo<TEnum>(enumItem, enumValue, memberName, display));
            }
            return infos;
        }

        public static TAttribute GetMemberAttribute<TType, TAttribute>(string memberName, bool checkMetadataType = false, bool inherit = false) where TAttribute : Attribute {
            return GetMemberAttribute<TAttribute>(typeof(TType), memberName, checkMetadataType, inherit);
        }

        public static TAttribute GetMemberAttribute<TAttribute>(Type type, string memberName, bool checkMetadataType = false, bool inherit = false) where TAttribute : Attribute {
            var member = type.GetMember(memberName).First();
            return member.GetAttribute<TAttribute>(checkMetadataType, inherit);
        }

        public static string GetTwoLetterLanguageCode(Language language) {
            var langInfo = GetMemberAttribute<LanguageInfoAttribute>(typeof(Language), language.ToString());
            return langInfo.TwoLetterCode;
        }

        public static string GetThreeLetterLanguageCode(Language language) {
            var langInfo = GetMemberAttribute<LanguageInfoAttribute>(typeof(Language), language.ToString());
            return langInfo.ThreeLetterCode;
        }

        public static void CopyDirectory(string source, string destination, string deleteExtensionsStr, IEnumerable<string> files = null) {
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
                    file.CopyTo(destinationFilePath);
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
            try {
                await taskGetter();
            }
            catch (Exception ex) {
                if (exceptionHandler == null)
                    throw;

                // ReSharper disable ExplicitCallerInfoArgument
                exceptionHandler.HandleException(ex, callerName, callerFilePath, callerLine);
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        public async static Task<TResult> RunTask<TResult>(Func<Task<TResult>> taskGetter, IExceptionHandler exceptionHandler,
                [CallerMemberName] string callerName = null,
                [CallerFilePath] string callerFilePath = null,
                [CallerLineNumber] int callerLine = -1) {
            try {
                var result = await taskGetter();
                return result;
            }
            catch (Exception ex) {
                if (exceptionHandler == null)
                    throw;

                // ReSharper disable ExplicitCallerInfoArgument
                exceptionHandler.HandleException(ex, callerName, callerFilePath, callerLine);
                // ReSharper restore ExplicitCallerInfoArgument
            }

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
