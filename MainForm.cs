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
using Emgu.CV.CvEnum; // إذا احتجتها لاحقاً في الفلاتر

namespace PixelLab
{
    public partial class MainForm : Form
    {

        // الاحتفاظ بالصورة الأصلية
        //private Bitmap _originalLoadedBitmap = null;

        private readonly ImageManager _imageManager = new ImageManager();
        private string _currentImageSystem = "RGB";
        private readonly string[] _allColorSystems = { "RGB", "CMY", "CMYK", "HSV", "YCBCR", "YUV", "LAB" };
        private bool _isUpdatingCombo = false;
        private Bitmap _originalLoadedBitmap = null;
        private Bitmap _quantizedBitmap = null; // الصورة بعد تقليل الألوان

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // لتقليل الومض عند التحديث
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

        private void RegisterChannelEvents()
        {
            // ربط ValueChanged للاستجابة اللحظية أثناء السحب
            trackBarCh1.ValueChanged += ChannelControl_Changed;
            trackBarCh2.ValueChanged += ChannelControl_Changed;
            trackBarCh3.ValueChanged += ChannelControl_Changed;
            trackBarCh4.ValueChanged += ChannelControl_Changed;

            checkBoxCh1.CheckedChanged += ChannelControl_Changed;
            checkBoxCh2.CheckedChanged += ChannelControl_Changed;
            checkBoxCh3.CheckedChanged += ChannelControl_Changed;
            checkBoxCh4.CheckedChanged += ChannelControl_Changed;
        }

        // قم بتغيير اسم الدالة هنا لتطابق الاستدعاءات في الكود
        private void SetTrackBarRange(int max1, int max2, int max3, int max4)
        {
            // قمت بتعديلها لتصبح أكثر مرونة بناءً على استخدامك
            //trackBarCh1.Minimum = 0; trackBarCh1.Maximum = max1;
            //trackBarCh2.Minimum = 0; trackBarCh2.Maximum = max2;
            //trackBarCh3.Minimum = 0; trackBarCh3.Maximum = max3;
            //trackBarCh4.Minimum = 0; trackBarCh4.Maximum = max4;

            trackBarCh1.Minimum = -max1; trackBarCh1.Maximum = max1;
            trackBarCh2.Minimum = -max2; trackBarCh2.Maximum = max2;
            trackBarCh3.Minimum = -max3; trackBarCh3.Maximum = max3;
            trackBarCh4.Minimum = -max4; trackBarCh4.Maximum = max4;
        }

