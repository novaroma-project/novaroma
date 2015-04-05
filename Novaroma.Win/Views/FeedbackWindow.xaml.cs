using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using Novaroma.Interface;

namespace Novaroma.Win.Views {

    public partial class FeedbackWindow : INotifyPropertyChanged, IDataErrorInfo {
        private static readonly Uri _feedbackUrl = new Uri("http://novaroma.azurewebsites.net/FeedbackHandler.ashx");
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private readonly bool _isFrown;
        private string _message;
        private MultiCheckSelection<ILogItem> _logs;

        public FeedbackWindow(IExceptionHandler exceptionHandler, ILogger logger, IDialogService dialogService, bool isFrown) {
            _exceptionHandler = exceptionHandler;
            _logger = logger;
            _dialogService = dialogService;
            _isFrown = isFrown;

            DataContext = this;

            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            MessageTextBox.Focus();

            var logs = await _logger.GetLogItems(new LogSearchModel { LogType = LogType.Error, MaxCount = 20 });
            Logs = new MultiCheckSelection<ILogItem>(logs);
            if (IsFrown)
                Logs.Selections.ToList().ForEach(l => l.IsSelected = true);
        }

        private void FeedbackWindow_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                Close();
        }

        private async void DoSendFeedback(object sender, RoutedEventArgs e) {
            await SendFeedback();
        }

        private async Task SendFeedback() {
            var mailCheck = IncludeEmailCheckBox.IsChecked.HasValue && IncludeEmailCheckBox.IsChecked.Value;

            var err = Error;
            if (!string.IsNullOrEmpty(err)) {
                await _dialogService.Error(Properties.Resources.MontyNi, err);
                return;
            }

            IsEnabled = false;
            ProgressRing.IsActive = true;

            using (var client = new NovaromaWebClient()) {
                var feedback = new {
                    IsFrown,
                    Message,
                    Logs = _logs.SelectedItems,
                    Email = mailCheck ? EmailTextBox.Text : string.Empty
                };
                try {
                    var feedbackJson = JsonConvert.SerializeObject(feedback);
                    await client.UploadStringTaskAsync(_feedbackUrl, feedbackJson);

                    Close();
                }
                catch (Exception ex) {
                    _exceptionHandler.HandleException(ex);

                    IsEnabled = true;
                    ProgressRing.IsActive = false;
                }
            }
        }

        public bool IsFrown {
            get { return _isFrown; }
        }

        public string FeedbackWatermark {
            get {
                return _isFrown
                    ? Properties.Resources.FrownWatermark
                    : Properties.Resources.SmileWatermark;
            }
        }

        public string SendButtonText {
            get {
                return _isFrown
                    ? Properties.Resources.SendAFrown
                    : Properties.Resources.SendASmile;
            }
        }

        public string Message {
            get { return _message; }
            set {
                _message = value;

                SendButton.IsEnabled = !string.IsNullOrEmpty(_message);
            }
        }

        public MultiCheckSelection<ILogItem> Logs {
            get { return _logs; }
            set {
                if (Equals(_logs, value)) return;

                _logs = value;
                RaisePropertyChanged("Logs");
            }
        }

        public string Email { get; set; }

        private void IncludeEmailCheckBox_Click(object sender, RoutedEventArgs e) {
            var emailChecked = IncludeEmailCheckBox.IsChecked.HasValue && IncludeEmailCheckBox.IsChecked.Value;
            EmailTextBox.IsEnabled = emailChecked;
            if (emailChecked)
                EmailTextBox.Focus();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDataErrorInfo Members

        public string Error {
            get {
                return this["Email"];
            }
        }

        public string this[string propertyName] {
            get {
                switch (propertyName) {
                    case "Email":
                        var emailChecked = IncludeEmailCheckBox.IsChecked.HasValue && IncludeEmailCheckBox.IsChecked.Value;
                        if (emailChecked && (string.IsNullOrEmpty(Email) || !Regex.IsMatch(Email, Constants.EmailRegex)))
                            return Properties.Resources.InvalidEmail;
                        return string.Empty;
                    default:
                        return string.Empty;
                }
            }
        }

        #endregion
    }
}
