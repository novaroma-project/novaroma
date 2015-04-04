using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Novaroma.Interface.Model;

namespace Novaroma {

    public class MultiCheckSelection<TItem> : ModelBase, IMultiCheckSelection<TItem> {
        private readonly ObservableCollection<TItem> _items;
        private IEnumerable<ItemSelection<TItem>> _selections;

        public MultiCheckSelection(IEnumerable<TItem> items): this(new NovaromaObservableCollection<TItem>(items)) {
        }

        public MultiCheckSelection(ObservableCollection<TItem> items) {
            _items = items;

            CreateSelections();
            _items.CollectionChanged += ItemsOnCollectionChanged;
        }

        private void CreateSelections() {
            var selections = _items.Select(i => new ItemSelection<TItem>(i)).ToList();

            foreach (var newSelection in selections) {
                if (Selections != null) {
                    foreach (var oldSelection in Selections) {
                        if (oldSelection.IsSelected) {
                            if (Equals(newSelection.Item, oldSelection.Item))
                                newSelection.IsSelected = true;
                        }

                        oldSelection.PropertyChanged -= ItemSelectionPropertyChanged;
                    }
                }

                newSelection.PropertyChanged += ItemSelectionPropertyChanged;
            }

            Selections = selections;
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            CreateSelections();
        }

        private void ItemSelectionPropertyChanged(object sender, PropertyChangedEventArgs e) {
            RaisePropertyChanged("SelectedItems");
            RaisePropertyChanged("SelectedNames");
        }

        public IEnumerable<ItemSelection<TItem>> Selections {
            get { return _selections; }
            set {
                if (Equals(_selections, value)) return;

                _selections = value;
                RaisePropertyChanged("Selections");
            }
        }

        public IEnumerable<TItem> SelectedItems {
            get {
                return Selections.Where(s => s.IsSelected).Select(s => s.Item).ToList();
            }
        }

        #region IMultiCheckSelection<TItem> Members

        IEnumerable<IItemSelection<TItem>> IMultiCheckSelection<TItem>.Selections {
            get { return Selections; }
        }

        #endregion
    }

    public interface IMultiCheckSelection<out TItem> {
        IEnumerable<IItemSelection<TItem>> Selections { get; }
        IEnumerable<TItem> SelectedItems { get; }
    }

    public interface IItemSelection<out TItem>: INotifyPropertyChanged {
        TItem Item { get; }
        bool IsSelected { get; set; }
    }

    public class ItemSelection<TItem> : ModelBase, IItemSelection<TItem> {
        private readonly TItem _item;
        private bool _isSelected;

        public ItemSelection(TItem item, bool isSelected = false) {
            _item = item;
            IsSelected = isSelected;
        }

        public TItem Item {
            get { return _item; }
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
