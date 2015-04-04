namespace Novaroma.Shell.Preview {
    partial class MediaPreviewUserControl {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lblTitle = new System.Windows.Forms.Label();
            this.flPanelTop = new System.Windows.Forms.FlowLayoutPanel();
            this.lblYear = new System.Windows.Forms.Label();
            this.flPanelFill = new System.Windows.Forms.FlowLayoutPanel();
            this.pbPoster = new System.Windows.Forms.PictureBox();
            this.lblGenres = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRating = new System.Windows.Forms.Label();
            this.lblVoteCount = new System.Windows.Forms.Label();
            this.lblCredits = new System.Windows.Forms.Label();
            this.lblOutline = new System.Windows.Forms.Label();
            this.flPanelTop.SuspendLayout();
            this.flPanelFill.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPoster)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(66, 31);
            this.lblTitle.TabIndex = 2;
            this.lblTitle.Text = "Title";
            // 
            // flPanelTop
            // 
            this.flPanelTop.AutoSize = true;
            this.flPanelTop.Controls.Add(this.lblTitle);
            this.flPanelTop.Controls.Add(this.lblYear);
            this.flPanelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.flPanelTop.Location = new System.Drawing.Point(0, 0);
            this.flPanelTop.Margin = new System.Windows.Forms.Padding(0);
            this.flPanelTop.Name = "flPanelTop";
            this.flPanelTop.Size = new System.Drawing.Size(394, 31);
            this.flPanelTop.TabIndex = 3;
            // 
            // lblYear
            // 
            this.lblYear.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblYear.AutoSize = true;
            this.lblYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblYear.ForeColor = System.Drawing.Color.White;
            this.lblYear.Location = new System.Drawing.Point(75, 5);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(43, 20);
            this.lblYear.TabIndex = 3;
            this.lblYear.Text = "Year";
            // 
            // flPanelFill
            // 
            this.flPanelFill.AutoScroll = true;
            this.flPanelFill.Controls.Add(this.pbPoster);
            this.flPanelFill.Controls.Add(this.lblGenres);
            this.flPanelFill.Controls.Add(this.flowLayoutPanel1);
            this.flPanelFill.Controls.Add(this.lblCredits);
            this.flPanelFill.Controls.Add(this.lblOutline);
            this.flPanelFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flPanelFill.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flPanelFill.Location = new System.Drawing.Point(0, 31);
            this.flPanelFill.Name = "flPanelFill";
            this.flPanelFill.Padding = new System.Windows.Forms.Padding(5);
            this.flPanelFill.Size = new System.Drawing.Size(394, 477);
            this.flPanelFill.TabIndex = 5;
            this.flPanelFill.WrapContents = false;
            // 
            // pbPoster
            // 
            this.pbPoster.Location = new System.Drawing.Point(8, 5);
            this.pbPoster.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.pbPoster.Name = "pbPoster";
            this.pbPoster.Size = new System.Drawing.Size(220, 208);
            this.pbPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbPoster.TabIndex = 5;
            this.pbPoster.TabStop = false;
            // 
            // lblGenres
            // 
            this.lblGenres.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGenres.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblGenres.ForeColor = System.Drawing.Color.White;
            this.lblGenres.Location = new System.Drawing.Point(8, 218);
            this.lblGenres.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.lblGenres.Name = "lblGenres";
            this.lblGenres.Size = new System.Drawing.Size(220, 17);
            this.lblGenres.TabIndex = 9;
            this.lblGenres.Text = "Genres";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.lblRating);
            this.flowLayoutPanel1.Controls.Add(this.lblVoteCount);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(8, 243);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(220, 15);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // lblRating
            // 
            this.lblRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRating.AutoSize = true;
            this.lblRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblRating.ForeColor = System.Drawing.Color.White;
            this.lblRating.Location = new System.Drawing.Point(3, 0);
            this.lblRating.Margin = new System.Windows.Forms.Padding(3, 0, 10, 0);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(43, 15);
            this.lblRating.TabIndex = 10;
            this.lblRating.Text = "Rating";
            // 
            // lblVoteCount
            // 
            this.lblVoteCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVoteCount.AutoSize = true;
            this.lblVoteCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblVoteCount.ForeColor = System.Drawing.Color.White;
            this.lblVoteCount.Location = new System.Drawing.Point(59, 0);
            this.lblVoteCount.Name = "lblVoteCount";
            this.lblVoteCount.Size = new System.Drawing.Size(63, 15);
            this.lblVoteCount.TabIndex = 11;
            this.lblVoteCount.Text = "VoteCount";
            // 
            // lblCredits
            // 
            this.lblCredits.AutoSize = true;
            this.lblCredits.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblCredits.ForeColor = System.Drawing.Color.White;
            this.lblCredits.Location = new System.Drawing.Point(8, 263);
            this.lblCredits.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.lblCredits.Name = "lblCredits";
            this.lblCredits.Size = new System.Drawing.Size(45, 15);
            this.lblCredits.TabIndex = 10;
            this.lblCredits.Text = "Credits";
            // 
            // lblOutline
            // 
            this.lblOutline.AutoSize = true;
            this.lblOutline.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblOutline.ForeColor = System.Drawing.Color.White;
            this.lblOutline.Location = new System.Drawing.Point(8, 283);
            this.lblOutline.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.lblOutline.Name = "lblOutline";
            this.lblOutline.Size = new System.Drawing.Size(40, 13);
            this.lblOutline.TabIndex = 11;
            this.lblOutline.Text = "Outline";
            // 
            // MediaPreviewUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.Controls.Add(this.flPanelFill);
            this.Controls.Add(this.flPanelTop);
            this.Name = "MediaPreviewUserControl";
            this.Size = new System.Drawing.Size(394, 508);
            this.flPanelTop.ResumeLayout(false);
            this.flPanelTop.PerformLayout();
            this.flPanelFill.ResumeLayout(false);
            this.flPanelFill.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPoster)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel flPanelTop;
        private System.Windows.Forms.FlowLayoutPanel flPanelFill;
        private System.Windows.Forms.PictureBox pbPoster;
        private System.Windows.Forms.Label lblYear;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblRating;
        private System.Windows.Forms.Label lblVoteCount;
        private System.Windows.Forms.Label lblGenres;
        private System.Windows.Forms.Label lblCredits;
        private System.Windows.Forms.Label lblOutline;
    }
}
