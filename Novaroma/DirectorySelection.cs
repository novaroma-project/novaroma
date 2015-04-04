using Novaroma.Interface.Model;

namespace Novaroma {

    public class DirectorySelection: ModelBase {
        private string _path;

        public DirectorySelection() {
            _path = string.Empty;
        }

        public string Path {
            get { return _path; }
            set {
                if (_path == value) return;

                _path = value;
                RaisePropertyChanged("Path");
            }
        }
    }
}
