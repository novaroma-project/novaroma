using System;
using System.Collections.Generic;

namespace Novaroma.Interface.Download {

    public class DownloadCompletedEventArgs: EventArgs {
        private readonly string _downloadKey;
        private readonly string _downloadDirectory;
        private readonly IEnumerable<string> _files;

        public DownloadCompletedEventArgs(string downloadKey, string downloadDirectory, IEnumerable<string> files) {
            _downloadKey = downloadKey;
            _downloadDirectory = downloadDirectory;
            _files = files;
        }

        public string DownloadKey {
            get { return _downloadKey; }
        }

        public IEnumerable<string> Files {
            get { return _files; }
        }

        public string DownloadDirectory {
            get { return _downloadDirectory; }
        }

        public bool Found { get; set; }
    }
}
