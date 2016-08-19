using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novaroma.Services.Rarbg {
    public class RarbgSettings : TorrentProviderSettingsBase {
        internal const string ImdbSearchQuery = "%imdbId%";

        public RarbgSettings() : base("https://rarbg.to/") {
            MovieSearchPattern = ImdbSearchQuery;
        }
    }
}
