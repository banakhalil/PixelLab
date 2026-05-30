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
using PixelLab;
using Emgu.CV;
using Emgu.CV.CvEnum; 
namespace PixelLab
{
    public partial class MainForm : Form
    {

        private readonly ImageManager _imageManager = new ImageManager();
        private string _currentImageSystem = "RGB";
        private readonly string[] _allColorSystems = { "RGB", "CMY", "CMYK", "HSV", "YCBCR", "YUV", "LAB" };
        private bool _isUpdatingCombo = false;
        private Bitmap _originalLoadedBitmap = null;
        private Bitmap _quantizedBitmap = null; // الصورة بعد تقليل الألوان

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true; 
            SetupDragDrop();
            RegisterChannelEvents();
        }
        private void ToggleControls(bool enabled)
        {
            trackBarCh1.Enabled = enabled;
            trackBarCh2.Enabled = enabled;
            trackBarCh3.Enabled = enabled;
            trackBarCh4.Enabled = enabled;
            cmbColorSpaces.Enabled = enabled;
            numKColors.Enabled = enabled;
            btnSaveImage.Enabled = enabled;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateAvailableTargets();
            cmbColorSpaces.SelectedIndex = cmbColorSpaces.Items.IndexOf("RGB");
            UpdateChannelControls("RGB");
            ResetChannelControls();
            ToggleControls(false);
        }

        // requirement 3
        private void RegisterChannelEvents()
        {
            //  ValueChanged للاستجابة اللحظية أثناء السحب
            trackBarCh1.ValueChanged += ChannelControl_Changed;
            trackBarCh2.ValueChanged += ChannelControl_Changed;
            trackBarCh3.ValueChanged += ChannelControl_Changed;
            trackBarCh4.ValueChanged += ChannelControl_Changed;

            checkBoxCh1.CheckedChanged += ChannelControl_Changed;
            checkBoxCh2.CheckedChanged += ChannelControl_Changed;
            checkBoxCh3.CheckedChanged += ChannelControl_Changed;
            checkBoxCh4.CheckedChanged += ChannelControl_Changed;
        }

        
        private void SetTrackBarRange(int max1, int max2, int max3, int max4)
        {
            trackBarCh1.Minimum = -max1; trackBarCh1.Maximum = max1;
            trackBarCh2.Minimum = -max2; trackBarCh2.Maximum = max2;
            trackBarCh3.Minimum = -max3; trackBarCh3.Maximum = max3;
            trackBarCh4.Minimum = -max4; trackBarCh4.Maximum = max4;
        }

        private void UpdateAvailableTargets()
        {
            _isUpdatingCombo = true;
            cmbColorSpaces.Items.Clear();

            // إضافة جميع الأنظمة 
            foreach (string system in _allColorSystems)
            {
                cmbColorSpaces.Items.Add(system);
            }

            _isUpdatingCombo = false;
        }


        // requirement 1 
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
            
