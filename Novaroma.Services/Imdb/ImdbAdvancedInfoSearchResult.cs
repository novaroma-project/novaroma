using System;
using Novaroma.Interface.Info;
using Novaroma.Properties;

namespace Novaroma.Services.Imdb {

    public class ImdbAdvancedInfoSearchResult : ImdbInfoSearchResult, IAdvancedInfoSearchResult {
        private readonly string _outline;
        private readonly string _credits;
        private readonly float? _rating;
        private readonly int? _numberOfVotes;
        private readonly int? _runtime;
        private readonly string _genres;

        public ImdbAdvancedInfoSearchResult(ImdbInfoProvider service, string id, string url, string title, byte[] poster, int? year, bool isTvShow,
            string outline, string credits, float? rating, int? numberOfVotes, int? runtime, string genres)
            : base(service, id, url, title, poster,
                string.Join(Environment.NewLine,
                    new[] {credits, runtime.HasValue ? runtime + " " + Resources.MinuteReduced + " - " + genres : string.Empty, outline}), year, isTvShow) {
            _outline = outline;
            _credits = credits;
            _rating = rating;
            _numberOfVotes = numberOfVotes;
            _runtime = runtime;
            _genres = genres;
        }

        public string Outline {
            get { return _outline; }
        }

        public string Credits {
            get { return _credits; }
        }

        public float? Rating {
            get { return _rating; }
        }

        public int? NumberOfVotes {
            get { return _numberOfVotes; }
        }

        public int? Runtime {
            get { return _runtime; }
        }

        public string Genres {
            get { return _genres; }
        }
    }
}
