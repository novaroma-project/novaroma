using System;
using System.Collections.Generic;

namespace Novaroma.Interface.Track {

    public class TvShowUpdate: ITvShowUpdate {
        private readonly DateTime _updateDate;
        private readonly bool _isActive;
        private readonly string _status;
        private readonly IEnumerable<ITvShowEpisodeInfo> _updateEpisodes;

        public TvShowUpdate(DateTime updateDate, bool isActive, string status, IEnumerable<ITvShowEpisodeInfo> episodes) {
            _updateDate = updateDate;
            _isActive = isActive;
            _status = status;
            _updateEpisodes = episodes;
        }

        public DateTime UpdateDate {
            get { return _updateDate; }
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
