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
        private readonly ImageManager _imageManager = new ImageManager();
        private string _currentImageSystem = "RGB";
        private Mat _originalLoadedMat = null;
        private Bitmap _backupOriginalBitmap = null; 
        
        // 1. أضفنا نظام الـ CMYK إلى مصفوفة الأنظمة المدعومة
        private readonly string[] _allColorSystems = { "RGB", "CMY", "CMYK", "HSV", "YCBCR", "YUV", "LAB" };
        private bool _isUpdatingCombo = false;

        // الاحتفاظ بالصورة الأصلية
        private Bitmap _originalLoadedBitmap = null;

        public MainForm()
        {
            InitializeComponent();
            SetupDragDrop();
            RegisterChannelEvents(); // ربط أحداث الأشرطة تلقائياً
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateAvailableTargets();
            numKColors.Enabled = false;
            // جعل المؤشر يقف افتراضياً على نظام RGB عند الإقلاع
            cmbColorSpaces.SelectedIndex = cmbColorSpaces.Items.IndexOf("RGB");
            UpdateChannelControls(_currentImageSystem);
            
        }

        // تعديل الدالة لإلغاء شرط الاستبعاد وفحص الأمان

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
                ResetChannelControls(); // إعادة تصفير الأشرطة فور تحميل صورة جديدة
                UpdateChannelControls(_currentImageSystem);
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
                    break;
                case "CMY":
                    checkBoxCh1.Text = "C (السيان)";
                    checkBoxCh2.Text = "M (الماجنتا)";
                    checkBoxCh3.Text = "Y (الأصفر)";
                    break;
                case "CMYK":
                    checkBoxCh1.Text = "C (السيان)";
                    checkBoxCh2.Text = "M (الماجنتا)";
                    checkBoxCh3.Text = "Y (الأصفر)";
                    checkBoxCh4.Text = "K (الأسود)";
                    break;
                case "HSV":
                    checkBoxCh1.Text = "H (التدرج)";
                    checkBoxCh2.Text = "S (التشبع)";
                    checkBoxCh3.Text = "V (القيمة/السطوع)";
                    break;
                case "YUV":
                case "YCBCR":
                    checkBoxCh1.Text = "Y (الإضاءة)";
                    checkBoxCh2.Text = (targetSystem.ToUpper() == "YUV") ? "U (فرق الأزرق)" : "Cb (فرق الأزرق)";
                    checkBoxCh3.Text = (targetSystem.ToUpper() == "YUV") ? "V (فرق الأحمر)" : "Cr (فرق الأحمر)";
                    break;
                case "LAB":
                    checkBoxCh1.Text = "L (السطوع)";
                    checkBoxCh2.Text = "A (أخضر-أحمر)";
                    checkBoxCh3.Text = "B (أزرق-أصفر)";
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

        private void RegisterChannelEvents()
        {
            trackBarCh1.Scroll += ChannelControl_Changed;
            trackBarCh2.Scroll += ChannelControl_Changed;
            trackBarCh3.Scroll += ChannelControl_Changed;
            trackBarCh4.Scroll += ChannelControl_Changed;

            checkBoxCh1.CheckedChanged += ChannelControl_Changed;
            checkBoxCh2.CheckedChanged += ChannelControl_Changed;
            checkBoxCh3.CheckedChanged += ChannelControl_Changed;
            checkBoxCh4.CheckedChanged += ChannelControl_Changed;
        }

        private void ChannelControl_Changed(object sender, EventArgs e)
        {
            ApplyColorTransformation(_currentImageSystem);
        }

        private void ApplyColorTransformation(string targetSystem)
        {
            if (_originalLoadedBitmap == null) return;

            try
            {
                Bitmap resultBitmap = ColorConvertor.ConvertBetweenAnySpaces(
                    _originalLoadedBitmap,
                    "RGB",
                    targetSystem,
                    trackBarCh1.Value, trackBarCh2.Value, trackBarCh3.Value, trackBarCh4.Value,
                    checkBoxCh1.Checked, checkBoxCh2.Checked, checkBoxCh3.Checked, checkBoxCh4.Checked
                );

                if (resultBitmap != null)
                {
                    var oldImg = pictureBoxMain.Image;
                    pictureBoxMain.Image = resultBitmap;
                    if (oldImg != _originalLoadedBitmap) oldImg?.Dispose();

                    pictureBoxMain.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء معالجة القنوات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void cmbColorSpaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCombo || cmbColorSpaces.SelectedItem == null) return;

            string selectedSystem = cmbColorSpaces.SelectedItem.ToString();

            _currentImageSystem = selectedSystem;

            ResetChannelControls();

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