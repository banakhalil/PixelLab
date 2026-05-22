namespace PixelLab
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pictureBoxMain = new System.Windows.Forms.PictureBox();
            this.btnOpenImage = new System.Windows.Forms.Button();
            this.lblDropHint = new System.Windows.Forms.Label();
            this.cmbColorSpaces = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnImageInfo = new System.Windows.Forms.Button();
            this.btnDisplaySpaces = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxMain
            // 
            this.pictureBoxMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.pictureBoxMain, "pictureBoxMain");
            this.pictureBoxMain.Name = "pictureBoxMain";
            this.pictureBoxMain.TabStop = false;
            this.pictureBoxMain.Click += new System.EventHandler(this.pictureBoxMain_Click);
            // 
            // btnOpenImage
            // 
            this.btnOpenImage.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnOpenImage.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnOpenImage, "btnOpenImage");
            this.btnOpenImage.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnOpenImage.Name = "btnOpenImage";
            this.btnOpenImage.UseVisualStyleBackColor = false;
            this.btnOpenImage.Click += new System.EventHandler(this.btnOpenImage_Click);
            // 
            // lblDropHint
            // 
            resources.ApplyResources(this.lblDropHint, "lblDropHint");
            this.lblDropHint.Name = "lblDropHint";
            // 
            // cmbColorSpaces
            // 
            resources.ApplyResources(this.cmbColorSpaces, "cmbColorSpaces");
            this.cmbColorSpaces.FormattingEnabled = true;
            this.cmbColorSpaces.Name = "cmbColorSpaces";
            this.cmbColorSpaces.SelectedIndexChanged += new System.EventHandler(this.cmbColorSpaces_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // btnImageInfo
            // 
            this.btnImageInfo.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnImageInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnImageInfo, "btnImageInfo");
            this.btnImageInfo.Name = "btnImageInfo";
            this.btnImageInfo.UseVisualStyleBackColor = false;
            this.btnImageInfo.Click += new System.EventHandler(this.btnImageInfo_Click);
            // 
            // btnDisplaySpaces
            // 
            this.btnDisplaySpaces.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnDisplaySpaces.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnDisplaySpaces, "btnDisplaySpaces");
            this.btnDisplaySpaces.Name = "btnDisplaySpaces";
            this.btnDisplaySpaces.UseVisualStyleBackColor = false;
            this.btnDisplaySpaces.Click += new System.EventHandler(this.btnDisplaySpaces_Click);
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnReset, "btnReset");
            this.btnReset.Name = "btnReset";
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnDisplaySpaces);
            this.Controls.Add(this.btnImageInfo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbColorSpaces);
            this.Controls.Add(this.lblDropHint);
            this.Controls.Add(this.btnOpenImage);
            this.Controls.Add(this.pictureBoxMain);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxMain;
        private System.Windows.Forms.Button btnOpenImage;
        private System.Windows.Forms.Label lblDropHint;
        private System.Windows.Forms.ComboBox cmbColorSpaces;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnImageInfo;
        private System.Windows.Forms.Button btnDisplaySpaces;
        private System.Windows.Forms.Button btnReset;
    }
}

