using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using System.Web;
using Novaroma.DTO;
using Novaroma.Interface;

namespace Novaroma.Engine {

    public class WebUIService : IWebUIService {
        private readonly INovaromaEngine _engine;

        public WebUIService(INovaromaEngine engine) {
            _engine = engine;
        }

        [WebInvoke(UriTemplate = "/api/GetUnseenEpisodes", Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public async Task<IEnumerable<TvShowDTO>> GetUnseenEpisodes() {
            var episodes = await _engine.GetUnseenEpisodes();
            return episodes
                .GroupBy(e => e.TvShowSeason.TvShow, (t, es) =>
                    new TvShowDTO {
                        Id = t.Id,
                        Poster = Convert.ToBase64String(t.Poster),
                        Title = t.Title,
                        Episodes = es.Select(e => new EpisodeDTO {
                            TvShowId = t.Id,
                            AirDate = e.AirDate,
                            Episode = e.Episode,
                            Name = e.Name,
                            Overview = e.Overview,
                            Season = e.TvShowSeason.Season,
                        }).ToList()
                    });
        }

        [WebInvoke(UriTemplate = "/api/ExecuteDownloads", Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void ExecuteDownloads() {
            _engine.ExecuteDownloadJob();
        }

        [WebInvoke(UriTemplate = "/{*arguments}", Method = "GET", BodyStyle = WebMessageBodyStyle.Bare)]
        public Stream Get(string arguments) {
            if (WebOperationContext.Current == null) return null;

            if (string.IsNullOrEmpty(arguments)) arguments = "index.html";
            var path = Path.Combine(Environment.CurrentDirectory, "WebUI", arguments);
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists) return null;

            WebOperationContext.Current.OutgoingResponse.ContentType = MimeMapping.GetMimeMapping(path);

            return fileInfo.OpenRead();
        }
    }
}
