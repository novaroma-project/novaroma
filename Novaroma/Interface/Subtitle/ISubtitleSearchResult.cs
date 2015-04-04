using System.ComponentModel.DataAnnotations;
using Novaroma.Properties;

namespace Novaroma.Interface.Subtitle {

    public interface ISubtitleSearchResult {
        [Display(Name = "Provider", ResourceType = typeof(Resources))]
        ISubtitleDownloader Service { get; }
        [Display(Name = "Name", ResourceType = typeof(Resources))]
        string Name { get; }
        string Url { get; }
        [Display(Name = "Language", ResourceType = typeof(Resources))]
        string Language { get; }
        [Display(Name = "DownloadCount", ResourceType = typeof(Resources))]
        int DownloadCount { get; }
    }
}
