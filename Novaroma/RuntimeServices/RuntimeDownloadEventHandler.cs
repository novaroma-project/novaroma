using Novaroma.Interface.EventHandler;
using Novaroma.Model;

namespace Novaroma.RuntimeServices {

    public class RuntimeDownloadEventHandler: RuntimeServiceBase<IDownloadEventHandler>, IDownloadEventHandler {

        public override string DefaultCode {
            get { return DEFAULT_CODE; }
        }

        #region IDownloadEventHandler Members

        public void MovieDownloaded(Movie movie) {
            Instance.MovieDownloaded(movie);
        }

        public void MovieSubtitleDownloaded(Movie movie) {
            Instance.MovieSubtitleDownloaded(movie);
        }

        public void TvShowEpisodeDownloaded(TvShowEpisode episode) {
            Instance.TvShowEpisodeDownloaded(episode);
        }

        public void TvShowEpisodeSubtitleDownloaded(TvShowEpisode episode) {
            Instance.TvShowEpisodeSubtitleDownloaded(episode);
        }

        #endregion

        public const string DEFAULT_CODE =
@"
using System;
using Novaroma.Interface.EventHandler;
using Novaroma.Model;

namespace Novaroma.MyServices {

    public class MyDownloadEventHandler : IDownloadEventHandler {

        #region IDownloadEventHandler Members

        public void MovieDownloaded(Movie movie) {
            throw new NotImplementedException();
        }

        public void MovieSubtitleDownloaded(Movie movie) {
            throw new NotImplementedException();
        }

        public void TvShowEpisodeDownloaded(TvShowEpisode episode) {
            throw new NotImplementedException();
        }

        public void TvShowEpisodeSubtitleDownloaded(TvShowEpisode episode) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyIDownloadEventHandler""; }
        }

        #endregion
    }
}
";
    }
}
