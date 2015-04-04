using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using Novaroma.Interface;

namespace Novaroma.Win.Utilities {

    public class DataGridColumnGenerationBehavior : Behavior<DataGrid> {

        protected override void OnAttached() {
            AssociatedObject.AutoGeneratingColumn += OnAutoGeneratingColumn;
        }

        protected override void OnDetaching() {
            AssociatedObject.AutoGeneratingColumn -= OnAutoGeneratingColumn;
        }

        protected void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) {
            var displayAttribute = GetPropertyDisplayName(e.PropertyDescriptor);

            if (displayAttribute != null) {
                if (typeof(INovaromaService).IsAssignableFrom(e.PropertyType)) {
                    var column = new DataGridTextColumn();
                    column.Binding = new Binding(e.PropertyName + ".ServiceName");
                    e.Column = column;
                }

                e.Column.Header = displayAttribute.GetName();
            }
            else
                e.Cancel = true;
        }

        protected static DisplayAttribute GetPropertyDisplayName(object descriptor) {
            var pd = descriptor as PropertyDescriptor;
            if (pd != null)
                return pd.Attributes[typeof(DisplayAttribute)] as DisplayAttribute;

            var pi = descriptor as PropertyInfo;
            if (pi != null) {
                var attrs = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                foreach (var att in attrs) {
                    var attribute = att as DisplayAttribute;
                    if (attribute != null)
                        return attribute;
                }
            }

            return null;
        }
    }
}
