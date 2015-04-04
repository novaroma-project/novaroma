using Novaroma.Interface.Subtitle;
using OSDBnet;

namespace Novaroma.Services.OpenSubtitles {

    public class OpenSubtitleSearchResult: ISubtitleSearchResult {
        private readonly OpenSubtitleDownloader _service;
        private readonly Subtitle _subtitle;

        public OpenSubtitleSearchResult(OpenSubtitleDownloader service, Subtitle subtitle) {
            _service = service;
            _subtitle = subtitle;
        }

        public OpenSubtitleDownloader Service {
            get { return _service; }
        }

        public Subtitle Subtitle {
            get { return _subtitle; }
        }

        #region ISubtitleSearchResult Members

        ISubtitleDownloader ISubtitleSearchResult.Service {
            get { return Service; }
        }

        public string Name {
            get { return _subtitle.SubtitleFileName; }
        }

        public string Language {
            get { return _subtitle.LanguageName; }
        }

        public string Url {
            get { return _subtitle.SubTitleDownloadLink.AbsolutePath; }
        }

        public int DownloadCount {
            get { return 0; }
        }

        #endregion
    }
}
