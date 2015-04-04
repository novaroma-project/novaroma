using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Novaroma.Model;
using Novaroma.Properties;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace Novaroma.Shell.Context {

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    [DisplayName("Novaroma Directory Context Menu")]
    [Guid("107BF368-E1F2-47C2-926A-81DE64F92E60")]
    public class DirectoryContextMenu : SharpContextMenu {

        protected override bool CanShowMenu() {
            try {
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

            if (SelectedItemPaths.Count() == 1) {
                var singleSelection = SelectedItemPaths.First();

                var mediaTask = client.GetMedia(singleSelection);
                mediaTask.Wait();
                var media = mediaTask.Result;
                var movie = media as Movie;

                if (movie != null) {
                    var updateWatchStatus = new ToolStripMenuItem {
                        Text = Resources.IsWatched,
                        Image = Resources.Img_Watch_16x16,
                        Checked = movie.IsWatched
                    };
                    updateWatchStatus.Click += (sender, args) => UpdateWatchStatus(singleSelection, !movie.IsWatched);
                    menuRoot.DropDownItems.Add(updateWatchStatus);

                    var download = new ToolStripMenuItem {
                        Text = Resources.Download,
                        Image = Resources.Img_Download_16x16
                    };
                    download.Click += (sender, args) => Download(singleSelection);
                    menuRoot.DropDownItems.Add(download);

                    if (!string.IsNullOrEmpty(movie.FilePath) && File.Exists(movie.FilePath)) {
                        var downloadSubtitle = new ToolStripMenuItem {
                            Text = Resources.DownloadSubtitle,
                            Image = Resources.Img_DownloadSubtitle_16x16
                        };
                        downloadSubtitle.Click += (sender, args) => DownloadSubtitle(singleSelection);
                        menuRoot.DropDownItems.Add(downloadSubtitle);
                    }
                }
                else {
                    if (media != null) {
                        var editMedia = new ToolStripMenuItem {
                            Text = Resources.Edit,
                            Image = Resources.Img_Edit_16x16
                        };
                        editMedia.Click += (sender, args) => EditMedia(singleSelection);
                        menuRoot.DropDownItems.Add(editMedia);
                    }

                    var dirStatusTask = client.GetDirectoryWatchStatus(singleSelection);
                    dirStatusTask.Wait();
                    var dirStatus = dirStatusTask.Result;

                    if (media == null) {
                        if (dirStatus != DirectoryWatchStatus.Direct) {
                            var addMedia = new ToolStripMenuItem {
                                Text = Resources.Add,
                                Image = Resources.Img_AddMedia_16x16
                            };
                            addMedia.Click += (sender, args) => AddMedia(SelectedItemPaths);
                            menuRoot.DropDownItems.Add(addMedia);
                        }

                        var newMedia = new ToolStripMenuItem {
                            Text = Resources.New,
                            Image = Resources.Img_NewMedia_16x16
                        };
                        newMedia.Click += (sender, args) => NewMedia(singleSelection);
                        menuRoot.DropDownItems.Add(newMedia);
                    }

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
                }
            }
            else {
                var addMedia = new ToolStripMenuItem {
                    Text = Resources.Add,
                    Image = Resources.Img_AddMedia_16x16
                };
                addMedia.Click += (sender, args) => AddMedia(SelectedItemPaths);
                menuRoot.DropDownItems.Add(addMedia);
            }

            if (menuRoot.HasDropDownItems)
                menu.Items.Add(menuRoot);
            return menu;
        }

        private static async void WatchDirectory(string directory) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.WatchDirectory(directory);
        }

        private static async void AddMedia(IEnumerable<string> directories) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.AddMedia(directories.ToArray());
        }

        private static async void NewMedia(string directory) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.NewMedia(directory);
        }

        private static async void UpdateWatchStatus(string directory, bool isWatched) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.UpdateMovieWatchStatus(directory, isWatched);
        }

        private static async void Download(string directory) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.DownloadMovie(directory);
        }

        private static async void DownloadSubtitle(string directory) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.DownloadSubtitle(directory);
        }

        private static async void EditMedia(string directory) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            await client.EditMedia(directory);
        }
    }
}
