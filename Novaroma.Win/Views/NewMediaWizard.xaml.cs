using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Novaroma.Win.UserControls;
using Novaroma.Win.ViewModels;
using Resx = Novaroma.Properties.Resources;

namespace Novaroma.Win.Views {

    public partial class NewMediaWizard {
        private readonly NewMediaWizardViewModel _viewModel;

        public NewMediaWizard(NewMediaWizardViewModel viewModel) {
            InitializeComponent();

            DataContext = viewModel;
            _viewModel = viewModel;

            DirectorySelectUserControl.Loaded += DirectorySelectUserControlOnLoaded;
            SearchItemsControl.Loaded += SearchItemsControlOnLoaded;
            DiscoverItemsControl.Loaded += DiscoverItemsControlOnLoaded;
            Closing += OnClosing;
        }

        private void DirectorySelectUserControlOnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            DirectorySelectUserControl.TextBox.Focus();
        }

        private void SearchItemsControlOnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            var contentPresenter = (ContentPresenter)SearchItemsControl.ItemContainerGenerator.ContainerFromIndex(0);
            var searchUserControl = contentPresenter.FindChild<SimpleSearchUserControl>("SimpleSearchUserControl");
            if (searchUserControl != null)
                searchUserControl.SearchTextBox.Focus();
        }

        private void DiscoverItemsControlOnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            var contentPresenter = (ContentPresenter)DiscoverItemsControl.ItemContainerGenerator.ContainerFromIndex(0);
            var searchUserControl = contentPresenter.FindChild<AdvancedSearchUserControl>("AdvancedSearchUserControl");
            if (searchUserControl != null)
                searchUserControl.SearchTextBox.Focus();
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e) {
            var i = TabControl.SelectedIndex;

            if ((i == 0 && string.IsNullOrEmpty(_viewModel.SelectedDirectory))
                || (i == 1 && (_viewModel.Searches == null || !_viewModel.Searches.Any(s => s.IsSelected)))
                || (i == 2 && (_viewModel.SelectedSearches == null || !_viewModel.SelectedSearches.Any(s => s.IsSelected && s.Search.SearchInitialized)))
                || (i == 3 && (_viewModel.SelectedSearches == null || !_viewModel.SelectedSearches.Any(s => s.IsSelected && s.Search.SearchInitialized)))
                || (i == 4 && (_viewModel.EditList == null || !_viewModel.EditList.Any()))) {
                TabControl.SelectedIndex = 5;
                return;
            }

            do i++; while (TabControl.Items.Cast<TabItem>().ElementAt(i).Visibility != Visibility.Visible);
            TabControl.SelectedIndex = i;
        }

        private void Prev_Button_Click(object sender, RoutedEventArgs e) {
            var i = TabControl.SelectedIndex;
            do i--; while (TabControl.Items.Cast<TabItem>().ElementAt(i).Visibility != Visibility.Visible);
            TabControl.SelectedIndex = i;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private bool _shouldBeClosed;
        private async void Save_Button_Click(object sender, RoutedEventArgs e) {
            IsEnabled = false;
            SaveProgressRing.IsActive = true;
            SaveProgressRing.BringIntoView();
            await _viewModel.Save();
            _shouldBeClosed = true;
            Close();
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            var scrollViewer = ((DependencyObject)sender).FindAncestor<ScrollViewer>();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        }

        private async void OnClosing(object sender, CancelEventArgs e) {
            if (_shouldBeClosed || _viewModel.Searches == null) return;
            if (_viewModel.Searches.Count() == 1) {
                var search = _viewModel.Searches.First();
                if (search.Search.Results == null)
                    return;
            }
            
            e.Cancel = true;

            if (await _viewModel.Confirm(Resx.MontyNi, Resx.AreYouSure)) {
                _shouldBeClosed = true;
                Close();
            }
        }
    }
}
