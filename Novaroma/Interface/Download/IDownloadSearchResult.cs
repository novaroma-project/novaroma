using System.ComponentModel.DataAnnotations;
using Novaroma.Properties;

namespace Novaroma.Interface.Download {
    
    public interface IDownloadSearchResult {
        IDownloader Service { get; }
        [Display(Name = "Name", ResourceType = typeof(Resources))]
        string Name { get; }
        string Url { get; }
        [Display(Name = "Availability", ResourceType = typeof(Resources))]
        int Availability { get; }
    }
}
