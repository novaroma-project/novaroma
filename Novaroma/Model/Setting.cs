using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class Setting: EntityBase {
        private string _settingName;
        private string _value;

        public string SettingName {
            get { return _settingName; }
            set {
                if (_settingName == value) return;

                _settingName = value;
                RaisePropertyChanged("SettingName");
            }
        }

        public string Value {
            get { return _value; }
            set {
                if (_value == value) return;

                _value = value;
                RaisePropertyChanged("Value");
            }
        }
    }
}
