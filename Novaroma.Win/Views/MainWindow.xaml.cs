using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Novaroma.Model;
using Novaroma.Model.Search;
using Novaroma.Win.ViewModels;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Novaroma.Win.Views {

    public partial class MainWindow {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel) {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;

            Application.Current.Exit += ApplicationOnExit;
        }

        protected override void OnContentRendered(EventArgs e) {
            base.OnContentRendered(e);

            if (!_viewModel.IsEngineInitialized)
                _viewModel.InitialConfiguration();
        }

        private void ApplicationOnExit(object sender, ExitEventArgs exitEventArgs) {
            if (MovieDetailFlyout.IsOpen)
                _viewModel.MovieSaveCommand.Execute(null);
            else if (TvShowDetailFlyout.IsOpen)
                _viewModel.TvShowSaveCommand.Execute(null);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            var scrollViewer = ((DependencyObject)sender).FindAncestor<ScrollViewer>();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        }

        private void MovieDetailFlyout_OnIsOpenChanged(object sender, RoutedEventArgs e) {
            if (!MovieDetailFlyout.IsOpen)
                MovieListView.SelectedItem = null;
        }

        private void TvShowDetailFlyout_OnIsOpenChanged(object sender, RoutedEventArgs e) {
            if (!TvShowDetailFlyout.IsOpen)
                TvShowListView.SelectedItem = null;
        }

        private void MoviePosterView_OnMouseEnter(object sender, MouseEventArgs e) {
            var listViewItem = ((Grid)sender).FindAncestor<ListViewItem>();
            _viewModel.FocusedMovie = (Movie)listViewItem.DataContext;

            var childStackPanel = (StackPanel)VisualTreeHelper.GetChild((Grid)sender, 1);
            childStackPanel.Visibility = Visibility.Visible;
        }

        private void MoviePosterView_OnMouseLeave(object sender, MouseEventArgs e) {
            _viewModel.FocusedMovie = null;

            var childStackPanel = (StackPanel)VisualTreeHelper.GetChild((Grid)sender, 1);
            childStackPanel.Visibility = Visibility.Hidden;
        }

        private void TvShowPosterView_OnMouseEnter(object sender, MouseEventArgs e) {
            var listViewItem = ((Grid)sender).FindAncestor<ListViewItem>();
            _viewModel.FocusedTvShow = (TvShow)listViewItem.DataContext;

            var childStackPanel = (StackPanel)VisualTreeHelper.GetChild((Grid)sender, 1);
            childStackPanel.Visibility = Visibility.Visible;
        }

        private void TvShowPosterView_OnMouseLeave(object sender, MouseEventArgs e) {
            _viewModel.FocusedTvShow = null;

            var childStackPanel = (StackPanel)VisualTreeHelper.GetChild((Grid)sender, 1);
            childStackPanel.Visibility = Visibility.Hidden;
        }

        private void TvShowFilterPanel_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && _viewModel.TvShowSearchCommand.CanExecute(null))
                _viewModel.TvShowSearchCommand.Execute(null);
        }

        private void TvShowOrderLink_OnClick(object sender, RoutedEventArgs e) {
            var hyperlink = (Hyperlink)sender;
            var orderSelection = (OrderSelection)hyperlink.DataContext;
            _viewModel.TvShowSearchModel.SelectedOrder = orderSelection;
        }

        private void MovieFilterPanel_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && _viewModel.MovieSearchCommand.CanExecute(null))
                _viewModel.MovieSearchCommand.Execute(null);
        }

        private void MovieOrderLink_OnClick(object sender, RoutedEventArgs e) {
            var hyperlink = (Hyperlink)sender;
            var orderSelection = (OrderSelection)hyperlink.DataContext;
            _viewModel.MovieSearchModel.SelectedOrder = orderSelection;
        }

        private void MoviePreviousPageButton_OnClick(object sender, RoutedEventArgs e) {
            _viewModel.MovieSearchModel.Page -= 1;
        }

        private void MovieNextPageButton_OnClick(object sender, RoutedEventArgs e) {
            _viewModel.MovieSearchModel.Page += 1;
        }

        private void TvShowPreviousPageButton_OnClick(object sender, RoutedEventArgs e) {
            _viewModel.TvShowSearchModel.Page -= 1;
        }

        private void TvShowNextPageButton_OnClick(object sender, RoutedEventArgs e) {
            _viewModel.TvShowSearchModel.Page += 1;
        }

        private void TvShowPageNumericUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) {
            if (TvShowPageNumericUpDown.Value < 1)
                TvShowPageNumericUpDown.Value = 1;
            else {
                if (TvShowPageNumericUpDown.Value > _viewModel.TvShowMaxPage)
                    TvShowPageNumericUpDown.Value = _viewModel.TvShowMaxPage;
            }
        }

        private void MoviePageNumericUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) {
            if (MoviePageNumericUpDown.Value < 1)
                MoviePageNumericUpDown.Value = 1;
            else {
                if (MoviePageNumericUpDown.Value > _viewModel.MovieMaxPage)
                    MoviePageNumericUpDown.Value = _viewModel.MovieMaxPage;
            }
        }

        private void ListView_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            var scrollViewer = ((DependencyObject)sender).FindAncestor<ScrollViewer>();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }
    }
}
