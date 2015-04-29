using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class ScriptService: EntityBase {
        private string _name;
        private string _code;

        public string Name {
            get { return _name; }
            set {
                if (_name == value) return;

                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string Code {
            get { return _code; }
            set {
                if (_code == value) return;

                _code = value;
                RaisePropertyChanged("Code");
            }
        }

        protected override void CopyFrom(IEntity entity) {
            var external = Helper.ConvertTo<ScriptService>(entity);

            Name = external.Name;
            Code = external.Code;
        }
    }
}
