using System.Collections;
using System.Windows.Input;

namespace Novaroma.Win.UserControls {

    public partial class MultiSelectionUserControl {

        public MultiSelectionUserControl() {
            InitializeComponent();
        }

        private void NotSelectedListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selectedItem = NotSelectedListView.SelectedItem;
            if (selectedItem != null) {
                ((IList)NotSelectedListView.ItemsSource).Remove(selectedItem);
                ((IList)SelectedListView.ItemsSource).Add(selectedItem);
            }
        }

        private void SelectedListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selectedItem = SelectedListView.SelectedItem;
            if (selectedItem != null) {
                ((IList)SelectedListView.ItemsSource).Remove(selectedItem);
                ((IList)NotSelectedListView.ItemsSource).Add(selectedItem);
            }
        }
    }
}
