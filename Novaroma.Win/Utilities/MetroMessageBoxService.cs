using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Novaroma.Interface;

namespace Novaroma.Win.Utilities {

    public class MetroMessageBoxService: IDialogManager {
        private readonly MetroWindow _window;

        public MetroMessageBoxService(MetroWindow window) {
            _window = window;
        }

        #region IMessageBoxService Members

        public Task ShowMessage(string title, string message) {
            return _window.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative);
        }

        public Task ShowWarning(string title, string message) {
            return _window.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, new MetroDialogSettings(){})
        }

        public Task ShowError(string title, string error, string detail) {
        }

        public Task<bool> ShowConfirm(string title, string question) {
        }

        #endregion
    }
}
