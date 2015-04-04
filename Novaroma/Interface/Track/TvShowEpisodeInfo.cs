using System;

namespace Novaroma.Interface.Track {

    public class TvShowEpisodeInfo: ITvShowEpisodeInfo {
        private readonly int _season;
        private readonly int _episode;
        private readonly string _name;
        private readonly DateTime _airDate;
        private readonly string _overview;

        public TvShowEpisodeInfo(int season, int episode, string name, DateTime airDate, string overview) {
            _season = season;
            _episode = episode;
            _name = name;
            _airDate = airDate;
            _overview = overview;
        }

        public int Season {
            get { return _season; }
        }

        public int Episode {
            get { return _episode; }
        }

        public string Name {
            get { return _name; }
        }

        public DateTime AirDate {
            get { return _airDate; }
        }

        public string Overview {
            get { return _overview; }
        }
    }
}
