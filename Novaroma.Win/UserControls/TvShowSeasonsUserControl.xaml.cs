using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Novaroma.Win.UserControls {

    public partial class TvShowSeasonsUserControl {
    
        public TvShowSeasonsUserControl() {
            InitializeComponent();
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            var scrollViewer = ((DependencyObject)sender).FindAncestor<ScrollViewer>();
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        }
    }
}
