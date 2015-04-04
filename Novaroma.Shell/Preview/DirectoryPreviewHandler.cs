using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using Novaroma.Model;
using SharpShell.Attributes;
using SharpShell.SharpPreviewHandler;

namespace Novaroma.Shell.Preview {

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory, "Directory")]
    [DisplayName("Novaroma Directory Preview Handler")]
    [PreviewHandler]
    [Guid("55D68975-B4DA-4D91-9395-9DEA8706E2A8")]
    public class DirectoryPreviewHandler : SharpPreviewHandler {

        protected override PreviewHandlerControl DoPreview() {
            try {
                var infoPath = Path.Combine(SelectedFilePath, "novaroma.info");
                if (!File.Exists(infoPath)) return null;

                var serializedMedia = File.ReadAllText(infoPath);
                var media = JsonConvert.DeserializeObject<Media>(serializedMedia);

                return new MediaPreviewUserControl(media);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);

                return null;
            }
        }
    }
}
