using System.ComponentModel.DataAnnotations;
using Novaroma.Interface.Model;
using Novaroma.Properties;

namespace Novaroma.Services.Imdb {

    public class ImdbSettings : ModelBase {
        private bool _useAdvancedSearch;
        private int _advancedSearchResultCount = 42;

        [Display(Name = "UseAdvancedSearch", ResourceType = typeof(Resources))]
        public bool UseAdvancedSearch {
            get { return _useAdvancedSearch; }
            set {
                if (_useAdvancedSearch == value) return;

                _useAdvancedSearch = value;
                RaisePropertyChanged("UseAdvancedSearch");
            }
        }

        [Display(Name = "AdvancedSearchResultCount", ResourceType = typeof(Resources))]
        public int AdvancedSearchResultCount {
            get { return _advancedSearchResultCount; }
            set {
                if (_advancedSearchResultCount == value) return;

                _advancedSearchResultCount = value;
                RaisePropertyChanged("AdvancedSearchResultCount");
            }
        }
    }
}
