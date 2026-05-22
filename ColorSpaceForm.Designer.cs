namespace PixelLab
{
    partial class ColorSpaceForm
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
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnCMY = new System.Windows.Forms.Button();
            this.btnLAB = new System.Windows.Forms.Button();
            this.btnYUV = new System.Windows.Forms.Button();
            this.btnHSV = new System.Windows.Forms.Button();
            this.btnYCbCr = new System.Windows.Forms.Button();
            this.btnRGB = new System.Windows.Forms.Button();
            this.pnlRender = new System.Windows.Forms.Panel();
            this.pnlColorSimulate = new System.Windows.Forms.Panel();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnCMY);
            this.pnlButtons.Controls.Add(this.btnLAB);
            this.pnlButtons.Controls.Add(this.btnYUV);
            this.pnlButtons.Controls.Add(this.btnHSV);
            this.pnlButtons.Controls.Add(this.btnYCbCr);
            this.pnlButtons.Controls.Add(this.btnRGB);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlButtons.Location = new System.Drawing.Point(0, 0);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(257, 1132);
            this.pnlButtons.TabIndex = 0;
            // 
            // btnCMY
            // 
            this.btnCMY.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnCMY.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCMY.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCMY.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnCMY.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnCMY.Location = new System.Drawing.Point(43, 166);
            this.btnCMY.Name = "btnCMY";
            this.btnCMY.Size = new System.Drawing.Size(153, 60);
            this.btnCMY.TabIndex = 14;
            this.btnCMY.Text = "CMY";
            this.btnCMY.UseVisualStyleBackColor = false;
            this.btnCMY.Click += new System.EventHandler(this.btnCMY_Click);
            // 
            // btnLAB
            // 
            this.btnLAB.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnLAB.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLAB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLAB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnLAB.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnLAB.Location = new System.Drawing.Point(43, 412);
            this.btnLAB.Name = "btnLAB";
            this.btnLAB.Size = new System.Drawing.Size(153, 60);
            this.btnLAB.TabIndex = 13;
            this.btnLAB.Text = "LAB";
            this.btnLAB.UseVisualStyleBackColor = false;
            this.btnLAB.Click += new System.EventHandler(this.btnLAB_Click);
            // 
            // btnYUV
            // 
            this.btnYUV.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnYUV.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnYUV.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnYUV.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnYUV.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnYUV.Location = new System.Drawing.Point(43, 535);
            this.btnYUV.Name = "btnYUV";
            this.btnYUV.Size = new System.Drawing.Size(153, 60);
            this.btnYUV.TabIndex = 12;
            this.btnYUV.Text = "YUV";
            this.btnYUV.UseVisualStyleBackColor = false;
            this.btnYUV.Click += new System.EventHandler(this.btnYUV_Click);
            // 
            // btnHSV
            // 
            this.btnHSV.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnHSV.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHSV.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnHSV.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnHSV.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnHSV.Location = new System.Drawing.Point(43, 285);
            this.btnHSV.Name = "btnHSV";
            this.btnHSV.Size = new System.Drawing.Size(153, 60);
            this.btnHSV.TabIndex = 11;
            this.btnHSV.Text = "HSV";
            this.btnHSV.UseVisualStyleBackColor = false;
            this.btnHSV.Click += new System.EventHandler(this.btnHSV_Click);
            // 
            // btnYCbCr
            // 
            this.btnYCbCr.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnYCbCr.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnYCbCr.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnYCbCr.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnYCbCr.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnYCbCr.Location = new System.Drawing.Point(43, 658);
            this.btnYCbCr.Name = "btnYCbCr";
            this.btnYCbCr.Size = new System.Drawing.Size(153, 60);
            this.btnYCbCr.TabIndex = 10;
            this.btnYCbCr.Text = "YCbCr";
            this.btnYCbCr.UseVisualStyleBackColor = false;
            this.btnYCbCr.Click += new System.EventHandler(this.btnYCbCr_Click);
            // 
            // btnRGB
            // 
            this.btnRGB.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnRGB.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRGB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRGB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnRGB.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnRGB.Location = new System.Drawing.Point(43, 46);
            this.btnRGB.Name = "btnRGB";
            this.btnRGB.Size = new System.Drawing.Size(153, 60);
            this.btnRGB.TabIndex = 9;
            this.btnRGB.Text = "RGB";
            this.btnRGB.UseVisualStyleBackColor = false;
            this.btnRGB.Click += new System.EventHandler(this.btnRGB_Click);
            // 
            // pnlRender
            // 
            this.pnlRender.BackColor = System.Drawing.SystemColors.Control;
            this.pnlRender.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRender.Location = new System.Drawing.Point(257, 0);
            this.pnlRender.Name = "pnlRender";
            this.pnlRender.Padding = new System.Windows.Forms.Padding(0, 30, 0, 30);
            this.pnlRender.Size = new System.Drawing.Size(1937, 1132);
            this.pnlRender.TabIndex = 1;
            // 
            // pnlColorSimulate
            // 
            this.pnlColorSimulate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlColorSimulate.Location = new System.Drawing.Point(257, 873);
            this.pnlColorSimulate.Name = "pnlColorSimulate";
            this.pnlColorSimulate.Size = new System.Drawing.Size(1937, 259);
            this.pnlColorSimulate.TabIndex = 2;
            // 
            // ColorSpaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2194, 1132);
            this.Controls.Add(this.pnlColorSimulate);
            this.Controls.Add(this.pnlRender);
            this.Controls.Add(this.pnlButtons);
            this.Name = "ColorSpaceForm";
            this.Text = "ColorSpaceForm";
            this.Load += new System.EventHandler(this.ColorSpaceForm_Load);
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Panel pnlRender;
        private System.Windows.Forms.Button btnRGB;
        private System.Windows.Forms.Panel pnlColorSimulate;
        private System.Windows.Forms.Button btnHSV;
        private System.Windows.Forms.Button btnYCbCr;
        private System.Windows.Forms.Button btnCMY;
        private System.Windows.Forms.Button btnLAB;
        private System.Windows.Forms.Button btnYUV;
    }
}