using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Novaroma.Interface.Download.Torrent.Provider;
using Novaroma.Properties;

namespace Novaroma.Interface.Download.Torrent {

    public interface ITorrentSearchResult: IDownloadSearchResult {
        [Display(Name = "Provider", ResourceType = typeof(Resources))]
        ITorrentProvider Provider { get; }
        [Display(Name = "Name", ResourceType = typeof(Resources))]
        new string Name { get; }
        [Display(Name = "Seed", ResourceType = typeof(Resources))]
        int Seed { get; }
        [Display(Name = "Leech", ResourceType = typeof(Resources))]
        int Leech { get; }
        [Display(Name = "SizeMB", ResourceType = typeof(Resources))]
        double Size { get; }
        [Display(Name = "FileCount", ResourceType = typeof(Resources))]
        int? FileCount { get; }
        [Display(Name = "Age", ResourceType = typeof(Resources))]
        string Age { get; }
        string MagnetUri { get; }
        Task<byte[]> Download();
    }
}
