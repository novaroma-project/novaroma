using System.Windows;
using System.Windows.Input;
using Novaroma.Win.ViewModels;

namespace Novaroma.Win.Views {

    public partial class SubtitleSearchWindow {
        private readonly SubtitleSearchViewModel _viewModel;

        public SubtitleSearchWindow(SubtitleSearchViewModel viewModel) {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;
            SearchTextBox.Focus();
        }

        private async void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            await _viewModel.Download();

            Close();
        }

        private async void Download_Button_Click(object sender, RoutedEventArgs e) {
            await _viewModel.Download();

            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
