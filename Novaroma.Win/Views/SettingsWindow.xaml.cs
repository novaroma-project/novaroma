using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MahApps.Metro.Controls;
using Novaroma.Interface;
using Novaroma.Win.UserControls;
using Novaroma.Win.ViewModels;
using Binding = System.Windows.Data.Binding;

namespace Novaroma.Win.Views {

    public partial class SettingsWindow {
        private readonly SettingsViewModel _viewModel;
        private readonly List<BindingExpressionBase> _bindings = new List<BindingExpressionBase>(); 
        private readonly List<ILateBindable> _lateBindables = new List<ILateBindable>(); 

        public SettingsWindow(SettingsViewModel viewModel) {
            _viewModel = viewModel;
            InitializeComponent();

            DataContext = viewModel;

            Closing += OnClosing;

            GenerateControls();
        }

        private void GenerateControls() {
            var settings = _viewModel.Settings;
            var properties = settings.GetType().GetProperties();
            var groups = new Dictionary<string, Dictionary<TextBlock, Control>>();
            Control firstControl = null;
            foreach (var property in properties) {
                var type = property.PropertyType;
                var propertyPath = property.Name;
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                
                Control control = null;
                DependencyProperty bindProperty = null;
                if (genericType == typeof(SettingSingleSelection<>)) {
                    if (typeof(INovaromaService).IsAssignableFrom(type.GetGenericArguments().First()))
                        control = new SingleSelectionUserControl();
                    else
                        control = new SingleSelectComboBox();
                    bindProperty = DataContextProperty;
                    _lateBindables.Add((ILateBindable)property.GetValue(settings));
                }
                else if (genericType == typeof(SettingMultiSelection<>)) {
                    control = new MultiSelectionUserControl();
                    bindProperty = DataContextProperty;
                    _lateBindables.Add((ILateBindable)property.GetValue(settings));
                }
                else if (type == typeof (DirectorySelection)) {
                    control = new DirectorySelectUserControl();
                    bindProperty = DirectorySelectUserControl.TextProperty;
                    propertyPath += ".Path";
                }
                else if (!property.CanWrite) continue;
                else if (type == typeof(string)) {
                    control = new TextBox();
                    bindProperty = TextBox.TextProperty;
                }
                else if (type.IsNumericType()) {
                    control = new NumericUpDown();
                    bindProperty = NumericUpDown.ValueProperty;
                }
                else if (type == typeof (bool) 
                        || (genericType != null && genericType == typeof(Nullable<>) && genericType.GenericTypeArguments[0] == typeof(bool))) {
                    control = new ToggleSwitch {Language = Language};
                    bindProperty = ToggleSwitch.IsCheckedProperty;
                }

                if (control != null && bindProperty != null) {
                    if (firstControl == null) firstControl = control;

                    var displayAttr = property.GetAttribute<DisplayAttribute>();
                    string displayValue;
                    string description;
                    string groupName;
                    if (displayAttr != null) {
                        displayValue = displayAttr.GetName() ?? property.Name;
                        description = displayAttr.GetDescription() ?? string.Empty;
                        groupName = displayAttr.GetGroupName() ?? string.Empty;
                    }
                    else {
                        displayValue = property.Name;
                        description = string.Empty;
                        groupName = string.Empty;
                    }

                    var textBlock = new TextBlock();
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.Margin = new Thickness(10);
                    textBlock.Text = displayValue;
                    if (!string.IsNullOrEmpty(description))
                        textBlock.ToolTip = description;
                    textBlock.SetCurrentValue(Grid.ColumnProperty, 0);

                    control.Margin = new Thickness(10);
                    control.SetCurrentValue(Grid.ColumnProperty, 1);

                    var binding = new Binding();
                    binding.Source = settings;
                    binding.Path = new PropertyPath(propertyPath);
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                    var bindingExpression = BindingOperations.SetBinding(control, bindProperty, binding);
                    _bindings.Add(bindingExpression);

                    if (groups.ContainsKey(groupName))
                        groups[groupName].Add(textBlock, control);
                    else
                        groups.Add(groupName, new Dictionary<TextBlock, Control> {{textBlock, control}});
                }
            }

            foreach (var group in groups) {
                var tabItem = new TabItem();
                if (groups.Count > 1)
                    tabItem.Header = string.IsNullOrEmpty(group.Key) ? Novaroma.Properties.Resources.Main : group.Key;
                ControlsTabControl.Items.Add(tabItem);

                var scrollViewer = new ScrollViewer();
                tabItem.Content = scrollViewer;

                var grid = new Grid();
                grid.HorizontalAlignment = HorizontalAlignment.Stretch;
                grid.VerticalAlignment = VerticalAlignment.Stretch;
                grid.Margin = new Thickness(20);
                grid.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
                grid.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
                grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(0, GridUnitType.Auto)});
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                scrollViewer.Content = grid;

                var i = 0;
                foreach (var controls in group.Value) {
                    var rowDefinition = new RowDefinition();
                    rowDefinition.Height = new GridLength(0, GridUnitType.Auto);
                    grid.RowDefinitions.Add(rowDefinition);

                    var textBlock = controls.Key;
                    textBlock.SetCurrentValue(Grid.RowProperty, i);

                    var control = controls.Value;
                    control.SetCurrentValue(Grid.RowProperty, i);

                    grid.Children.Add(controls.Key);
                    grid.Children.Add(controls.Value);

                    i++;
                }
            }

            if (firstControl != null)
                firstControl.Focus();
        }

        private bool _shouldBeClosed;
        private async void SaveClick(object sender, RoutedEventArgs e) {
            _lateBindables.ForEach(lb => lb.AcceptChanges());
            _bindings.ForEach(b => b.UpdateSource());
            await _viewModel.Save();
            _shouldBeClosed = true;

            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs) {
            if (!_shouldBeClosed) {
                _lateBindables.ForEach(lb => lb.CancelChanges());
                _viewModel.Cancel();
            }
        }
    }
}
