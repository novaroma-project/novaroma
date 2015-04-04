using System.Threading.Tasks;
using Novaroma.Interface.Info;
using Novaroma.Model;

namespace Novaroma.Win.Infrastructure {

    public interface IInfoSearchMediaViewModel<out TSearchResult> where TSearchResult: IInfoSearchResult {
        TSearchResult SearchResult { get; }
        Media Media { get; }
        Task DownloadMedia();
    }
}
