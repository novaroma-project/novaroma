using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Novaroma.Win.UserControls {

    public partial class MultiSelectComboBox {

        public MultiSelectComboBox() {
            InitializeComponent();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable<IItemSelection<object>>), typeof(MultiSelectComboBox),
            new FrameworkPropertyMetadata(null, OnItemsSourceChanged)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(MultiSelectComboBox),
            new UIPropertyMetadata(string.Empty)
        );

        public static readonly DependencyProperty DefaultTextProperty = DependencyProperty.Register(
            "DefaultText",
            typeof(string),
            typeof(MultiSelectComboBox),
            new UIPropertyMetadata(string.Empty)
        );

        public IEnumerable<IItemSelection<object>> ItemsSource {
            get {
                return (IEnumerable<IItemSelection<object>>)GetValue(ItemsSourceProperty);
            }
            set {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public string Text {
            get {
                return (string)GetValue(TextProperty);
            }
            set {
                SetValue(TextProperty, value);
            }
        }

        public string DefaultText {
            get {
                return (string)GetValue(DefaultTextProperty);
            }
            set {
                SetValue(DefaultTextProperty, value);
            }
        }

        #endregion

        #region Events

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = (MultiSelectComboBox)d;

            var oldSelections = e.OldValue as IEnumerable<IItemSelection<object>>;
            if (oldSelections != null)
                foreach (var oldSelection in oldSelections)
                    oldSelection.PropertyChanged -= control.SelectionOnPropertyChanged;

            foreach (var newSelection in control.ItemsSource)
                newSelection.PropertyChanged += control.SelectionOnPropertyChanged;

            control.MultiSelectCombo.ItemsSource = control.ItemsSource;
            control.SetText();
        }

        private void SelectionOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            SetText();
        }

        #endregion

        #region Methods

        private void SetText() {
            var selectedItems = ItemsSource.Where(s => s.IsSelected);
            var displayText = string.Join(", ", selectedItems.Select(s => s.Item.NovaromaName()));

            if (string.IsNullOrEmpty(displayText))
                Text = DefaultText;

            Text = displayText;
            MultiSelectCombo.ToolTip = displayText;
        }

        #endregion
    }
}
