using System.Collections.Generic;

namespace Novaroma.Interface.Track {

    public interface ITvShowUpdate {

        bool IsActive { get; }
        string Status { get; }
        IEnumerable<ITvShowEpisodeInfo> UpdateEpisodes { get; }
    }
}
