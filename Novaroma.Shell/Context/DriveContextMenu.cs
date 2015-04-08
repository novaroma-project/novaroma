using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Novaroma.Properties;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace Novaroma.Shell.Context {

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Drive)]
    [DisplayName("Novaroma Drive Context Menu")]
    [Guid("0A97AEFC-C494-4288-8316-9CE7EE52B027")]
    public class DriveContextMenu : SharpContextMenu {

        protected override bool CanShowMenu() {
            try {
                if (SelectedItemPaths.Count() != 1) return false;

                var client = Novaroma.Helper.CreateShellServiceClient(TimeSpan.FromMilliseconds(50));
                client.Test();
                return true;
            }
            catch {
                return false;
            }
        }

        protected override ContextMenuStrip CreateMenu() {
            var menu = new ContextMenuStrip();

            var menuRoot = new ToolStripMenuItem {
                Text = Constants.Novaroma,
                Image = Resources.Img_Logo_16x16
            };

            var client = Novaroma.Helper.CreateShellServiceClient();
            Helper.SetCulture(client);

            var singleSelection = SelectedItemPaths.Single();

            var newMedia = new ToolStripMenuItem {
                Text = Resources.New,
                Image = Resources.Img_NewMedia_16x16
            };
            newMedia.Click += (sender, args) => NewMedia(singleSelection);
            menuRoot.DropDownItems.Add(newMedia);

            var dirStatus = client.GetDirectoryWatchStatus(singleSelection).Result;
            if (dirStatus == DirectoryWatchStatus.None) {
                if (Directory.GetDirectories(singleSelection).Any()) {
                    var watchDirectory = new ToolStripMenuItem {
                        Text = Resources.AddSubdirectories,
                        Image = Resources.Img_AddSubdirectories_16x16
                    };
                    watchDirectory.Click += (sender, args) => WatchDirectory(singleSelection);
                    menuRoot.DropDownItems.Add(watchDirectory);
                }
            }

            menu.Items.Add(menuRoot);
            return menu;
        }

        private static async void WatchDirectory(string directory) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.WatchDirectory(directory);
        }

        private static async void NewMedia(string directory) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.NewMedia(directory);
        }
    }
}
