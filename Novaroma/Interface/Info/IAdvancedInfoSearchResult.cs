namespace Novaroma.Interface.Info {

    public interface IAdvancedInfoSearchResult: IInfoSearchResult {
        string Outline { get; }
        string Credits { get; }
        float? Rating { get; }
        int? VoteCount { get; }
        int? Runtime { get; }
        string Genres { get; }
    }
}
