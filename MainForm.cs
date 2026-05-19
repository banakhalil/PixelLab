using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelLab.Core;

namespace PixelLab
{
    public partial class MainForm : Form
    {
        private readonly ImageManager _imageManager = new ImageManager();

        public MainForm()
        {
            InitializeComponent();
            SetupDragDrop();   
        }

        // زر فتح الصورة 
        private void btnOpenImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select an Image";
                dialog.Filter =
                    "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff" +
                    "|JPEG|*.jpg;*.jpeg" +
                    "|PNG|*.png" +
                    "|BMP|*.bmp" +
                    "|All Files|*.*";

                // فتح مجلد الصور فورا 
                dialog.InitialDirectory = Path.Combine(
            Application.StartupPath, "Assets");

                if (dialog.ShowDialog() == DialogResult.OK)
                    TryLoadImage(dialog.FileName);
            }
        }

        // تحميل الصورة ومعالجة الأخطاء 
        private void TryLoadImage(string path)
        {
            if (_imageManager.LoadImage(path))
            {
                DisplayImage();
            }
            else
            {
                MessageBox.Show(
                    "Unsupported file type. Please select a valid image.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // PictureBox  عرض الصورة في  
        private void DisplayImage()
        {
            pictureBoxMain.Image = _imageManager.CurrentImage;
            lblDropHint.Visible = false;  // اخفاء الليبل بعد التحميل
        }



        private void SetupDragDrop()
        {
            pictureBoxMain.AllowDrop = true;

            pictureBoxMain.DragEnter += (sender, e) =>
            {
                //  المسحوب هو ملف 
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;  
                else
                    e.Effect = DragDropEffects.None;  
            };

            pictureBoxMain.DragDrop += (sender, e) =>
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                    TryLoadImage(files[0]);  // نأخذ الملف الأول فقط في حال تحميل عدة صور 
            };
        }
    }
    

}
