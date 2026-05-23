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

namespace PixelLab
{
    public partial class MainForm : Form
    {
        private readonly ImageManager _imageManager = new ImageManager();
        private string _currentImageSystem = "RGB";

        // 1. أضفنا نظام الـ CMYK إلى مصفوفة الأنظمة المدعومة
        private readonly string[] _allColorSystems = { "RGB", "CMY", "CMYK", "HSV", "YCBCR", "YUV", "LAB" };
        private bool _isUpdatingCombo = false;

        // الاحتفاظ بالصورة النظيفة لكي نعدل القنوات عليها دون خسارة البيانات الأصلية
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
            UpdateChannelControls(_currentImageSystem); // تحديث الأسماء لأول مرة (RGB افتراضياً)
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
            
        }

        // requirement 8
        private void btnImageInfo_Click(object sender, EventArgs e)
        {
            // 1. فحص الأمان: إذا لم تكن هناك صورة محملة أصلاً في الواجهة
            if (_imageManager == null)
            {
                MessageBox.Show("الرجاء تحميل صورة أولاً لعرض معلوماتها.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // متغيرات لحفظ القيم
                string name = "صورة معالجة";
                string format = "PNG / Bitmap";
                int width = 0;
                int height = 0;

                // 2. الحل الذكي: نحاول جلب الصورة الحالية، وإن لم تكن جاهزة نأخذ الصورة الأصلية (التي تظهر بعد الـ Reset) تلقائياً!
                System.Drawing.Image imageToRead = _imageManager.CurrentImage ?? _imageManager.OriginalImage;

                if (imageToRead == null)
                {
                    MessageBox.Show("لم يتم العثور على بيانات صورة صالحة في الذاكرة.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3. قراءة البيانات بأمان عبر كبسولة ذاكرة معزولة تفادياً للقفل البرمجي
                lock (imageToRead) // نمنع أي دالة أخرى من قفل الصورة أثناء القراءة
                {
                    using (System.Drawing.Bitmap tempBitmap = new System.Drawing.Bitmap(imageToRead))
                    {
                        width = tempBitmap.Width;
                        height = tempBitmap.Height;
                        format = imageToRead.RawFormat?.ToString() ?? "معالجة داخلية";
                    }
                }

                int channels = 3;
                int bpp = 24;
                string colorSystem = "RGB";

                // 4. عرض النافذة مباشرة وبشكل فوري
                this.Invoke((MethodInvoker)delegate
                {
                    ImageInfoForm infoForm = new ImageInfoForm(name, format, width, height, channels, 0, bpp, colorSystem);
                    infoForm.ShowDialog(this);
                });
            }
            catch (Exception ex)
            {
                // إذا حدث أي تضارب، نقوم بعمل محاكاة سريعة للـ Reset برمجياً لجلب الأبعاد دون إزعاج المستخدم
                try
                {
                    if (_imageManager.OriginalImage != null)
                    {
                        int w = _imageManager.OriginalImage.Width;
                        int h = _imageManager.OriginalImage.Height;
                        ImageInfoForm infoForm = new ImageInfoForm("صورة معالجة", "Bitmap", w, h, 3, 0, 24, "RGB");
                        infoForm.ShowDialog(this);
                        return;
                    }
                }
                catch { }

                MessageBox.Show($"تعذر جلب البيانات مباشرة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NewMethod(ImageInfoForm infoForm)
        {
            NewMethod1(infoForm);
        }

        private void NewMethod1(ImageInfoForm infoForm)
        {
            infoForm.ShowDialog(this);
        }

        //requirement 4
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


        // requirement 2
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

        // ========================================================
        // 🛠️ الإضافات الجديدة: إدارة وتحديث القنوات الرسومية ديناميكياً
        // ========================================================

        private void UpdateChannelControls(string targetSystem)
        {
            // إخفاء عناصر القناة الرابعة افتراضياً (لأنها تستخدم فقط مع CMYK)
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
            // تصفير القيم وإعادة التفعيل عند الانتقال لنظام جديد
            trackBarCh1.Value = 0;
            trackBarCh2.Value = 0;
            trackBarCh3.Value = 0;
            trackBarCh4.Value = 0;

            checkBoxCh1.Checked = true;
            checkBoxCh2.Checked = true;
            checkBoxCh3.Checked = true;
            checkBoxCh4.Checked = true;
        }

        // ربط الأحداث برمجياً للتأكد من التقاط أي حركة على الأشرطة أو الخيارات
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

        // دالة موحدة تُستدعى تلقائياً فور تعديل أي قناة أو شريط
        private void ChannelControl_Changed(object sender, EventArgs e)
        {
            ApplyColorTransformation(_currentImageSystem);
        }

        private void ApplyColorTransformation(string targetSystem)
        {
            if (_originalLoadedBitmap == null) return;

            try
            {
                // نستدعي دالة التحويل مع تمرير كافة قيم الأشرطة والـ CheckBoxes من الواجهة
                Bitmap resultBitmap = ColorConvertor.ConvertBetweenAnySpaces(
                    _originalLoadedBitmap,
                    "RGB", // الصورة الأصلية المحفوظة هي دائماً بنظام RGB النشط
                    targetSystem,
                    trackBarCh1.Value, trackBarCh2.Value, trackBarCh3.Value, trackBarCh4.Value,
                    checkBoxCh1.Checked, checkBoxCh2.Checked, checkBoxCh3.Checked, checkBoxCh4.Checked
                );

                if (resultBitmap != null)
                {
                    // استبدال الصورة القديمة في صندوق العرض وتنظيف ذاكرتها
                    var oldImg = pictureBoxMain.Image;
                    pictureBoxMain.Image = resultBitmap;
                    if (oldImg != _originalLoadedBitmap) oldImg?.Dispose();

                    pictureBoxMain.Refresh();

                    _currentImageSystem = targetSystem;
                    _imageManager.CurrentColorSystem = targetSystem; // تحديث النظام اللوني من اجل عرض معلومات الصورة
                    // تحديث الخيارات المتاحة بناءً على النظام الجديد المستقر للصورة
                    UpdateAvailableTargets();

                    this.Text = $"PixelLab - Current Space: [{_currentImageSystem}]";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء معالجة القنوات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========================================================

        private void cmbColorSpaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCombo || cmbColorSpaces.SelectedItem == null)
                return;

            if (_originalLoadedBitmap == null)
            {
                MessageBox.Show("الرجاء تحميل صورة أولاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string targetSystem = cmbColorSpaces.SelectedItem.ToString();

            // عند تغيير الكومبو بوكس: نقوم بتصفير الأشرطة وتحديث أسمائها ثم تطبيق التحويل
            _currentImageSystem = targetSystem;
            ResetChannelControls();
            UpdateChannelControls(targetSystem);
            UpdateAvailableTargets();

            ApplyColorTransformation(targetSystem);
            this.Text = $"PixelLab - Current Space: [{_currentImageSystem}]";
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
            if (_imageManager.CurrentImage == null) return;

            _imageManager.Reset();
            DisplayImage();
        }
        private void label1_Click(object sender, EventArgs e) { }
    }
}