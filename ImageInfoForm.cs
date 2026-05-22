using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelLab
{
    public partial class ImageInfoForm : Form
    {
        public ImageInfoForm(string name, string format, int width,
                             int height, int channels, long sizeKb, int bpp, string colorSystem)
        {
            InitializeComponent();
            BuildUI(name, format, width, height, channels, sizeKb, bpp, colorSystem);
        }

        private void BuildUI(string name, string format, int width,
                             int height, int channels, long sizeKb, int bpp, string colorSystem)
        {
            this.Text = "Image Info";
            this.Size = new Size(320, 340);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lbl = new Label();
            lbl.Font = new Font("Consolas", 10);
            lbl.AutoSize = true;
            lbl.Dock = DockStyle.None;
            lbl.Padding = new Padding(20);
            lbl.Text =
                $"Name        :  {name}\n\n" +
                $"Format      :  {format}\n\n" +
                $"Width       :  {width} px\n\n" +
                $"Height      :  {height} px\n\n" +
                $"Channels    :  {channels}\n\n" +
                $"Size        :  {sizeKb:F1} KB\n\n" +
                $"Color Depth  :  {bpp} bpp\n\n" +
                $"Color System :  {colorSystem}";

            this.Controls.Add(lbl);
        }

        private void ImageInfoForm_Load(object sender, EventArgs e)
        {

        }
    }
}
