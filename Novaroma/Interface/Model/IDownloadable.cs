using Novaroma.Model;

namespace Novaroma.Interface.Model {
    
     public interface IDownloadable {
         bool BackgroundDownload { get; set; }
         string DownloadKey { get; set; }
         bool NotFound { get; set; }
         string FilePath { get; set; }
         bool BackgroundSubtitleDownload { get; set; }
         bool SubtitleNotFound { get; set; }
         bool SubtitleDownloaded { get; set; }
         bool IsWatched { get; set; }
         VideoQuality VideoQuality { get; }
         string ExtraKeywords { get; }
         string ExcludeKeywords { get; }
         int? MinSize { get; }
         int? MaxSize { get; }
         Media Media { get; }
         string GetSearchQuery();
     }
}
