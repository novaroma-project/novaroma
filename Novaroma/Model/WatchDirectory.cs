using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class WatchDirectory: EntityBase {
        private string _directory;

        public string Directory {
            get { return _directory; }
            set {
                if (_directory == value) return;

                _directory = value;
                RaisePropertyChanged("Directory");
            }
        }

        protected override void CopyFrom(IEntity entity) {
            var external = Helper.ConvertTo<WatchDirectory>(entity);

            Directory = external.Directory;
        }
    }
}
