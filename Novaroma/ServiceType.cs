using System.ComponentModel.DataAnnotations;
using Novaroma.Properties;

namespace Novaroma {

    public enum ServiceType {
        [Display(Name = "DownloadEventHandler", ResourceType = typeof(Resources))]
        DownloadEventHandler,
        [Display(Name = "PluginService", ResourceType = typeof(Resources))]
        PluginService,
        [Display(Name = "SubtitleDownloader", ResourceType = typeof(Resources))]
        SubtitleDownloader,
        [Display(Name = "TvShowTorrentProvider", ResourceType = typeof(Resources))]
        TorrentTvShowProvider,
        [Display(Name = "MovieTorrentProvider", ResourceType = typeof(Resources))]
        TorrentMovieProvider,
        [Display(Name = "InfoProvider", ResourceType = typeof(Resources))]
        InfoProvider,
        [Display(Name = "Downloader", ResourceType = typeof(Resources))]
        Downloader,
        [Display(Name = "ShowTracker", ResourceType = typeof(Resources))]
        ShowTracker,
    }
}
