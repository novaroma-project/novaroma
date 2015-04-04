using System.Collections.Generic;

namespace Novaroma.Interface.Track {

    public class TvShowUpdate: ITvShowUpdate {
        private readonly bool _isActive;
        private readonly string _status;
        private readonly IEnumerable<ITvShowEpisodeInfo> _updateEpisodes;

        public TvShowUpdate(bool isActive, string status, IEnumerable<ITvShowEpisodeInfo> episodes) {
            _isActive = isActive;
            _status = status;
            _updateEpisodes = episodes;
        }

        public bool IsActive {
            get { return _isActive; }
        }

        public string Status {
            get { return _status; }
        }

        public IEnumerable<ITvShowEpisodeInfo> UpdateEpisodes {
            get { return _updateEpisodes; }
        }
    }
}
