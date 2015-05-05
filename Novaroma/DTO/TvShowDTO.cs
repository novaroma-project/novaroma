using System.Collections.Generic;

namespace Novaroma.DTO {

    public class TvShowDTO: MediaDTO {
        public IList<EpisodeDTO> Episodes { get; set; }
    }
}
