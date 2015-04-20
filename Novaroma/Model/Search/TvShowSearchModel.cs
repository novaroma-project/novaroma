using System.Collections.ObjectModel;

namespace Novaroma.Model.Search {

    public class TvShowSearchModel : MediaSearchModel {
        private bool? _ended;

        public TvShowSearchModel(ObservableCollection<string> mediaGenres)
            : base(mediaGenres) {
        }

        protected override string SettingName {
            get { return "TvShowSearchModel"; }
        }

        public bool? Ended {
            get { return _ended; }
            set {
                if (_ended == value) return;

                _ended = value;
                RaisePropertyChanged("Ended");
            }
        }
    }
}
