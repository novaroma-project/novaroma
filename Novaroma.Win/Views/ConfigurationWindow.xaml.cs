using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using Novaroma.Engine;
using Novaroma.Interface;
using Novaroma.Services.Transmission;
using Novaroma.Services.UTorrent;
using Novaroma.Win.UserControls;
using Novaroma.Win.ViewModels;

namespace Novaroma.Win.Views {

    public partial class ConfigurationWindow {
        private readonly IConfigurable _configurableEngine;
        private readonly NovaromaEngine _engine;
        private readonly IConfigurable _downloader;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IDialogService _dialogService;
        private readonly string _initialMovieDir;
        private readonly string _initialTvShowDir;

        private readonly INotifyPropertyChanged _engineSettings;
        private readonly INotifyPropertyChanged _torrentSettings;

        public ConfigurationWindow(NovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService) {
            InitializeComponent();

            _configurableEngine = engine;
            _engine = engine;
            _downloader = engine.Settings.Downloader.SelectedItem as IConfigurable;
            _exceptionHandler = exceptionHandler;
            _dialogService = dialogService;
            _initialMovieDir = engine.MovieDirectory;
            _initialTvShowDir = engine.TvShowDirectory;

            _engineSettings = engine.Settings;

            var utor = _downloader as UTorrentDownloader;
            if (utor != null && utor.IsAvailable) {
                _torrentSettings = utor.Settings;
                TorrentHowToHyperink.NavigateUri = new Uri(Properties.Resources.Url_HowToConfigureUtorrentWebUISettings);
            }
            else {
                var trantor = _downloader as TransmissionDownloader;
                if (trantor != null) {
                    _torrentSettings = trantor.Settings;
                    TorrentHowToHyperink.NavigateUri = new Uri(Properties.Resources.Url_HowToConfigureTransmissionWebUISettings);
                }
            }
            if (_torrentSettings == null)
                TorrentSettingsExpander.Visibility = Visibility.Collapsed;
            else if (_downloader != null)
                TorrentSettingsExpander.Tag = string.Format(Properties.Resources.TorrentWebUISettings, _downloader.SettingName);

            DataContext = this;

            Closing += OnClosing;
        }

        private void SaveClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private async void OnClosing(object sender, CancelEventArgs cancelEventArgs) {
            var languageSelection = (ILateBindable)LanguageSelection.DataContext;
            languageSelection.AcceptChanges();
            var subtitleLanguagesSelection = (ILateBindable)SubtitleLanguagesSelection.DataContext;
            subtitleLanguagesSelection.AcceptChanges();

            // ReSharper disable PossibleNullReferenceException
            MovieDirectorySelection.GetBindingExpression(DirectorySelectUserControl.TextProperty).UpdateSource();
            TvShowDirectorySelection.GetBindingExpression(DirectorySelectUserControl.TextProperty).UpdateSource();
            UserNameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            PasswordTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            PortTextBox.GetBindingExpression(NumericUpDown.ValueProperty).UpdateSource();
            // ReSharper restore PossibleNullReferenceException

            await _engine.SaveSettings(_configurableEngine.SettingName, _configurableEngine.SerializeSettings());
            if (_downloader != null)
                await _engine.SaveSettings(_downloader.SettingName, _downloader.SerializeSettings());

            await AddDirectories(_engine.MovieDirectory, _initialMovieDir);
            await AddDirectories(_engine.TvShowDirectory, _initialTvShowDir);
        }

        private async Task AddDirectories(string directory, string initialDir) {
            if (string.Equals(directory, initialDir, StringComparison.OrdinalIgnoreCase)) return;
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) return;

            var subDirectories = Directory.GetDirectories(directory).ToArray();
            if (subDirectories.Any()) {
                var wizardViewModel = new NewMediaWizardViewModel(_engine, _exceptionHandler, _dialogService);
                await wizardViewModel.AddFromDirectories(subDirectories);
                var wizard = new NewMediaWizard(wizardViewModel);
                wizard.ForceShow();
            }
        }

        public INotifyPropertyChanged EngineSettings {
            get { return _engineSettings; }
        }

        public INotifyPropertyChanged TorrentSettings {
            get { return _torrentSettings; }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }
    }
}
