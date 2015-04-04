using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
