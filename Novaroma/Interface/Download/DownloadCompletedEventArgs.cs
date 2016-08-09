using System;
using System.Collections.Generic;

namespace Novaroma.Interface.Download {

    public class DownloadCompletedEventArgs: EventArgs {
        private readonly string _downloadKey;
        private readonly string _downloadDirectory;
        private readonly IEnumerable<string> _files;
        private readonly bool _downloadOnly;

        public DownloadCompletedEventArgs(string downloadKey, string downloadDirectory, IEnumerable<string> files, bool downloadOnly) {
            _downloadKey = downloadKey;
            _downloadDirectory = downloadDirectory;
            _files = files;
            _downloadOnly = downloadOnly;
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

        public bool DownloadOnly {
            get { return _downloadOnly; }
        }

        public bool Found { get; set; }

        public bool Moved { get; set; }
    }
}
