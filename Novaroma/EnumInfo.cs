using System.ComponentModel.DataAnnotations;
using Novaroma.Interface.Model;

namespace Novaroma {

    public class EnumInfo<TEnum>: ModelBase {
        private readonly TEnum _item;
        private readonly int _value;
        private readonly string _name;
        private readonly DisplayAttribute _displayAttribute;

        public EnumInfo(TEnum item, int value, string name, DisplayAttribute displayAttribute) {
            _item = item;
            _value = value;
            _name = name;
            _displayAttribute = displayAttribute;
        }

        public void RaiseResourceProperties() {
            RaisePropertyChanged("DisplayName");
        }

        public TEnum Item {
            get { return _item; }
        }

        public int Value {
            get { return _value; }
        }

        public string Name {
            get { return _name; }
        }

        public string DisplayName {
            get { return _displayAttribute == null ? _name : _displayAttribute.GetName(); }
        }

        public override string ToString() {
            return DisplayName;
        }
    }
}
