using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Info;

namespace Novaroma.RuntimeServices {

    public class RuntimeAdvancedInfoProvider: RuntimeServiceBase<IAdvancedInfoProvider>, IAdvancedInfoProvider {

        public override string DefaultCode {
            get { return DEFAULT_CODE; }
        }

        #region IAdvancedInfoProvider Members

        public IEnumerable<string> Genres {
            get { return Instance.Genres; }
        }

        public Task<IEnumerable<IAdvancedInfoSearchResult>> AdvancedSearch(
                string query, MediaTypes mediaTypes = MediaTypes.All, int? releaseYearStart = null,
                int? releaseYearEnd = null, float? ratingMin = null, float? ratingMax = null, 
                int? numberOfVotesMin = null, int? numberOfVotesMax = null,
                int? runtimeMin = null, int? runtimeMax = null, 
                IEnumerable<string> genres = null, Language language = Language.English) {
            return Instance.AdvancedSearch(query, mediaTypes, releaseYearStart, releaseYearEnd, ratingMin, ratingMax, numberOfVotesMin, numberOfVotesMax,
                runtimeMin, runtimeMax, genres, language);
        }

        public Task<IMovieInfo> GetMovie(IAdvancedInfoSearchResult searchResult, Language language = Language.English) {
            return Instance.GetMovie(searchResult, language);
        }

        public Task<ITvShowInfo> GetTvShow(IAdvancedInfoSearchResult searchResult, Language language = Language.English) {
            return Instance.GetTvShow(searchResult, language);
        }

        #endregion

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

        private const string DEFAULT_CODE =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Info;

namespace Novaroma.MyServices {

    public class MyAdvancedInfoProvider : IAdvancedInfoProvider {

        #region IAdvancedInfoProvider Members

        public IEnumerable<string> Genres {
            get { throw new NotImplementedException(); }
        }

        public Task<IEnumerable<IAdvancedInfoSearchResult>> AdvancedSearch(string query, MediaTypes mediaTypes = MediaTypes.All, int? releaseYearStart = null, int? releaseYearEnd = null, 
                                                                           float? ratingMin = null, float? ratingMax = null, int? numberOfVotesMin = null, int? numberOfVotesMax = null, 
                                                                           int? runtimeMin = null, int? runtimeMax = null, IEnumerable<string> genres = null, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<IMovieInfo> GetMovie(IAdvancedInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<ITvShowInfo> GetTvShow(IAdvancedInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        #endregion

        #region IInfoProvider Members

        public Task<IEnumerable<IInfoSearchResult>> Search(string query, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<IMovieInfo> GetMovie(IInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<IMovieInfo> GetMovie(string id, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<ITvShowInfo> GetTvShow(IInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<ITvShowInfo> GetTvShow(string id, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyAdvancedInfoProvider""; }
        }

        #endregion
    }
}
";
    }
}
