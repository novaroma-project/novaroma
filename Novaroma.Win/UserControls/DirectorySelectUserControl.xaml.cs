using System.Windows;
using System.Windows.Forms;

namespace Novaroma.Win.UserControls {

    public partial class DirectorySelectUserControl {

        public static DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(DirectorySelectUserControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public static DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description",
            typeof(string),
            typeof(DirectorySelectUserControl),
            new PropertyMetadata(null)
        );

        public string Text {
            get {
                return GetValue(TextProperty) as string;
            }
            set {
                SetValue(TextProperty, value);
            }
        }

        public string Description {
            get {
                return GetValue(DescriptionProperty) as string;
            }
            set {
                SetValue(DescriptionProperty, value);
            }
        }

        public DirectorySelectUserControl() {
            InitializeComponent();
        }

        private void BrowseFolder(object sender, RoutedEventArgs e) {
            using (var dlg = new FolderBrowserDialog()) {
                dlg.Description = Description;
                dlg.SelectedPath = Text;
                dlg.ShowNewFolderButton = true;
                var result = dlg.ShowDialog();
                if (result == DialogResult.OK) {
                    Text = dlg.SelectedPath;
                    var be = GetBindingExpression(TextProperty);
                    if (be != null)
                        be.UpdateSource();
                }
            }
        }
    }
}
