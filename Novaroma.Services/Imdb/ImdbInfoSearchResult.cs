using Novaroma.Interface.Info;

namespace Novaroma.Services.Imdb {

    public class ImdbInfoSearchResult : IInfoSearchResult {
        private readonly ImdbInfoProvider _service;
        private readonly string _id;
        private readonly string _url;
        private readonly string _imdbId;
        private readonly string _title;
        private readonly string _description;
        private readonly byte[] _poster;
        private readonly int? _year;
        private readonly bool _isTvShow;

        public ImdbInfoSearchResult(ImdbInfoProvider service, string id, string url, string title, byte[] poster, string description, int? year, bool isTvShow) {
            _service = service;
            _id = id;
            _url = url;
            _imdbId = Id;
            _title = title;
            _description = description;
            _poster = poster;
            _year = year;
            _isTvShow = isTvShow;
        }

        public ImdbInfoProvider Service {
            get { return _service; }
        }

        #region IInfoSearchResult Members

        IInfoProvider IInfoSearchResult.Service {
            get { return Service; }
        }

        public string Id {
            get { return _id; }
        }

        public string Url {
            get { return _url; }
        }

        public string ImdbId {
            get { return _imdbId; }
        }

        public string Title {
            get { return _title; }
        }

        public string Description {
            get { return _description; }
        }

        public byte[] Poster {
            get { return _poster; }
        }

        public int? Year {
            get { return _year; }
        }

        public bool IsTvShow {
            get { return _isTvShow; }
        }

        #endregion
    }
}
