using System.Collections.Generic;

namespace Novaroma.Interface.Info {

    public interface IMediaInfo {
        IInfoProvider Service { get; }
        string Id { get; }
        string Url { get; }
        string ImdbId { get; }
        string Title { get; }
        string OriginalTitle { get; }
        string Outline { get; }
        byte[] Poster { get; }
        int? Year { get; }
        string Credits { get; }
        float? Rating { get; }
        int? VoteCount { get; }
        int? Runtime { get; }
        Language? Language { get; }
        bool IsTvShow { get; }
        IEnumerable<string> Genres { get; }
        IDictionary<string, object> ServiceIds { get; }
    }
}
