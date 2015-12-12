using System.ComponentModel.DataAnnotations;
using Novaroma.Interface.Subtitle;
using Novaroma.Properties;
using OSDBnet;

namespace Novaroma.Services.OpenSubtitles {

    public class OpenSubtitleSearchResult : ISubtitleSearchResult {
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

        [Display(Name = "Provider", ResourceType = typeof(Resources))]
        ISubtitleDownloader ISubtitleSearchResult.Service {
            get { return Service; }
        }

        [Display(Name = "Name", ResourceType = typeof(Resources))]
        public string Name {
            get { return _subtitle.SubtitleFileName; }
        }

        [Display(Name = "Language", ResourceType = typeof(Resources))]
        public string Language {
            get { return _subtitle.LanguageName; }
        }

        public string Url {
            get { return _subtitle.SubTitleDownloadLink.AbsolutePath; }
        }

        [Display(Name = "DownloadCount", ResourceType = typeof(Resources))]
        public int DownloadCount {
            get { return 0; }
        }

        #endregion
    }
}
