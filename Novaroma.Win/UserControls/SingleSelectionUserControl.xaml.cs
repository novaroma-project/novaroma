using System.Windows.Controls;

namespace Novaroma.Win.UserControls {

    public partial class SingleSelectionUserControl {

        public SingleSelectionUserControl() {
            InitializeComponent();
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var listBox = sender as ListBox;
            if (listBox == null) return;

            if (listBox.SelectedItems.Count == 0 && e.RemovedItems.Count > 0) {
                e.Handled = true;
                var item = e.RemovedItems[0];
                listBox.SelectedItem = item;
            }
        }
    }
}
