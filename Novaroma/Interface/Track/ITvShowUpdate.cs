using System;
using System.Collections.Generic;

namespace Novaroma.Interface.Track {

    public interface ITvShowUpdate {
        DateTime UpdateDate { get; }
        bool IsActive { get; }
        string Status { get; }
        IEnumerable<ITvShowEpisodeInfo> UpdateEpisodes { get; }
    }
}
