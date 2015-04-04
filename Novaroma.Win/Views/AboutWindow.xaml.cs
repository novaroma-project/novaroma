using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Novaroma.Win.Views {

    public partial class AboutWindow {

        public AboutWindow() {
            InitializeComponent();

            var app = Assembly.GetExecutingAssembly();

            var product = ((AssemblyProductAttribute)app.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            var copyright = ((AssemblyCopyrightAttribute)app.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            var version = app.GetName().Version.ToString(2);

            ProductTextBlock.Text = product;
            VersionTextBlock.Text = "v" + version;
            CopyrightTextBlock.Text = copyright;
            DescriptionTextBlock.Text = Properties.Resources.NovaromaSlogan;

            WriteQuote();
        }

        private void WriteQuote() {
            var idx = new Random().Next(Constants.Quotes.Count);
            var quote = Constants.Quotes.Keys.ElementAt(idx);
            var quoteDetail = Constants.Quotes[Constants.Quotes.Keys.ElementAt(idx)];
            QuoteTextBlock.Text = quote;
            QuoteDetailTextBlock.Text = quoteDetail;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }

        private void RefreshQuoteClick(object sender, RoutedEventArgs e) {
            WriteQuote();
        }

        private void AboutWindow_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
