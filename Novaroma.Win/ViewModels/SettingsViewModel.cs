using System.ComponentModel;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Properties;
using Novaroma.Win.Utilities;

namespace Novaroma.Win.ViewModels {

    public class SettingsViewModel : ViewModelBase {
        private readonly INovaromaEngine _engine;
        private readonly IConfigurable _configurable;
        private readonly object _settings;
        private readonly string _initialValues;
        private readonly RelayCommand _editServiceSettingsCommand;

        public SettingsViewModel(INovaromaEngine engine, IDialogService dialogService, IConfigurable configurable): base(dialogService) {
            _engine = engine;
            _configurable = configurable;
            _settings = configurable.Settings;
            _initialValues = configurable.SerializeSettings();

            _editServiceSettingsCommand = new RelayCommand(EditServiceSettings);
        }
        
        public async Task<bool> Save() {
            var idei = _settings as IDataErrorInfo;
            if (idei != null) {
                var error = idei.Error;
                if (!string.IsNullOrEmpty(error)) {
                    await DialogService.Error(Resources.MontyNi, error);
                    return false;
                }
            }

            await _engine.SaveSettings(_configurable.SettingName, _configurable.SerializeSettings());
            return true;
        }

        public void Cancel() {
            _configurable.DeserializeSettings(_initialValues);
        }

        private void EditServiceSettings(object obj) {
            var configurable = obj as IConfigurable;
            if (configurable == null) return;

            Helper.EditSettings(_engine, DialogService, configurable);
        }

        public object Settings {
            get { return _settings; }
        }

        public string Title {
            get { return _configurable.SettingName + " " + Resources.Settings.ToLowerInvariant(); }
        }

        public RelayCommand EditServiceSettingsCommand {
            get { return _editServiceSettingsCommand; }
        }
    }
}
