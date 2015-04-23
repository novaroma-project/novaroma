using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Novaroma.Win.ViewModels;

namespace Novaroma.Win.Views {

    public partial class DownloadSearchWindow {
        private readonly DownloadSearchViewModel _viewModel;

        public DownloadSearchWindow(DownloadSearchViewModel viewModel) {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;
            SearchTextBox.Focus();
        }

        private async void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            await Download();
        }

        private async void Download_Button_Click(object sender, RoutedEventArgs e) {
            await Download();
        }

        private async Task Download() {
            IsEnabled = false;
            var result = await _viewModel.Download();
            if (!string.IsNullOrEmpty(result))
                Close();
            else 
                IsEnabled = true;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
