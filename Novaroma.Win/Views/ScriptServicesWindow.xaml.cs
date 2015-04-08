using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Novaroma.Interface;
using Novaroma.Model;
using Novaroma.Win.Utilities;

namespace Novaroma.Win.Views {

    public partial class ScriptServicesWindow : INotifyPropertyChanged {
        private readonly INovaromaEngine _engine;
        private readonly IDialogService _dialogService;
        private readonly RelayCommand _deleteScriptServiceCommand;
        private NovaromaObservableCollection<ScriptService> _scriptServices;
        private bool _isModified;

        public ScriptServicesWindow(INovaromaEngine engine, IDialogService dialogService) {
            _engine = engine;
            _dialogService = dialogService;
            _deleteScriptServiceCommand = new RelayCommand(DoDeleteScriptService);

            InitializeComponent();
            DataContext = this;

            Loaded += async (sender, args) => await LoadData();
        }

        private async Task LoadData() {
            var scriptServices = await _engine.GetScriptServices();
            ScriptServices = new NovaromaObservableCollection<ScriptService>(scriptServices);
        }

        private async void AddButton_OnClick(object sender, RoutedEventArgs e) {
            var window = new ScriptServiceWindow(_dialogService);
            var result = window.ShowDialog();
            if (result.HasValue && result.Value) {
                var scriptService = new ScriptService();
                scriptService.Name = window.ScriptName;
                scriptService.Code = window.Code;

                await _engine.InsertEntity(scriptService);
                ScriptServices.Add(scriptService);
                IsModified = true;
            }
        }

        private void PluginButton_OnClick(object sender, RoutedEventArgs e) {
            var pluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");
            if (Directory.Exists(pluginDirectory))
                Process.Start(pluginDirectory);
        }

        private async void ScriptServicesDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (SelectedScriptService == null) return;

            var window = new ScriptServiceWindow(_dialogService, SelectedScriptService);
            var result = window.ShowDialog();
            if (result.HasValue && result.Value) {
                SelectedScriptService.Name = window.ScriptName;
                SelectedScriptService.Code = window.Code;

                await _engine.UpdateEntity(SelectedScriptService);
                IsModified = true;
            }
        }

        private async void DoDeleteScriptService(object oScriptService) {
            var scriptService = oScriptService as ScriptService;
            if (scriptService == null) return;

            await DeleteScriptService(scriptService);
        }

        private async Task DeleteScriptService(ScriptService scriptService) {
            if (!await _dialogService.Confirm(Novaroma.Properties.Resources.MontyNi, Novaroma.Properties.Resources.AreYouSure))
                return;

            await _engine.DeleteEntity(scriptService);
            _scriptServices.Remove(scriptService);
            IsModified = true;
        }

        private void RestartNovaromaButton_Click(object sender, RoutedEventArgs e) {
            // very clever snippet by Bali C: http://stackoverflow.com/questions/9603926/restart-an-application-by-itself
            var info = new ProcessStartInfo {
                Arguments = "/C ping 127.0.0.1 -n 2 && \"" + Application.ResourceAssembly.Location + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(info);
            Helper.ExitApplication();
        }

        public RelayCommand DeleteScriptServiceCommand {
            get { return _deleteScriptServiceCommand; }
        }

        public NovaromaObservableCollection<ScriptService> ScriptServices {
            get { return _scriptServices; }
            set {
                _scriptServices = value;

                RaisePropertyChanged("ScriptServices");
            }
        }

        public ScriptService SelectedScriptService {
            get;
            set;
        }

        public bool IsModified {
            get { return _isModified; }
            set {
                _isModified = value;

                RaisePropertyChanged("IsModified");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
