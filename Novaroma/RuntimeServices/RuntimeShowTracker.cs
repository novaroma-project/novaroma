using System;
using System.Threading.Tasks;
using Novaroma.Interface.Track;

namespace Novaroma.RuntimeServices {

    public class RuntimeShowTracker: RuntimeServiceBase<IShowTracker>, IShowTracker {
        public override string DefaultCode {
            get { return DEFAULT_CODE; }
        }

        #region IShowTracker Members

        public Task<ITvShowUpdate> GetTvShowUpdate(string id, DateTime? lastUpdate = null, Language language = Language.English) {
            return Instance.GetTvShowUpdate(id, lastUpdate, language);
        }

        public Task<ITvShowUpdate> GetTvShowUpdateByImdbId(string imdbId, DateTime? lastUpdate = null, Language language = Language.English) {
            return Instance.GetTvShowUpdateByImdbId(imdbId, lastUpdate, language);
        }

        #endregion

        public const string DEFAULT_CODE =
@"
using System;
using System.Threading.Tasks;
using Novaroma.Interface.Track;

namespace Novaroma.MyServices {

    public class MyShowTracker : IShowTracker {


        #region IShowTracker Members

        public Task<ITvShowUpdate> GetTvShowUpdate(string id, DateTime? lastUpdate = null, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<ITvShowUpdate> GetTvShowUpdateByImdbId(string imdbId, DateTime? lastUpdate = null, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyShowTracker""; }
        }

        #endregion
    }
}";
    }
}
