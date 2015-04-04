using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novaroma.Interface.Download {

    public interface IDownloader: INovaromaService {

        Task<IEnumerable<IDownloadSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
            string extraKeywords = null, string excludeKeywords = null);
     
        Task<string> DownloadMovie(string path, string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any,
            string extraKeywords = null, string excludeKeywords = null);
        
        Task<IEnumerable<IDownloadSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName = null, string imdbId = null,
            VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null);
       
        Task<string> DownloadTvShowEpisode(string path, string name, int season, int episode, string episodeName = null, string imdbId = null,
            VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null);
        
        Task<IEnumerable<IDownloadSearchResult>> Search(string query, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null);
       
        Task<string> Download(string path, IDownloadSearchResult searchResult);
        
        Task Refresh();

        event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;
    }
}