        private void UpdateAvailableTargets()
        {
            _isUpdatingCombo = true;
            cmbColorSpaces.Items.Clear();

            // إضافة جميع الأنظمة بدون استثناء
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
            // تصفير النسخة الاحتياطية القديمة في الـ Tag عند فتح صورة جديدة تماماً
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

                // بدلاً من ResetChannelControls التي تصفر الأشرطة،
                // نستخدم التزامن لقراءة لون الصورة الأصلية وتحديث الأشرطة بناءً عليه
                //Color sample = _originalLoadedBitmap.GetPixel(0, 0);
                //SyncTrackBarsWithColor(sample, _currentImageSystem);
                ResetChannelControls(); // كل شيء على 0 عند تحميل صورة

                UpdateChannelControls(_currentImageSystem);
                ApplyColorTransformation(_currentImageSystem);
                ToggleControls(true); // تفعيل الأدوات بعد نجاح التحميل
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

            // الأبعاد من الصورة الحالية (قد تكون معالجة)
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


        //private void btnImageInfo_Click(object sender, EventArgs e)
        //{
        //    // 1. التحقق من وجود صورة معروضة حالياً
        //    if (pictureBoxMain.Image == null)
        //    {
        //        MessageBox.Show("الرجاء تحميل صورة أولاً لعرض معلوماتها.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    try
        //    {
        //        string name = "صورة معالجة داخل التطبيق";
        //        string format = "Memory Mat / Bitmap Strip";

        //        // 2. قراءة الأبعاد مباشرة من الكائن الأصلي لتجنب خطأ الـ Parameter is not valid
        //        int width = pictureBoxMain.Image.Width;
        //        int height = pictureBoxMain.Image.Height;

        //        int channels = 3;
        //        int bpp = 24;
        //        string colorSystem = _currentImageSystem;

        //        // 3. حساب القنوات وعمق البت بناءً على النظام اللوني الحالي المختار في الواجهة
        //        string systemUpper = colorSystem.ToUpper();
        //        if (systemUpper == "CMYK")
        //        {
        //            channels = 4;
        //            bpp = 32;
        //        }
        //        else if (systemUpper == "GRAY" || pictureBoxMain.Image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
        //        {
        //            channels = 1;
        //            bpp = 8;
        //        }
        //        else
        //        {
        //            // الأنظمة الأخرى مثل RGB, HSV, YCbCr, LAB, CMY كلها تعتمد على 3 قنوات
        //            channels = 3;
        //            bpp = 24;
        //        }

        //        // 4. فتح واجهة عرض الخصائص بأمان
        //        this.Invoke((MethodInvoker)delegate
        //        {
        //            ImageInfoForm infoForm = new ImageInfoForm(name, format, width, height, channels, 0, bpp, colorSystem);
        //            infoForm.ShowDialog(this);
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"تعذر جلب البيانات مباشرة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}




        private void NewMethod(ImageInfoForm infoForm)
        {
            NewMethod1(infoForm);
        }

        private void NewMethod1(ImageInfoForm infoForm)
        {
            infoForm.ShowDialog(this);
        }

        private void btnDisplaySpaces_Click(object sender, EventArgs e)
        {
            //if (_imageManager.CurrentImage == null)
            //{
            //    MessageBox.Show("Load an image first.", "Info",
            //        MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

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
            // هذا الحدث سيعمل الآن فور تحريك أي شريط (TrackBar)
            ApplyColorTransformation(_currentImageSystem);
        }

        private void ApplyColorTransformation(string targetSystem)
        {
            // 1. التحقق من وجود الصورة الأصلية
            Bitmap source = _quantizedBitmap ?? _originalLoadedBitmap;

            // 2. جلب القيم من أشرطة التمرير
            int v1 = trackBarCh1.Value;
            int v2 = trackBarCh2.Value;
            int v3 = trackBarCh3.Value;

            // ضبط القيمة الرابعة بناءً على النظام اللوني
            int v4 = (targetSystem.ToUpper() == "CMYK") ? trackBarCh4.Value : 0;

            // 3. استدعاء دالة التحويل
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

            // 4. تحديث واجهة المستخدم وإدارة الذاكرة
            if (resultBitmap != null)
            {
                // حفظ الصورة الحالية قبل استبدالها
                Image oldImage = pictureBoxMain.Image;

                // عرض الصورة الجديدة
                pictureBoxMain.Image = resultBitmap;
                pictureBoxMain.Refresh();

                // التخلص من الصورة القديمة (Dispose) لمنع تسريب الذاكرة
                // بشرط ألا تكون هي الصورة الأصلية المحملة
                if (oldImage != null && oldImage != _originalLoadedBitmap)
                {
                    oldImage.Dispose();
                }
            }
        }

        private void SyncTrackBarsWithColor(Color c, string system)
        {
            _isUpdatingCombo = true; // منع إطلاق الـ Events
            SetTrackBarRanges(system); // ضبط المدى قبل القيمة

            int v1 = 0, v2 = 0, v3 = 0, v4 = 0;

            switch (system.ToUpper())
            {
                case "RGB":
                    v1 = c.R; v2 = c.G; v3 = c.B;
                    break;

                case "CMY":
                    v1 = 255 - c.R; v2 = 255 - c.G; v3 = 255 - c.B;
                    break;

                case "CMYK":
                    int k = Math.Min(c.R, Math.Min(c.G, c.B));
                    v1 = 255 - c.R; v2 = 255 - c.G; v3 = 255 - c.B; v4 = 255 - k;
                    break;

                case "HSV":
                    v1 = (int)c.GetHue();
                    v2 = (int)(c.GetSaturation() * 100);
                    v3 = (int)(c.GetBrightness() * 100);
                    break;

                case "YUV":
                    v1 = Clamp((int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B), 0, 255);
                    v2 = Clamp((int)(-0.147 * c.R - 0.289 * c.G + 0.436 * c.B + 128), 0, 255);
                    v3 = Clamp((int)(0.615 * c.R - 0.515 * c.G - 0.100 * c.B + 128), 0, 255);
                    break;

                case "YCBCR":
                    v1 = Clamp((int)(16 + (65.481 * c.R + 128.553 * c.G + 24.966 * c.B) / 256), 16, 235);
                    v2 = Clamp((int)(128 + (-37.797 * c.R - 74.203 * c.G + 112.0 * c.B) / 256), 16, 240);
                    v3 = Clamp((int)(128 + (112.0 * c.R - 93.786 * c.G - 18.214 * c.B) / 256), 16, 240);
                    break;

                case "LAB":
                    double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
                    double X = 0.4124564 * r + 0.3575761 * g + 0.1804375 * b;
                    double Y = 0.2126729 * r + 0.7151522 * g + 0.0721750 * b;
                    double Z = 0.0193339 * r + 0.1191920 * g + 0.9503041 * b;
                    v1 = Clamp((int)(116 * Math.Pow(Y, 1.0 / 3.0) - 16), 0, 100);
                    v2 = Clamp((int)(500 * (Math.Pow(X, 1.0 / 3.0) - Math.Pow(Y, 1.0 / 3.0))), -128, 127);
                    v3 = Clamp((int)(200 * (Math.Pow(Y, 1.0 / 3.0) - Math.Pow(Z, 1.0 / 3.0))), -128, 127);
                    break;
            }

            trackBarCh1.Value = Clamp(v1, trackBarCh1.Minimum, trackBarCh1.Maximum);
            trackBarCh2.Value = Clamp(v2, trackBarCh2.Minimum, trackBarCh2.Maximum);
            trackBarCh3.Value = Clamp(v3, trackBarCh3.Minimum, trackBarCh3.Maximum);

            if (trackBarCh4.Visible)
                trackBarCh4.Value = Clamp(v4, trackBarCh4.Minimum, trackBarCh4.Maximum);

            _isUpdatingCombo = false;
        }

        // دالة مساعدة لضبط النطاقات (يجب استدعاؤها أيضاً عند تغيير الـ ComboBox)
        private void SetTrackBarRanges(string system)
        {
            //if (system == "LAB")
            //{
            //    trackBarCh1.Minimum = 0; trackBarCh1.Maximum = 100;
            //    trackBarCh2.Minimum = -128; trackBarCh2.Maximum = 127;
            //    trackBarCh3.Minimum = -128; trackBarCh3.Maximum = 127;
            //}
            //else if (system == "HSV")
            //{
            //    trackBarCh1.Minimum = 0; trackBarCh1.Maximum = 360;
            //    trackBarCh2.Minimum = 0; trackBarCh2.Maximum = 100;
            //    trackBarCh3.Minimum = 0; trackBarCh3.Maximum = 100;
            //}
            //else
            //{
            //    // الافتراضي للأنظمة الأخرى (RGB, CMY, YUV, YCbCr)
            //    trackBarCh1.Minimum = 0; trackBarCh1.Maximum = 255;
            //    trackBarCh2.Minimum = 0; trackBarCh2.Maximum = 255;
            //    trackBarCh3.Minimum = 0; trackBarCh3.Maximum = 255;
            //    if (trackBarCh4.Visible) { trackBarCh4.Minimum = 0; trackBarCh4.Maximum = 255; }
            //}
            switch (system)
            {
                case "RGB":
                    trackBarCh1.Minimum = -255; trackBarCh1.Maximum = 255;
                    trackBarCh2.Minimum = -255; trackBarCh2.Maximum = 255;
                    trackBarCh3.Minimum = -255; trackBarCh3.Maximum = 255;
                    break;
                case "CMY":
                    trackBarCh1.Minimum = 255; trackBarCh1.Maximum = -255;
                    trackBarCh2.Minimum = 255; trackBarCh2.Maximum = -255;
                    trackBarCh3.Minimum = 255; trackBarCh3.Maximum = -255;
                    break;

                case "HSV":
                    trackBarCh1.Minimum = -180; trackBarCh1.Maximum = 180; // H
                    trackBarCh2.Minimum = -100; trackBarCh2.Maximum = 100; // S
                    trackBarCh3.Minimum = -100; trackBarCh3.Maximum = 100; // V
                    break;

                case "YUV":
                case "YCBCR":
                case "LAB":
                    trackBarCh1.Minimum = -255; trackBarCh1.Maximum = 255;
                    trackBarCh2.Minimum = -255; trackBarCh2.Maximum = 255;
                    trackBarCh3.Minimum = -255; trackBarCh3.Maximum = 255;
                    break;
            }

            
        }

        private int Clamp(int val, int min, int max) => Math.Max(min, Math.Min(max, val));
        private void cmbColorSpaces_SelectedIndexChanged(object sender, EventArgs e)
{
    if (_isUpdatingCombo || _originalLoadedBitmap == null) return;

    _currentImageSystem = cmbColorSpaces.SelectedItem.ToString().ToUpper();
    
    // التقاط عينة من وسط الصورة
    Color sample = _originalLoadedBitmap.GetPixel(_originalLoadedBitmap.Width / 2, _originalLoadedBitmap.Height / 2);

            // التزامن
            //SyncTrackBarsWithColor(sample, _currentImageSystem);
            ResetChannelControls();
            _quantizedBitmap?.Dispose();
            _quantizedBitmap = null;

            // تحديث الواجهة والتحويل
            UpdateChannelControls(_currentImageSystem);
    ApplyColorTransformation(_currentImageSystem);
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

        

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            //if (_imageManager.CurrentImage == null) return;


            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("No image loaded yet.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //_imageManager.Reset();
            //DisplayImage();



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

        //private async void numKColors_ValueChanged(object sender, EventArgs e)
        //{
        //    if (pictureBoxMain.Image == null) return;

        //    if (pictureBoxMain.Tag == null)
        //    {
        //        pictureBoxMain.Tag = new Bitmap(pictureBoxMain.Image);
        //    }

        //    Bitmap backupBmp = (Bitmap)pictureBoxMain.Tag;
        //    int selectedK = (int)numKColors.Value;

        //    try
        //    {
        //        numKColors.Enabled = false;

        //        Bitmap currentDisplayedBmp = (Bitmap)pictureBoxMain.Image;

        //        Mat currentMat = ColorConvertor.BitmapToMat(currentDisplayedBmp);

        //        Mat resultMat = await Task.Run(() =>
        //            ColorConvertor.QuantizeColorsAdvanced(currentMat, selectedK, _currentImageSystem)
        //        );

        //        if (resultMat != null && !resultMat.IsEmpty)
        //        {
        //            Image oldImg = pictureBoxMain.Image;

        //            pictureBoxMain.Image = ColorConvertor.MatToBitmap(resultMat);
        //            pictureBoxMain.Refresh();

        //            if (oldImg != null && oldImg != backupBmp)
        //            {
        //                oldImg.Dispose();
        //            }

        //            resultMat.Dispose();
        //        }

        //        currentMat.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"حدث خطأ أثناء معالجة الألوان: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        numKColors.Enabled = true;
        //        numKColors.Focus();
        //    }
        //}




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

        private void checkBoxCh1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void trackBarCh1_Scroll(object sender, EventArgs e)
        {

        }



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







        /*private async void numKColors_ValueChanged(object sender, EventArgs e)
        {
            if (pictureBoxMain.Image == null) return;

            if (_backupOriginalBitmap == null)
            {
                _backupOriginalBitmap = new Bitmap(pictureBoxMain.Image);
            }

            int selectedK = (int)numKColors.Value;

            try
            {
                numKColors.Enabled = false;

                // استخدام التحويل المحلي النظيف المستقر
                Mat currentMat = ColorConvertor.BitmapToMat(_backupOriginalBitmap);

                // استدعاء الخوارزمية بـ وسيطين فقط (k و المصفوفة)
                Mat resultMat = await Task.Run(() =>
                    ColorConvertor.QuantizeColors(currentMat, selectedK)
                );

                if (resultMat != null && !resultMat.IsEmpty)
                {
                    Image oldImg = pictureBoxMain.Image;

                    pictureBoxMain.Image = ColorConvertor.MatToBitmap(resultMat);
                    pictureBoxMain.Refresh();

                    if (oldImg != null && oldImg != _backupOriginalBitmap) oldImg.Dispose();
                    resultMat.Dispose();
                }

                currentMat.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء معالجة الألوان: {ex.Message}");
            }
            finally
            {
                numKColors.Enabled = true;
                numKColors.Focus();
            }
        }*/
    }
}