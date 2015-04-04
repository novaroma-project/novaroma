using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novaroma.Interface.Subtitle {

    public interface ISubtitleDownloader : INovaromaService {
        Task<IEnumerable<ISubtitleSearchResult>> SearchForMovie(string name, string videoFilePath, Language[] languages, string imdbId = null);
        Task<bool> DownloadForMovie(string name, string videoFilePath, Language[] languages, string imdbId = null);
        Task<IEnumerable<ISubtitleSearchResult>> SearchForTvShowEpisode(string name, int season, int episode, string videoFilePath, Language[] languages, string imdbId = null);
        Task<bool> DownloadForTvShowEpisode(string name, int season, int episode, string videoFilePath, Language[] languages, string imdbId = null);
        Task<IEnumerable<ISubtitleSearchResult>> Search(string query, Language[] languages, string imdbId = null);
        Task<bool> Download(string videoFilePath, ISubtitleSearchResult searchResult);
        Task<bool> Download(string videoFilePath, Language[] languages, string imdbId = null);
    }
}
