using System;

namespace Novaroma.DTO {

    public class EpisodeDTO {
        public Guid TvShowId { get; set; }
        public DateTime? AirDate { get; set; }
        public int Episode { get; set; }
        public int Season { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
    }
}
