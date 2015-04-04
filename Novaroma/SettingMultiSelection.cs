using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Model;

namespace Novaroma {

    public class SettingMultiSelection<TItem> : ModelBase, ILateBindable {
        private readonly IEnumerable<TItem> _items;
        private ObservableCollection<TItem> _tmpNotSelectedItems;
        private ObservableCollection<TItem> _tmpSelectedItems;

        public SettingMultiSelection(IEnumerable<TItem> items) {
            var itemList = items as IList<TItem> ?? items.ToList();
            _items = itemList;

            _tmpNotSelectedItems = new NovaromaObservableCollection<TItem>();
            _tmpSelectedItems = new NovaromaObservableCollection<TItem>(itemList);
            NotSelectedItems = _tmpNotSelectedItems;
            SelectedItems = _tmpSelectedItems;
        }

        public virtual bool RuntimeSupported {
            get { return false; }
        }

        public IEnumerable<TItem> Items {
            get { return _items; }
        }

        public ObservableCollection<TItem> TmpNotSelectedItems {
            get { return _tmpNotSelectedItems; }
        }

        public ObservableCollection<TItem> TmpSelectedItems {
            get { return _tmpSelectedItems; }
        }

        public ObservableCollection<TItem> NotSelectedItems { get; private set; }

        public ObservableCollection<TItem> SelectedItems { get; private set; }

        public IEnumerable<string> SelectedItemNames {
            get {
                return TmpSelectedItems.Select(i => i.NovaromaName());
            }
            set {
                _tmpNotSelectedItems.Clear();
                foreach (var item in _items)
                    _tmpNotSelectedItems.Add(item);

                _tmpSelectedItems.Clear();
                foreach (var item in value) {
                    var old = _tmpNotSelectedItems.FirstOrDefault(i => i.NovaromaName() == item);
                    if (!Equals(old, default(TItem))) {
                        _tmpNotSelectedItems.Remove(old);
                        _tmpSelectedItems.Add(old);
                    }
                }

                NotSelectedItems = _tmpNotSelectedItems;
                SelectedItems = _tmpSelectedItems;

                RaisePropertyChanged("SelectedItemNames");
                RaisePropertyChanged("TmpNotSelectedItems");
                RaisePropertyChanged("TmpSelectedItems");
                RaisePropertyChanged("NotSelectedItems");
                RaisePropertyChanged("SelectedItems");
            }
        }

        public void AcceptChanges() {
            NotSelectedItems = _tmpNotSelectedItems;
            SelectedItems = _tmpSelectedItems;
        }

        public void CancelChanges() {
            _tmpNotSelectedItems = NotSelectedItems;
            _tmpSelectedItems = SelectedItems;
        }
    }
}
