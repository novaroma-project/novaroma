using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

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

        protected override Dictionary<string, object> GetSettingsDictionary() {
            var d = base.GetSettingsDictionary();
            d.Add("Ended", Ended);
            return d;
        }

        protected override void SetSettingsFromJson(JObject json) {
            base.SetSettingsFromJson(json);
            Ended = (bool?)json["Ended"];
        }
    }
}
