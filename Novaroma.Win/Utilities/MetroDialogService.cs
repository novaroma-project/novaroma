using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Novaroma.Interface;
using Sharpen.IO;

namespace Novaroma.Win.Utilities {

    public class MetroDialogService : IDialogService {

        public async Task Information(string title, string message) {
            var window = GetActiveWindow();
            if (window == null) return;

            await window.ShowMessageAsync(title, message);
        }

        public async Task<bool> Confirm(string title, string message) {
            var window = GetActiveWindow();
            if (window == null) return true;

            var result = await window.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative);
            return result != MessageDialogResult.Negative;
        }

        public async Task Error(string title, string message) {
            var window = GetActiveWindow();
            if (window == null) return;

            await window.ShowMessageAsync(title, message);
        }

        public string SaveFileDialog(string title, string fileName, string extension, string filter = null, string currentDirectory = null) {
            if (string.IsNullOrEmpty(currentDirectory)) {
                var fileInfo = new FileInfo(fileName);

                currentDirectory = Environment.CurrentDirectory;
            }

            var dialog = new SaveFileDialog {
                Title = title,
                FileName = fileName,
                DefaultExt = extension,
                AddExtension = true,
                InitialDirectory = currentDirectory
            };

            if (!string.IsNullOrEmpty(filter))
                dialog.Filter = filter;

            return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : string.Empty;
        }

        private static MetroWindow GetActiveWindow() {
            return Application.Current.Windows.OfType<MetroWindow>().SingleOrDefault(x => x.IsActive);
        }
    }
}