            pictureBoxMain.Tag = null;
        }

        private void TryLoadImage(string path)
        {
            if (_imageManager.LoadImage(path))
            {
                _currentImageSystem = "RGB";

                // حفظ نسخة أصلية من الصورة المرفوعة
                _originalLoadedBitmap?.Dispose();
                _originalLoadedBitmap = new Bitmap(_imageManager.CurrentImage);

                DisplayImage();
                UpdateAvailableTargets();

                
                ResetChannelControls(); // كل شيء على 0 عند تحميل صورة

                UpdateChannelControls(_currentImageSystem);
                ApplyColorTransformation(_currentImageSystem);
                ToggleControls(true); 
            }
            else
            {
                MessageBox.Show("Unsupported file type. Please select a valid image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

        private void DisplayImage()
        {

            pictureBoxMain.Image = _imageManager.CurrentImage;
            if (lblDropHint != null) lblDropHint.Visible = false;

            numKColors.Enabled = true;
            numKColors.Value = 16; 

        }

        // requirement 8
        private void btnImageInfo_Click(object sender, EventArgs e)
        {
            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("No image loaded yet.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int width = pictureBoxMain.Image.Width;
            int height = pictureBoxMain.Image.Height;

            // القنوات وعمق البت حسب النظام اللوني الحالي
            int channels, bpp;
            string systemUpper = _currentImageSystem.ToUpper();

            if (systemUpper == "CMYK")
            {
                channels = 4; bpp = 32;
            }
            else if (systemUpper == "GRAY")
            {
                channels = 1; bpp = 8;
            }
            else
            {
                channels = 3; bpp = 24;
            }

            using (var infoForm = new ImageInfoForm(
                _imageManager.ImageName,
                _imageManager.ImageFormat,
                width,
                height,
                channels,
                _imageManager.ImageSize / 1024,
                bpp,
                _currentImageSystem))
            {
                infoForm.ShowDialog(this);
            }
        }


        // requirement 4 +5
        private void btnDisplaySpaces_Click(object sender, EventArgs e)
        {
           
            var form = new ColorSpaceForm(_imageManager.CurrentImage);
            form.Show();
        }



        private void UpdateChannelControls(string targetSystem)
        {
            bool isCmyk = targetSystem.ToUpper() == "CMYK";
            trackBarCh4.Visible = isCmyk;
            checkBoxCh4.Visible = isCmyk;

            switch (targetSystem.ToUpper())
            {
                case "RGB":
                    checkBoxCh1.Text = "R (الأحمر)";
                    checkBoxCh2.Text = "G (الأخضر)";
                    checkBoxCh3.Text = "B (الأزرق)";
                    SetTrackBarRange(255, 255, 255, 0); // 0-255 لكل مركبة
                    break;

                case "CMY":
                    checkBoxCh1.Text = "C (السيان)";
                    checkBoxCh2.Text = "M (الماجنتا)";
                    checkBoxCh3.Text = "Y (الأصفر)";
                    SetTrackBarRange(255, 255, 255, 0);
                    break;

                case "CMYK":
                    checkBoxCh1.Text = "C (السيان)";
                    checkBoxCh2.Text = "M (الماجنتا)";
                    checkBoxCh3.Text = "Y (الأصفر)";
                    checkBoxCh4.Text = "K (الأسود)";
                    SetTrackBarRange(255, 255, 255, 255); // K له نطاق أيضاً
                    break;

                case "HSV":
                    checkBoxCh1.Text = "H (التدرج)";
                    checkBoxCh2.Text = "S (التشبع)";
                    checkBoxCh3.Text = "V (القيمة/السطوع)";
                    SetTrackBarRange(360, 100, 100, 0); // H حتى 360 والباقي نسب مئوية
                    break;

                case "YUV":
                case "YCBCR":
                    checkBoxCh1.Text = "Y (الإضاءة)";
                    checkBoxCh2.Text = (targetSystem.ToUpper() == "YUV") ? "U (فرق الأزرق)" : "Cb (فرق الأزرق)";
                    checkBoxCh3.Text = (targetSystem.ToUpper() == "YUV") ? "V (فرق الأحمر)" : "Cr (فرق الأحمر)";
                    SetTrackBarRange(255, 255, 255, 0);
                    break;

                case "LAB":
                    checkBoxCh1.Text = "L (السطوع)";
                    checkBoxCh2.Text = "A (أخضر-أحمر)";
                    checkBoxCh3.Text = "B (أزرق-أصفر)";
                    SetTrackBarRange(100, 255, 255, 0); // L من 0-100، A و B من 0-255
                    break;
            }
        }

        private void ResetChannelControls()
        {
            trackBarCh1.Value = 0;
            trackBarCh2.Value = 0;
            trackBarCh3.Value = 0;
            trackBarCh4.Value = 0;

            checkBoxCh1.Checked = true;
            checkBoxCh2.Checked = true;
            checkBoxCh3.Checked = true;
            checkBoxCh4.Checked = true;
        }



        private void ChannelControl_Changed(object sender, EventArgs e)
        {
            ApplyColorTransformation(_currentImageSystem);
        }

        private void ApplyColorTransformation(string targetSystem)
        {
           // الصورة الاساسية او المقللة الوانها
            Bitmap source = _quantizedBitmap ?? _originalLoadedBitmap;

            int v1 = trackBarCh1.Value;
            int v2 = trackBarCh2.Value;
            int v3 = trackBarCh3.Value;

           
            int v4 = (targetSystem.ToUpper() == "CMYK") ? trackBarCh4.Value : 0;


            Bitmap resultBitmap = ColorConvertor.ConvertBetweenAnySpaces(
                source,
                "RGB",
                targetSystem,
                v1, v2, v3, v4,
                checkBoxCh1.Checked,
                checkBoxCh2.Checked,
                checkBoxCh3.Checked,
                (targetSystem.ToUpper() == "CMYK") ? checkBoxCh4.Checked : false
            );

            if (resultBitmap != null)
            {
                
                Image oldImage = pictureBoxMain.Image;

                pictureBoxMain.Image = resultBitmap;
                pictureBoxMain.Refresh();

                // التخلص من الصورة القديمة لمنع تسريب الذاكرة
                // بشرط ألا تكون هي الصورة الأصلية المحملة
                if (oldImage != null && oldImage != _originalLoadedBitmap)
                {
                    oldImage.Dispose();
                }
            }
        }

        private void cmbColorSpaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCombo || _originalLoadedBitmap == null) return;

            _currentImageSystem = cmbColorSpaces.SelectedItem.ToString().ToUpper();
    
            // التقاط عينة من وسط الصورة
            Color sample = _originalLoadedBitmap.GetPixel(_originalLoadedBitmap.Width / 2, _originalLoadedBitmap.Height / 2);

            ResetChannelControls();
            _quantizedBitmap?.Dispose();
            _quantizedBitmap = null;

            // تحديث الواجهة والتحويل
            UpdateChannelControls(_currentImageSystem);
            ApplyColorTransformation(_currentImageSystem);
        }



        // requirement 9
        private void btnReset_Click(object sender, EventArgs e)
        {

            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("No image loaded yet.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _imageManager.Reset();

            // إعادة تحميل الصورة الأصلية
            _originalLoadedBitmap?.Dispose();
            _originalLoadedBitmap = new Bitmap(_imageManager.CurrentImage);

            // إعادة ضبط عدد الألوان
            numKColors.Value = 16;
            _quantizedBitmap?.Dispose();
            _quantizedBitmap = null;

            DisplayImage();
            _currentImageSystem = "RGB";
            cmbColorSpaces.SelectedIndex = cmbColorSpaces.Items.IndexOf("RGB");
            ResetChannelControls();
            UpdateChannelControls("RGB");
        }
        private void label1_Click(object sender, EventArgs e) { }

 
        // requirement 10
        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("لا توجد صورة معالجة حالياً لحفظها. يرجى تحميل صورة وتعديلها أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "حفظ الصورة المعالجة";
                saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Assets");
                saveFileDialog.FileName = "Processed_Image"; 

                saveFileDialog.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg;*.jpeg)|*.jpg;*.jpeg|Bitmap Image (*.bmp)|*.bmp|TIFF Image (*.tif;*.tiff)|*.tif;*.tiff";
                saveFileDialog.DefaultExt = "png"; // 
                saveFileDialog.FilterIndex = 1; //

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Image imageToSave = pictureBoxMain.Image;

                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                        string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                        switch (extension)
                        {
                            case ".jpg":
                            case ".jpeg":
                                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                break;
                            case ".bmp":
                                format = System.Drawing.Imaging.ImageFormat.Bmp;
                                break;
                            case ".tif":
                            case ".tiff":
                                format = System.Drawing.Imaging.ImageFormat.Tiff;
                                break;
                            default:
                                format = System.Drawing.Imaging.ImageFormat.Png;
                                break;
                        }

                        
                        lock (imageToSave)
                        {
                            using (Bitmap bmpContainer = new Bitmap(imageToSave))
                            {
                                bmpContainer.Save(saveFileDialog.FileName, format);
                            }
                        }

                        MessageBox.Show("تم حفظ الصورة بنجاح بجميع تعديلاتها الحالية!", "عملية ناجحة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"عذراً، فشل حفظ الصورة على القرص بسبب: {ex.Message}", "خطأ في الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        


        // requirement 7
        private async void numKColors_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            e.SuppressKeyPress = true;

            if (_originalLoadedBitmap == null) return;

            int selectedK = (int)numKColors.Value;

            if (selectedK >= 256)
            {
                //  الصورة الأصلية مع النظام اللوني الحالي
                ApplyColorTransformation(_currentImageSystem);
                return;
            }

            try
            {
                numKColors.Enabled = false;

                Mat srcMat = ColorConvertor.BitmapToMat(_originalLoadedBitmap);

                Mat quantizedMat = await Task.Run(() =>
                    ColorConvertor.QuantizeColorsAdvanced(srcMat, selectedK, _currentImageSystem)
                );

                if (quantizedMat != null && !quantizedMat.IsEmpty)
                {
                    // quantizedMat هي BGR 
                    //نحول للنظام اللوني الحالي للعرض
                    Bitmap quantizedBmp = ColorConvertor.MatToBitmap(quantizedMat);

                    Bitmap displayBmp = ColorConvertor.ConvertBetweenAnySpaces(
                        quantizedBmp, "RGB", _currentImageSystem,
                        0, 0, 0, 0,
                        true, true, true, true
                    );

                    _quantizedBitmap?.Dispose();
                    _quantizedBitmap = new Bitmap(quantizedBmp);

                    var oldImg = pictureBoxMain.Image;
                    pictureBoxMain.Image = displayBmp;
                    pictureBoxMain.Refresh();

                    if (oldImg != _originalLoadedBitmap) oldImg?.Dispose();
                    quantizedBmp.Dispose();
                    quantizedMat.Dispose();
                }

                srcMat.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                numKColors.Enabled = true;
                numKColors.Focus();
            }
        }



    }
}