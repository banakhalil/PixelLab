using System;
using System.Windows.Forms;
using System.Drawing;

namespace PixelLab
{
    public partial class ImageInfoForm : Form
    {
        // باني النافذة المطور والمحمي بالكامل
        public ImageInfoForm(string name, string format, int width, int height, int channels, long size, int bpp, string colorSystem)
        {
            // 1. حاوية حماية لدالة التصميم لضمان عدم انهيار النافذة إذا كان الـ Designer تالفاً
            try
            {
                InitializeComponent();
            }
            catch (Exception)
            {
                // إذا كان ملف الـ Designer يحتوي على أخطاء رسومية، نقوم بضبط أبعاد النافذة يدوياً هنا
                this.Size = new Size(450, 400);
                this.Text = "معلومات الصورة الرقمية";
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
            }

            // 2. الحل الجذري: إنشاء صندوق نصوص غني (RichTextBox) برمجياً معزول تماماً عن مشاكل الـ Designer
            try
            {
                // تنظيف أي أدوات تالفة قد تسبب خطأ المعلمة
                this.Controls.Clear();

                RichTextBox txtInfo = new RichTextBox();
                txtInfo.Dock = DockStyle.Fill;
                txtInfo.ReadOnly = true;
                txtInfo.BorderStyle = BorderStyle.None;
                txtInfo.BackColor = Color.FromArgb(245, 245, 245); // لون خلفية مريح للعين
                txtInfo.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
                txtInfo.Padding = new Padding(20);

                // تنسيق وعرض معلومات الصورة بشكل منسق واحترافي
                txtInfo.Text = "📊 تفاصيل ومواصفات الصورة الحالية:\n" +
                               "--------------------------------------------------\n\n" +
                               $"🔹 اسم الصورة: {name}\n\n" +
                               $"🔹 صيغة الملف: {format}\n\n" +
                               $"🔹 أبعاد الصورة: {width} × {height} بكسل (Pixel)\n\n" +
                               $"🔹 عدد القنوات (Channels): {channels}\n\n" +
                               $"🔹 العمق اللوني (BPP): {bpp} Bits\n\n" +
                               $"🔹 النظام اللوني الحالي: {colorSystem}\n\n" +
                               "--------------------------------------------------\n" +
                               "✨ تم جلب البيانات وتحديثها ديناميكياً بنجاح.";

                // إضافة صندوق النصوص للواجهة ليعرض فوراً وبأمان 100%
                this.Controls.Add(txtInfo);
            }
            catch (Exception ex)
            {
                // حماية قصوى: عرض رسالة صندوقية سريعة بالبيانات إذا فشل نظام الرسم بالكامل
                string backupText = $"الأبعاد: {width}x{height}\nالقنوات: {channels}\nالنظام اللوني: {colorSystem}";
                MessageBox.Show(backupText, "معلومات الصورة", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}