using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Info;

namespace Novaroma.RuntimeServices {

    public class RuntimeInfoProvider: RuntimeServiceBase<IInfoProvider>, IInfoProvider {
        public override string DefaultCode {
            get { return DEFAULT_CODE; }
        }

        #region IInfoProvider Members

        public Task<IEnumerable<IInfoSearchResult>> Search(string query, Language language = Language.English) {
            return Instance.Search(query, language);
        }

        public Task<IMovieInfo> GetMovie(IInfoSearchResult searchResult, Language language = Language.English) {
            return Instance.GetMovie(searchResult, language);
        }

        public Task<IMovieInfo> GetMovie(string id, Language language = Language.English) {
            return Instance.GetMovie(id, language);
        }

        public Task<ITvShowInfo> GetTvShow(IInfoSearchResult searchResult, Language language = Language.English) {
            return Instance.GetTvShow(searchResult, language);
        }

        public Task<ITvShowInfo> GetTvShow(string id, Language language = Language.English) {
            return Instance.GetTvShow(id, language);
        }

        #endregion

        public const string DEFAULT_CODE =
@"
using System;
using Novaroma.Interface.Info;

namespace Novaroma.MyServices {

    public class MyInfoProvider : IInfoProvider {

        #region IInfoProvider Members

        public System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<IInfoSearchResult>> Search(string query, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<IMovieInfo> GetMovie(IInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<IMovieInfo> GetMovie(string id, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<ITvShowInfo> GetTvShow(IInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<ITvShowInfo> GetTvShow(string id, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyInfoProvider""; }
        }

        #endregion
    }
}
";
    }
}
