using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Novaroma.Interface.Download.Torrent.Provider;
using Novaroma.Properties;

namespace Novaroma.Interface.Download.Torrent {

    public class TorrentSearchResult: ITorrentSearchResult {
        private readonly ITorrentDownloader _service;
        private readonly ITorrentProvider _provider;
        private readonly string _url;
        private readonly string _name;
        private readonly int _seed;
        private readonly int _leech;
        private readonly string _size;
        private readonly int? _files;
        private readonly string _age;
        private readonly string _magnetUri;
        private readonly Func<TorrentSearchResult, Task<byte[]>> _download;

        public TorrentSearchResult(ITorrentDownloader service, ITorrentProvider provider, string url, string name, int seed, int leech, string size, 
                                   int? files, string age, string magnetUri, Func<TorrentSearchResult, Task<byte[]>> download) {
            _service = service;
            _provider = provider;
            _url = url;
            _name = name;
            _seed = seed;
            _leech = leech;
            _size = size;
            _files = files;
            _age = age;
            _magnetUri = magnetUri;
            _download = download;
        }

        #region ITorrentSearchResult Members

        public ITorrentDownloader Service {
            get { return _service; }
        }

        [Display(Name = "Provider", ResourceType = typeof(Resources))]
        public ITorrentProvider Provider {
            get { return _provider; }
        }

        [Display(Name = "Name", ResourceType = typeof(Resources))]
        public string Name {
            get { return _name; }
        }

        [Display(Name = "Seed", ResourceType = typeof(Resources))]
        public int Seed {
            get { return _seed; }
        }

        [Display(Name = "Leech", ResourceType = typeof(Resources))]
        public int Leech {
            get { return _leech; }
        }

        [Display(Name = "Size", ResourceType = typeof(Resources))]
        public string Size {
            get { return _size; }
        }

        [Display(Name = "FileCount", ResourceType = typeof(Resources))]
        public int? FileCount {
            get { return _files; }
        }

        [Display(Name = "Age", ResourceType = typeof(Resources))]
        public string Age {
            get { return _age; }
        }

        public string MagnetUri {
            get { return _magnetUri; }
        }

        public Task<byte[]> Download() {
            return _download(this);
        }

        #endregion

        #region IDownloadSearchResult Members

        IDownloader IDownloadSearchResult.Service {
            get { return Service; }
        }

        public string Url {
            get { return _url; }
        }

        int IDownloadSearchResult.Availability {
            get { return _seed; }
        }

        #endregion
    }
}
