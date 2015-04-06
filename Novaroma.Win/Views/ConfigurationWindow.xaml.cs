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
using Novaroma.Interface;
using Novaroma.Services.UTorrent;
using Novaroma.Win.UserControls;
using Novaroma.Win.ViewModels;

namespace Novaroma.Win.Views {

    public partial class ConfigurationWindow {
        private readonly INovaromaEngine _engine;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IDialogService _dialogService;
        private readonly UTorrentDownloader _uTorrentDownloader;
        private readonly string _initialMovieDir;
        private readonly string _initialTvShowDir;

        private readonly INotifyPropertyChanged _engineSettings;
        private readonly UTorrentSettings _torrentSettings;

        public ConfigurationWindow(INovaromaEngine engine, IExceptionHandler exceptionHandler, IDialogService dialogService, UTorrentDownloader uTorrentDownloader) {
            InitializeComponent();

            _engine = engine;
            _exceptionHandler = exceptionHandler;
            _dialogService = dialogService;
            _uTorrentDownloader = uTorrentDownloader;
            _initialMovieDir = _engine.MovieDirectory;
            _initialTvShowDir = _engine.TvShowDirectory;

            _engineSettings = engine.Settings;
            _torrentSettings = uTorrentDownloader.Settings;

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

            await _engine.SaveSettings(_engine.SettingName, _engine.SerializeSettings());
            await _engine.SaveSettings(_uTorrentDownloader.SettingName, _uTorrentDownloader.SerializeSettings());

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

        public UTorrentSettings TorrentSettings {
            get { return _torrentSettings; }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }
    }
}
