using System;

namespace Novaroma.Interface.Track {

    public interface ITvShowEpisodeInfo {
        int Season { get; }
        int Episode { get; }
        string Name { get; }
        DateTime? AirDate { get; }
        string Overview { get; }
    }
}
