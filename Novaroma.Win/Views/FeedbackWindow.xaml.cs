using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using Novaroma.Interface;

namespace Novaroma.Win.Views {

    public partial class FeedbackWindow : INotifyPropertyChanged {
        private static readonly Uri _feedbackUrl = new Uri("http://novaroma.azurewebsites.net/FeedbackHandler.ashx");
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private readonly bool _isFrown;
        private MultiCheckSelection<ILogItem> _logs;
        private string _email;

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

            if (mailCheck && !IsValidEmail(_email)) {
                await _dialogService.Error(Properties.Resources.MontyNi, Properties.Resources.InvalidEmail);
                return;
            }

            IsEnabled = false;
            ProgressRing.IsActive = true;

            using (var client = new NovaromaWebClient()) {
                var feedback = new {
                    IsFrown,
                    Message = MessageTextBox.Text,
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

        public MultiCheckSelection<ILogItem> Logs {
            get { return _logs; }
            set {
                if (Equals(_logs, value)) return;

                _logs = value;
                RaisePropertyChanged("Logs");
            }
        }

        public string Email {
            get { return _email; }
            set {
                _email = value;

                CheckEmail(value);
            }
        }

        private void IncludeEmailCheckBox_Click(object sender, RoutedEventArgs e) {
            var emailChecked = IncludeEmailCheckBox.IsChecked.HasValue && IncludeEmailCheckBox.IsChecked.Value;
            EmailTextBox.IsEnabled = emailChecked;
            if (emailChecked)
                EmailTextBox.Focus();
        }

        private static void CheckEmail(string mail) {
            // ReSharper disable once ObjectCreationAsStatement
            new MailAddress(mail);
        }

        private bool IsValidEmail(string mail) {
            try {
                CheckEmail(mail);
                return true;
            }
            catch {
                return false;
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
