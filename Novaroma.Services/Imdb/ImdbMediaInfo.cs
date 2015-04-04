using System.Collections.Generic;
using System.Linq;
using Novaroma.Interface.Info;

namespace Novaroma.Services.Imdb {

    public class ImdbMediaInfo : IMediaInfo {
        private readonly ImdbInfoProvider _service;
        private readonly string _id;
        private readonly string _url;
        private readonly string _imdbId;
        private readonly string _title;
        private readonly string _originalTitle;
        private readonly string _outline;
        private readonly byte[] _poster;
        private readonly int? _year;
        private readonly string _credits;
        private readonly float? _rating;
        private readonly int? _voteCount;
        private readonly int? _runtime;
        private readonly Language? _language;
        private readonly bool _isTvShow;
        private readonly IEnumerable<string> _genres;
        private readonly IDictionary<string, object> _serviceIds;

        public ImdbMediaInfo(ImdbInfoProvider service, string id, string url, string imdbId, string title, string originalTitle, string outline, byte[] poster, int? year,
                             string credits, float? rating, int? voteCount, int? runtime, Language? language, bool isTvShow, 
                             IEnumerable<string> genres, IDictionary<string, object> serviceIds) {
            _service = service;
            _id = id;
            _url = url;
            _imdbId = imdbId;
            _title = title;
            _originalTitle = originalTitle;
            _outline = outline;
            _poster = poster;
            _year = year;
            _credits = credits;
            _rating = rating;
            _voteCount = voteCount;
            _runtime = runtime;
            _language = language;
            _isTvShow = isTvShow;
            _genres = genres ?? Enumerable.Empty<string>();
            _serviceIds = serviceIds ?? new Dictionary<string, object>();
        }

        public ImdbInfoProvider Service {
            get { return _service; }
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

        public string OriginalTitle {
            get { return _originalTitle; }
        }

        public string Outline {
            get { return _outline; }
        }

        public byte[] Poster {
            get { return _poster; }
        }

        public int? Year {
            get { return _year; }
        }

        public string Credits {
            get { return _credits; }
        }

        public float? Rating {
            get { return _rating; }
        }

        public int? VoteCount {
            get { return _voteCount; }
        }

        public int? Runtime {
            get { return _runtime; }
        }

        public Language? Language {
            get { return _language; }
        }

        public bool IsTvShow {
            get { return _isTvShow; }
        }

        public IEnumerable<string> Genres {
            get { return _genres; }
        }

        public IDictionary<string, object> ServiceIds {
            get { return _serviceIds; }
        }

        #region IMediaInfo Members

        IInfoProvider IMediaInfo.Service {
            get { return Service; }
        }

        #endregion
    }
}
