using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Novaroma.Properties;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace Novaroma.Shell.Context {

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)]
    [DisplayName("Novaroma File Context Menu")]
    [Guid("007E7E7D-F40D-40F6-969D-06C724CB4388")]
    public class FileContextMenu : SharpContextMenu {

        protected override bool CanShowMenu() {
            try {
                if (!SelectedItemPaths.All(Novaroma.Helper.IsVideoFile)) return false;

                var client = Novaroma.Helper.CreateShellServiceClient(TimeSpan.FromMilliseconds(50));
                client.Test();
                return SelectedItemPaths.Count() == 1;
            }
            catch {
                return false;
            }
        }

        protected override ContextMenuStrip CreateMenu() {
            var client = Novaroma.Helper.CreateShellServiceClient();
            Helper.SetCulture(client);

            var path = SelectedItemPaths.First();
            
            var menu = new ContextMenuStrip();
            var menuRoot = new ToolStripMenuItem {
                Text = Constants.Novaroma,
                Image = Resources.Img_Logo_16x16
            };

            var downloadable = client.GetDownloadable(path).Result;
            if (downloadable != null) {
                var updateWatchStatus = new ToolStripMenuItem {
                    Text = Resources.IsWatched,
                    Image = Resources.Img_Watch_16x16,
                    Checked = downloadable.IsWatched
                };
                updateWatchStatus.Click += (sender, args) => UpdateWatchStatus(path, !downloadable.IsWatched);
                menuRoot.DropDownItems.Add(updateWatchStatus);
            }

            var downloadSubtitle = new ToolStripMenuItem {
                Text = Resources.DownloadSubtitle,
                Image = Resources.Img_DownloadSubtitle_16x16
            };
            downloadSubtitle.Click += (sender, args) => DownloadSubtitle(path);
            menuRoot.DropDownItems.Add(downloadSubtitle);

            menu.Items.Add(menuRoot);
            return menu;
        }

        private static async void UpdateWatchStatus(string path, bool isWatched) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.UpdateDownloadableWatchStatus(path, isWatched);
        }

        private static async void DownloadSubtitle(string directory) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.DownloadSubtitle(directory);
        }
    }
}
