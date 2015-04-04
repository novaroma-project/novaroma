using System.Collections.Generic;
using Novaroma.Interface.Info;

namespace Novaroma.Services.Imdb {

    public class ImdbTvShowInfo : ImdbMediaInfo, ITvShowInfo {

        public ImdbTvShowInfo(ImdbInfoProvider service, string id, string url, string imdbId, string title, string originalTitle, string outline, byte[] poster, int? year,
                              string credits, float? rating, int? voteCount, int? runtime, Language? language,
                              IEnumerable<string> genres, IDictionary<string, object> otherIds)
            : base(service, id, url, imdbId, title, originalTitle, outline, poster, year, credits, rating, voteCount, runtime, language, true, genres, otherIds) {
        }
    }
}
