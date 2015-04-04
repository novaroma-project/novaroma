using System.Windows;
using System.Windows.Media;

namespace Novaroma.Win {

    public static class Extensions {

        public static void ForceShow(this Window window) {
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            if (!window.IsVisible)
                window.Show();

            window.BringIntoView();
            window.Activate();
        }

        public static TType FindAncestor<TType>(this DependencyObject dependencyObject) where TType : DependencyObject {
            var parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent == null) return null;

            var parentT = parent as TType;
            return parentT ?? FindAncestor<TType>(parent);
        }
    }
}
