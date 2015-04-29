using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class MediaGenre: EntityBase {
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name == value) return;

                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public override string ToString() {
            return Name;
        }

        protected override void CopyFrom(IEntity entity) {
            var external = Helper.ConvertTo<MediaGenre>(entity);

            CopyFrom(external);
        }

        internal void CopyFrom(MediaGenre genre) {
            Name = genre.Name;
        }
    }
}
