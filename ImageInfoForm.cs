using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelLab
{
    public partial class ImageInfoForm : Form
    {
        // عدّل باني الدالة (Constructor) الحالي ليكون بهذا الشكل:
        public ImageInfoForm(string name, string format, int width, int height, int channels, long size, int bpp, string colorSystem)
        {
            InitializeComponent();

            // إعدادات النافذة وتوسيطها
            this.Text = "خصائص ومعلومات الصورة الحالية";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(420, 480);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // لوحة تنظيم العناصر بشكل عمودي مريح
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(20),
                WrapContents = false
            };

            // دالة مساعدة لإنشاء سطر معلومات منسق
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

            // عنوان النافذة الداخلي
            Label lblHeader = new Label
            {
                Text = "بيانات الصورة الحية",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            panel.Controls.Add(lblHeader);

            // عرض البيانات الممررة من الواجهة الرئيسية
            AddInfoRow("أبعاد الصورة", $"{width} × {height} بكسل");
            AddInfoRow("فضاء الألوان الحالي", colorSystem.ToUpper());
            AddInfoRow("عدد القنوات (Channels)", $"{channels} قنوات");
            AddInfoRow("عمق البت (Depth)", $"{bpp} بت لكل بكسل (bpp)");
            AddInfoRow("صيغة المعالجة", format);

            // إضافة خط تجميلي قبل زر الإغلاق
            Label lblLine = new Label { Text = "________________________________________", ForeColor = Color.LightGray, AutoSize = true };
            panel.Controls.Add(lblLine);

            // زر إغلاق النافذة
            Button btnClose = new Button
            {
                Text = "إغلاق",
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Margin = new Padding(0, 15, 0, 0),
                DialogResult = DialogResult.OK
            };
            panel.Controls.Add(btnClose);

            this.Controls.Add(panel);
            this.AcceptButton = btnClose;
        }
    }
}