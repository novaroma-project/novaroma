using System.Windows.Input;
using Novaroma.Win.ViewModels.SearchMedia;

namespace Novaroma.Win.UserControls {

    public partial class AdvancedSearchUserControl {
    
        public AdvancedSearchUserControl() {
            InitializeComponent();
        }

        private void AdvancedSearchUserControl_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            var viewModel = ((AdvancedInfoSearchViewModel)DataContext);

            if (e.Key == Key.Enter && viewModel.SearchCommand.CanExecute(null))
                viewModel.SearchCommand.Execute(null);
        }
    }
}
