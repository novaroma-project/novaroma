using System.Windows;

namespace Novaroma.Win.UserControls {
    public partial class VideoSearchInfoUserControl {
        public VideoSearchInfoUserControl() {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemWidthProperty =
                DependencyProperty.Register("ItemWidth", typeof(double), typeof(VideoSearchInfoUserControl), new UIPropertyMetadata(double.NaN));

        public static readonly DependencyProperty LabelWidthProperty =
                DependencyProperty.Register("LabelWidth", typeof(double), typeof(VideoSearchInfoUserControl), new UIPropertyMetadata(double.NaN));

        public double ItemWidth {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        public double LabelWidth {
            get { return (double)GetValue(LabelWidthProperty); }
            set { SetValue(LabelWidthProperty, value); }
        }
    }
}
