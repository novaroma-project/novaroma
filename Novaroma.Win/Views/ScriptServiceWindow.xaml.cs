using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Novaroma.Interface;
using Novaroma.Model;
using Novaroma.RuntimeServices;
using Resx = Novaroma.Properties.Resources;

namespace Novaroma.Win.Views {

    public partial class ScriptServiceWindow : INotifyPropertyChanged {
        private readonly IDialogService _dialogService;
        private readonly IEnumerable<EnumInfo<ServiceType>> _serviceTypes = Novaroma.Helper.GetEnumInfo<ServiceType>();
        private readonly ObservableCollection<CompilerError> _errors = new ObservableCollection<CompilerError>();
        private readonly ObservableCollection<CompilerError> _warnings = new ObservableCollection<CompilerError>();
        private CompilerResults _compilerResults;

        public ScriptServiceWindow(IDialogService dialogService) {
            _dialogService = dialogService;
            InitializeComponent();

            DataContext = this;
            ServiceType = ServiceType.DownloadEventHandler;
            CodeEditor.Text = LoadCode();

            Closing += OnClosing;

            NameTextBox.Focus();
        }

        public ScriptServiceWindow(IDialogService dialogService, ScriptService scriptService) {
            _dialogService = dialogService;
            InitializeComponent();

            DataContext = this;

            ScriptName = scriptService.Name;
            CodeEditor.Text = scriptService.Code;

            Closing += OnClosing;
        }

        private void LoadDefaultCodeButton_Click(object sender, RoutedEventArgs e) {
            CodeEditor.Text = LoadCode();
        }

        private string LoadCode() {
            switch (ServiceType) {
                case ServiceType.DownloadEventHandler:
                    return RuntimeDownloadEventHandler.DEFAULT_CODE;
                case ServiceType.PluginService:
                    return RuntimePluginService.DEFAULT_CODE;
                case ServiceType.Downloader:
                    return RuntimeDownloader.DEFAULT_CODE;
                case ServiceType.InfoProvider:
                    return RuntimeInfoProvider.DEFAULT_CODE;
                case ServiceType.SubtitleDownloader:
                    return RuntimeSubtitleDownloader.DEFAULT_CODE;
                case ServiceType.TorrentMovieProvider:
                    return RuntimeTorrentMovieProvider.DEFAULT_CODE;
                case ServiceType.TorrentTvShowProvider:
                    return RuntimeTorrentTvShowProvider.DEFAULT_CODE;
                case ServiceType.ShowTracker:
                    return RuntimeShowTracker.DEFAULT_CODE;
                default: return RuntimePluginService.DEFAULT_CODE;
            }
        }

        private void BuildButton_OnClick(object sender, RoutedEventArgs e) {
            Build();
        }

        private void Build() {
            CompilerResults = Novaroma.Helper.CompileCode(CodeEditor.Text);
            CodeEditor.IsModified = false;

            if (CompilerResults.Errors.HasErrors)
                ErrorsFlyout.IsOpen = true;
        }

        private void ErrorDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            var selectedError = dataGrid.SelectedItem as CompilerError;
            if (selectedError == null) return;

            CodeEditor.ScrollTo(selectedError.Line, selectedError.Column);
            var line = CodeEditor.Document.Lines[selectedError.Line - 1];
            var offset = line.Offset;
            var length = line.Length;
            CodeEditor.Select(offset, length);

            ErrorsFlyout.IsOpen = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            if (CodeEditor.IsModified)
                Build();

            if (CompilerResults.Errors.HasErrors) {
                ErrorsFlyout.IsOpen = true;
                return;
            };
            var ass = CompilerResults.CompiledAssembly;

            var types = ass.ExportedTypes.Where(t => typeof(INovaromaService).IsAssignableFrom(t)).ToList();
            if (!types.Any()) {
                _dialogService.Error(Resx.MontyNi, Resx.CompiledCodeNovaromaServiceRequired);
                return;
            }

            if (types.Any(t => t.GetConstructor(Type.EmptyTypes) == null)) {
                _dialogService.Error(Resx.MontyNi, Resx.CompiledCodeParameterlessCtorRequired);
                return;
            }

            _shouldBeClosed = true;
            DialogResult = true;
            Close();
        }

        public void SetErrors() {
            _errors.Clear();
            _warnings.Clear();
            if (CompilerResults != null) {
                foreach (CompilerError error in CompilerResults.Errors) {
                    if (error.IsWarning)
                        _warnings.Add(error);
                    else
                        _errors.Add(error);
                }
            }
        }

        private bool _shouldBeClosed;
        private async void OnClosing(object sender, CancelEventArgs e) {
            if (_shouldBeClosed) return;

            e.Cancel = true;
            if (await _dialogService.Confirm(Resx.MontyNi, Resx.AreYouSure)) {
                _shouldBeClosed = true;
                Close();
            }
        }

        public IEnumerable<EnumInfo<ServiceType>> ServiceTypes {
            get { return _serviceTypes; }
        }

        public ServiceType ServiceType {
            get;
            set;
        }

        public CompilerResults CompilerResults {
            get { return _compilerResults; }
            private set {
                _compilerResults = value;

                SetErrors();
                RaisePropertyChanged("CompilerResults");
            }
        }

        public ObservableCollection<CompilerError> Errors {
            get { return _errors; }
        }

        public ObservableCollection<CompilerError> Warnings {
            get { return _warnings; }
        }

        [Required]
        public string ScriptName {
            get;
            set;
        }

        public string Code {
            get { return CodeEditor.Text; }
            set { CodeEditor.Text = value; }
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
