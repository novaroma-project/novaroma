using Novaroma.Interface.Model;

namespace Novaroma.Model.Search {

    public class OrderSelection: ModelBase {
        private readonly EnumInfo<OrderFields> _order; 
        private readonly bool _defaultDescending;
        private bool _isDescending;
        private bool _isSelected;

        public OrderSelection(EnumInfo<OrderFields> order, bool isDescending = false) {
            _order = order;
            _defaultDescending = isDescending;
            IsDescending = isDescending;
        }

        public EnumInfo<OrderFields> Order {
            get { return _order; }
        }

        public void Reset() {
            IsDescending = _defaultDescending;
        }

        public void RaiseResourceProperties() {
            Order.RaiseResourceProperties();
        }

        public bool IsDescending {
            get { return _isDescending; }
            set {
                if (_isDescending == value) return;

                _isDescending = value;
                RaisePropertyChanged("IsDescending");
                RaisePropertyChanged("IsAscending");
            }
        }

        public bool IsAscending {
            get { return !IsDescending; }
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;

                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
    }
}
