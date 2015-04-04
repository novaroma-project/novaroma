using System.Drawing;
using System.IO;
using System.Linq;
using Novaroma.Model;
using Novaroma.Properties;
using SharpShell.SharpPreviewHandler;

namespace Novaroma.Shell.Preview {

    public partial class MediaPreviewUserControl : PreviewHandlerControl {

        public MediaPreviewUserControl(Media media) {
            InitializeComponent();

            lblTitle.Text = media.Title;
            lblYear.Text = "(" + media.Year + ")";
            if (media.Poster != null) {
                using (var ms = new MemoryStream(media.Poster))
                    pbPoster.Image = Image.FromStream(ms);
            }
            lblGenres.Text = string.Join(" | ", media.Genres.Select(g => g.Name));
            lblRating.Text = media.Rating + "/10";
            lblVoteCount.Text = (media.VoteCount.HasValue ? media.VoteCount : 0) + " " + Resources.Votes;
            lblOutline.Text = media.Outline;
            lblCredits.Text = media.Credits;
        }
    }
}
