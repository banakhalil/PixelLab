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

        private string _currentImageSystem = "RGB";

        private readonly string[] _allColorSystems = { "RGB", "CMY", "HSV", "YCBCR", "YUV", "LAB" };

        private bool _isUpdatingCombo = false;

        public MainForm()
        {
            InitializeComponent();
            SetupDragDrop();
        }

        private void btnOpenImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select an Image";
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff";

                dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Assets");

                if (dialog.ShowDialog() == DialogResult.OK)
                    TryLoadImage(dialog.FileName);
            }
        }

        private void TryLoadImage(string path)
        {
            if (_imageManager.LoadImage(path))
            {
                _currentImageSystem = "RGB";
                DisplayImage();
                UpdateAvailableTargets(); 
            }
            else
            {
                MessageBox.Show("Unsupported file type. Please select a valid image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayImage()
        {
            pictureBoxMain.Image = _imageManager.CurrentImage;
            if (lblDropHint != null) lblDropHint.Visible = false;
        }

        private void UpdateAvailableTargets()
        {
            _isUpdatingCombo = true;

            cmbColorSpaces.Items.Clear();

            foreach (string system in _allColorSystems)
            {
                if (!system.Equals(_currentImageSystem, StringComparison.OrdinalIgnoreCase))
                {
                    cmbColorSpaces.Items.Add(system);
                }
            }

            if (cmbColorSpaces.Items.Count > 0)
                cmbColorSpaces.SelectedIndex = -1; 

            _isUpdatingCombo = false;
        }

        private void cmbColorSpaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCombo || cmbColorSpaces.SelectedItem == null)
                return;

            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("الرجاء تحميل صورة أولاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string targetSystem = cmbColorSpaces.SelectedItem.ToString();

            try
            {
                Bitmap activeBitmap = (Bitmap)pictureBoxMain.Image;

                // استدعاء التحويل الفوري السريع
                Bitmap resultBitmap = ColorConvertor.ConvertBetweenAnySpaces(activeBitmap, _currentImageSystem, targetSystem);

                if (resultBitmap != null)
                {
                    pictureBoxMain.Image = resultBitmap;
                    pictureBoxMain.Refresh();

                    _currentImageSystem = targetSystem;

                    // تحديث الخيارات المتاحة بناءً على النظام الجديد المستقر للصورة
                    UpdateAvailableTargets();

                    this.Text = $"PixelLab - Current Space: [{_currentImageSystem}]";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء التحويل: {ex.Message}", "خطأ في المعالجة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateAvailableTargets();
        }

        private void SetupDragDrop()
        {
            pictureBoxMain.AllowDrop = true;
            pictureBoxMain.DragEnter += (sender, e) => {
                e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            };
            pictureBoxMain.DragDrop += (sender, e) => {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0) TryLoadImage(files[0]);
            };
        }

        private void pictureBoxMain_Click(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}