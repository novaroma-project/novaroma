using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Novaroma.Win.UserControls {

    public partial class IconUserControl {

        public IconUserControl() {
            InitializeComponent();
        }

        private void IconUserControl_OnLoaded(object sender, RoutedEventArgs e) {
            if (double.IsNaN(Height)) Height = Width;
            if (double.IsNaN(IconWidth)) IconWidth = Width;
            if (double.IsNaN(IconHeight)) IconHeight = Height;
        }

        public static readonly DependencyProperty VisualProperty =
                DependencyProperty.Register("Visual", typeof(Canvas), typeof(IconUserControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty FillProperty =
                DependencyProperty.Register("Fill", typeof(SolidColorBrush), typeof(IconUserControl), new UIPropertyMetadata(new SolidColorBrush(Colors.Black)));

        public static readonly DependencyProperty IconWidthProperty =
                DependencyProperty.Register("IconWidth", typeof(double), typeof(IconUserControl), new UIPropertyMetadata(double.NaN));

        public static readonly DependencyProperty IconHeightProperty =
               DependencyProperty.Register("IconHeight", typeof(double), typeof(IconUserControl), new UIPropertyMetadata(double.NaN));

        public Canvas Visual {
            get { return (Canvas)GetValue(VisualProperty); }
            set { SetValue(VisualProperty, value); }
        }

        public SolidColorBrush Fill {
            get { return (SolidColorBrush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public double IconWidth {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public double IconHeight {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
    }
}
