namespace Novaroma.Interface.Info {

    public interface IInfoSearchResult {
        IInfoProvider Service { get; }
        string Id { get; }
        string Url { get; }
        string ImdbId { get; }
        string Title { get; }
        string Description { get; }
        byte[] Poster { get; }
        int? Year { get; }
        bool IsTvShow { get; }
    }
}