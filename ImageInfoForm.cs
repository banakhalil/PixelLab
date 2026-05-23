using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelLab
{
    public partial class ImageInfoForm : Form
    {
        public ImageInfoForm(string name, string format, int width, int height, int channels, long size, int bpp, string colorSystem)
        {
            InitializeComponent();

            
            this.Text = "Image Info";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(340, 320);

            
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(20),
                WrapContents = false
            };

            void AddInfoRow(string title, string value)
            {
                Label lbl = new Label
                {
                    Text = $"{title}:  {value}",
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 12),
                    ForeColor = Color.Black
                };
                panel.Controls.Add(lbl);
            }

            AddInfoRow("Name", name);
            AddInfoRow("Format", format);
            AddInfoRow("width x height", $"{width} × {height} px");
            AddInfoRow("Size", $"{size} KB");
            AddInfoRow("Color System", colorSystem.ToUpper());
            AddInfoRow("Channels", $"{channels} ");
            AddInfoRow("Color depth", $"{bpp} bpp");
            


            this.Controls.Add(panel);
        }

        private void ImageInfoForm_Load(object sender, EventArgs e)
        {

        }
    }

    //public partial class ImageInfoForm : Form
    //{
    //    public ImageInfoForm(string name, string format, int width,
    //                         int height, int channels, long sizeKb, int bpp, string colorSystem)
    //    {
    //        InitializeComponent();
    //        BuildUI(name, format, width, height, channels, sizeKb, bpp, colorSystem);
    //    }

    //    private void BuildUI(string name, string format, int width,
    //                         int height, int channels, long sizeKb, int bpp, string colorSystem)
    //    {
    //        this.Text = "Image Info";
    //        this.Size = new Size(340, 340);
    //        this.StartPosition = FormStartPosition.CenterParent;
    //        this.FormBorderStyle = FormBorderStyle.FixedDialog;
    //        this.MaximizeBox = false;
    //        this.MinimizeBox = false;

    //        var lbl = new Label();
    //        lbl.Font = new Font("Consolas", 10);
    //        lbl.AutoSize = true;
    //        lbl.Dock = DockStyle.None;
    //        lbl.Padding = new Padding(20);
    //        lbl.Text =
    //            $"Name        :  {name}\n\n" +
    //            $"Format      :  {format}\n\n" +
    //            $"Width       :  {width} px\n\n" +
    //            $"Height      :  {height} px\n\n" +
    //            $"Channels    :  {channels}\n\n" +
    //            $"Size        :  {sizeKb:F1} KB\n\n" +
    //            $"Color Depth  :  {bpp} bpp\n\n" +
    //            $"Color System :  {colorSystem}";

    //        this.Controls.Add(lbl);
    //    }

    //    private void ImageInfoForm_Load(object sender, EventArgs e)
    //    {

    //    }
    //}

}