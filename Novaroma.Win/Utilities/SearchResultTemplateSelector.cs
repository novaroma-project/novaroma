using System.Windows;
using System.Windows.Controls;
using Novaroma.Interface.Info;
using Novaroma.Win.Infrastructure;

namespace Novaroma.Win.Utilities {

    public class SearchResultTemplateSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var presenter = (ContentPresenter)container;
            var viewModel = item as IInfoSearchMediaViewModel<IInfoSearchResult>;

            if (viewModel != null && viewModel.SearchResult is IAdvancedInfoSearchResult)
                return (DataTemplate) presenter.FindResource("AdvancedSearchResultDataTemplate");

            return (DataTemplate)presenter.FindResource("SearchResultDataTemplate");
        }
    }
}
