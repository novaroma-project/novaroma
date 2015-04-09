using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Properties;
using Novaroma.Win.Utilities;
using Novaroma.Win.Views;

namespace Novaroma.Win.ViewModels {

    public class NotifyIconViewModel : ViewModelBase {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger _logger;
        private readonly IEnumerable<IPluginService> _pluginServices;
        private readonly RelayCommand _showWindowCommand;
        private readonly RelayCommand _newMediaCommand;
        private readonly RelayCommand _watchDirectoryCommand;
        private readonly RelayCommand _discoverCommand;
        private readonly RelayCommand _manualDownloadCommand;
        private readonly RelayCommand _executePluginServiceCommand;
        private readonly RelayCommand _executeDownloadsCommand;
        private readonly RelayCommand _executeTvShowUpdatesCommand;
        private readonly RelayCommand _backupDatabaseCommand;
        private readonly RelayCommand _manageRuntimeServicesCommand;
        private readonly RelayCommand _clearLogsAndActivitiesCommand;
        private readonly RelayCommand _checkForUpdatesCommand;
        private readonly RelayCommand _exitApplicationCommand;

        public NotifyIconViewModel(INovaromaEngine engine, IExceptionHandler exceptionHandler, ILogger logger, IDialogService dialogService)
            : base(dialogService) {
            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _logger = logger;
            _pluginServices = engine.Services.OfType<IPluginService>();

            _showWindowCommand = new RelayCommand(ShowWindow);
            _newMediaCommand = new RelayCommand(NewMedia);
            _watchDirectoryCommand = new RelayCommand(WatchDirectory);
            _discoverCommand = new RelayCommand(Discover);
            _manualDownloadCommand = new RelayCommand(DoManualDownload);
            _executePluginServiceCommand = new RelayCommand(DoExecutePluginService);
            _executeDownloadsCommand = new RelayCommand(ExecuteDownloads);
            _executeTvShowUpdatesCommand = new RelayCommand(ExecuteTvShowUpdates);
            _backupDatabaseCommand = new RelayCommand(DoBackupDatabase);
            _manageRuntimeServicesCommand = new RelayCommand(ManageRuntimeServices);
            _clearLogsAndActivitiesCommand = new RelayCommand(DoClearLogsAndActivities);
            _checkForUpdatesCommand = new RelayCommand(CheckForUpdates);
            _exitApplicationCommand = new RelayCommand(ExitApplication);
        }

        private static void ShowWindow() {
            Helper.ShowMainWindow();
        }

        private void NewMedia() {
            Helper.NewMedia(_engine, _exceptionHandler, DialogService, string.Empty);
        }

        private void WatchDirectory() {
            Helper.WatchDirectory(_engine, _exceptionHandler, DialogService);
        }

        private void Discover() {
            Helper.DiscoverMedia(_engine, _exceptionHandler, DialogService);
        }

        private async void DoManualDownload() {
            await ManualDownload();
        }

        private Task ManualDownload() {
            return Helper.ManualDownload(_engine, _exceptionHandler, DialogService);
        }

        private async void DoExecutePluginService(object prm) {
            var plugin = prm as IPluginService;
            if (plugin == null) return;

            await ExecutePluginService(plugin);
        }

        private async Task ExecutePluginService(IPluginService plugin) {
            try {
                await plugin.Activate();
            } catch (Exception ex) {
                _exceptionHandler.HandleException(ex);
            }
        }

        private void ExecuteDownloads() {
            _engine.ExecuteDownloadJob();
        }

        private void ExecuteTvShowUpdates() {
            _engine.ExecuteTvShowUpdateJob();
        }

        private async void DoBackupDatabase() {
            await BackupDatabase();
        }

        private async Task BackupDatabase() {
            var path = DialogService.SaveFileDialog(Resources.SelectAFile, Constants.Novaroma, ".db");
            if (string.IsNullOrEmpty(path)) return;

            await _engine.BackupDatabase(path);
        }

        private void ManageRuntimeServices() {
            new ScriptServicesWindow(_engine, DialogService).ShowDialog();
        }

        private async void DoClearLogsAndActivities() {
            await ClearLogsAndActivities();
        }

        private async Task ClearLogsAndActivities() {
            await _logger.Clear();
            await _engine.ClearActivities();
        }

        private static void CheckForUpdates() {
            var updaterPath = Path.Combine(Environment.CurrentDirectory, "Novaroma.Updater.exe");
            if (File.Exists(updaterPath))
                Process.Start(updaterPath,"/checknow");
        }

        private static void ExitApplication() {
            Helper.ExitApplication();
        }

        public IEnumerable<IPluginService> PluginServices {
            get { return _pluginServices; }
        }

        public bool HasPlugin {
            get { return _pluginServices.Any(); }
        }

        public RelayCommand ShowWindowCommand {
            get { return _showWindowCommand; }
        }

        public RelayCommand NewMediaCommand {
            get { return _newMediaCommand; }
        }

        public RelayCommand WatchDirectoryCommand {
            get { return _watchDirectoryCommand; }
        }

        public RelayCommand DiscoverCommand {
            get { return _discoverCommand; }
        }

        public RelayCommand ManualDownloadCommand {
            get { return _manualDownloadCommand; }
        }

        public RelayCommand ExecutePluginServiceCommand {
            get { return _executePluginServiceCommand; }
        }

        public RelayCommand ExecuteDownloadsCommand {
            get { return _executeDownloadsCommand; }
        }

        public RelayCommand ExecuteTvShowUpdatesCommand {
            get { return _executeTvShowUpdatesCommand; }
        }

        public RelayCommand BackupDatabaseCommand {
            get { return _backupDatabaseCommand; }
        }

        public RelayCommand ManageRuntimeServicesCommand {
            get { return _manageRuntimeServicesCommand; }
        }

        public RelayCommand ClearLogsAndActivitiesCommand {
            get { return _clearLogsAndActivitiesCommand; }
        }

        public RelayCommand CheckForUpdatesCommand {
            get { return _checkForUpdatesCommand; }
        }

        public RelayCommand ExitApplicationCommand {
            get { return _exitApplicationCommand; }
        }
    }
}
