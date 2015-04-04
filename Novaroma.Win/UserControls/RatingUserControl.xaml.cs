using System.Windows;

namespace Novaroma.Win.UserControls {

    public partial class RatingUserControl {

        public RatingUserControl() {
            InitializeComponent();
        }

        public static readonly DependencyProperty RatingSizeProperty =
                DependencyProperty.Register("FontSize", typeof(double), typeof(IconUserControl), new UIPropertyMetadata(double.NaN));

        public double RatingSize {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
    }
}
