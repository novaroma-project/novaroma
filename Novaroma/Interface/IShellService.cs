using System.ServiceModel;
using System.Threading.Tasks;
using Novaroma.Interface.Model;
using Novaroma.Model;

namespace Novaroma.Interface {

    [ServiceContract]
    public interface IShellService {

        [OperationContract]
        void Test();

        [OperationContract]
        Task<Language> GetSelectedLanguage();

        [OperationContract]
        Task ShowMainWindow();

        [OperationContract]
        Task HandleExeArgs(string[] args);

        [OperationContract]
        Task<DirectoryWatchStatus> GetDirectoryWatchStatus(string directory);

        [OperationContract]
        Task WatchDirectory(string directory);

        [OperationContract]
        Task StopWatching(string directory);

        [OperationContract]
        Task AddMedia(string[] directories);

        [OperationContract]
        Task NewMedia(string parentDirectory = null);

        [OperationContract]
        Task DiscoverMedia(string parentDirectory = null);

        [OperationContract]
        [ServiceKnownType(typeof(TvShow))]
        [ServiceKnownType(typeof(Movie))]
        [ReferencePreservingDataContractFormatAttribute]
        Task<Media> GetMedia(string directory);

        [OperationContract]
        Task<Movie> GetMovie(string directory);

        [OperationContract]
        [ReferencePreservingDataContractFormatAttribute]
        Task<TvShow> GetTvShow(string directory);
        
        [OperationContract]
        [ServiceKnownType(typeof(TvShowEpisode))]
        [ServiceKnownType(typeof(Movie))]
        [ReferencePreservingDataContractFormatAttribute]
        Task<IDownloadable> GetDownloadable(string filePath);

        [OperationContract]
        Task<Movie> GetMovieByFile(string filePath);

        [OperationContract]
        [ReferencePreservingDataContractFormatAttribute]
        Task<TvShowEpisode> GetTvShowEpisode(string filePath);

        [OperationContract]
        Task UpdateMovieWatchStatus(string directory, bool isWatched);

        [OperationContract]
        Task UpdateDownloadableWatchStatus(string filePath, bool isWatched);

        [OperationContract]
        Task DownloadMovie(string directory);

        [OperationContract]
        Task DownloadSubtitle(string filePath);

        [OperationContract]
        Task EditMedia(string directory);
    }
}
