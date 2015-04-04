using System.Collections.Generic;
using System.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Model;

namespace Novaroma {

    public class SettingSingleSelection<TItem> : ModelBase, ILateBindable {
        private readonly IEnumerable<TItem> _items;
        private TItem _tmpSelectedItem;
        private TItem _selectedItem;

        public SettingSingleSelection(IEnumerable<TItem> items) {
            var itemList = items as IList<TItem> ?? items.ToList();
            _items = itemList;

            if (itemList.Any()) {
                _tmpSelectedItem = itemList.First();
                _selectedItem = _tmpSelectedItem;
            }
        }

        public virtual bool RuntimeSupported  {
            get { return false; }
        }

        public IEnumerable<TItem> Items {
            get {
                return _items;
            }
        }

        public TItem TmpSelectedItem {
            get { return _tmpSelectedItem; }
            set {
                if (Equals(_tmpSelectedItem, value)) return;

                _tmpSelectedItem = value;
                RaisePropertyChanged("TmpSelectedItem");
            }
        }

        public TItem SelectedItem {
            get { return _selectedItem; }
            set {
                if (Equals(_selectedItem, value)) return;

                _tmpSelectedItem = value;
                _selectedItem = value;
                RaisePropertyChanged("TmpSelectedItem");
                RaisePropertyChanged("SelectedItem");
            }
        }

        public string SelectedItemName {
            get {
                return SelectedItem.NovaromaName();
            }
            set {
                var item = _items.FirstOrDefault(i => i.NovaromaName() == value);

                if (Equals(item, default(TItem)))
                    item = Items.FirstOrDefault();

                _tmpSelectedItem = item;
                _selectedItem = item;

                RaisePropertyChanged("SelectedItemName");
                RaisePropertyChanged("TmpSelectedItem");
                RaisePropertyChanged("SelectedItem");
            }
        }

        public void AcceptChanges() {
            SelectedItem = TmpSelectedItem;
        }

        public void CancelChanges() {
            TmpSelectedItem = SelectedItem;
        }
    }
}
