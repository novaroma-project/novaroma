using Novaroma.Model;

namespace Novaroma.Interface.EventHandler {

    public interface IDownloadEventHandler: INovaromaService {
        void MovieDownloaded(Movie movie);
        void MovieSubtitleDownloaded(Movie movie);
        void TvShowEpisodeDownloaded(TvShowEpisode episode);
        void TvShowEpisodeSubtitleDownloaded(TvShowEpisode episode);
    }
}
