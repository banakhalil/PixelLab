using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace PixelLab
{
    //requirement 4 + 5
    public partial class ColorSpaceForm : Form
    {
        
        private OpenTK.GLControl _glControl;
        //private Bitmap _sourceBitmap;
        private string _currentSpace = "RGB";
        private bool _spaceLoaded = false;
        private float _zoom = 3.5f; 


        // التدوير
        private float _rotX = 25f;
        private float _rotY = -45f;
        private Point _lastMouse;
        private bool _isDragging = false;

        
        // Constructor 
        public ColorSpaceForm(Bitmap sourceBitmap)
        {
            //_sourceBitmap = sourceBitmap;
            InitializeComponent();
            SetupGLControl();
        }

        
        //  GLControl
        private void SetupGLControl()
        {

            _glControl = new OpenTK.GLControl(
                new GraphicsMode(32, 24, 0, 4));
            _glControl.Dock = DockStyle.Fill;

            _glControl.Load += GL_Load;
            _glControl.Paint += GL_Paint;
            _glControl.Resize += GL_Resize;

            // كليك يسار للتدوير ويمين لاختيار لون
            _glControl.MouseDown += (s, e) =>
            {
                _isDragging = true;
                _lastMouse = e.Location;

                if (e.Button == MouseButtons.Right) 
                    PickColor(e.Location);
            };
            _glControl.MouseUp += (s, e) => _isDragging = false;
            _glControl.MouseMove += (s, e) =>
            {
                if (!_isDragging) return;
                _rotY += (e.X - _lastMouse.X) * 0.5f;
                _rotX += (e.Y - _lastMouse.Y) * 0.5f;
                _lastMouse = e.Location;
                _glControl.Invalidate();
            };

            _glControl.MouseWheel += (s, e) =>
            {
                _zoom -= e.Delta * 0.001f;        //  تقريب
                _zoom = Math.Max(1.0f, Math.Min(_zoom, 8f)); //  حدود الزوم
                _glControl.Invalidate();
            };

            pnlRender.Controls.Add(_glControl);
        }

        // OpenGL Setup 
        private void GL_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.12f, 0.12f, 0.12f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.PointSmooth);
            GL.PointSize(2.5f);
            SetProjection();
        }

        private void GL_Resize(object sender, EventArgs e) => SetProjection();

        private void SetProjection()
        {
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            float aspect = (float)_glControl.Width / _glControl.Height;
            var proj = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f), aspect, 0.1f, 100f);
            GL.LoadMatrix(ref proj);
        }

        //  Render Loop 
        private void GL_Paint(object sender, PaintEventArgs e)
        {
            _glControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (!_spaceLoaded)        // الرسم مع الزر
            {
                _glControl.SwapBuffers();
                return;
            }

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(0f, 0.3f, -_zoom);
            GL.Rotate(_rotX, 1f, 0f, 0f);
            GL.Rotate(_rotY, 0f, 1f, 0f);
            GL.Translate(-0.5f, -0.5f, -0.5f);

            switch (_currentSpace)
            {
                case "RGB": DrawRGBAxes(); DrawRGBCubeFaces(); break;
                case "HSV": DrawHSVAxes(); DrawHSVCone(); break;
                case "YCBCR": DrawYCbCrAxes(); DrawYCbCrPlane(); break;
                case "YUV": DrawYUVAxes(); DrawYUVPlane(); break;
                case "CMY": DrawCMYAxes(); DrawCMYCubeFaces(); break;
                case "LAB": DrawLABAxes(); DrawLABSphere(); break;
            }

            _glControl.SwapBuffers(); //للاظهار على الشاشة
        }


        // اختيار اللون
        private void PickColor(Point screenPos)
        {
            if (!_spaceLoaded) return;

            _glControl.MakeCurrent();

            // OpenGL origin is bottom-left, WinForms is top-left
            int flippedY = _glControl.Height - screenPos.Y;

            byte[] pixel = new byte[3];
            GL.ReadPixels(screenPos.X, flippedY, 1, 1,
                          PixelFormat.Rgb, PixelType.UnsignedByte, pixel);

            float r = pixel[0] / 255f; //OpenGL is 0-1
            float g = pixel[1] / 255f;
            float b = pixel[2] / 255f;

            // تجاهل الخلفية الداكنة
            if (r < 0.02f && g < 0.02f && b < 0.02f) return;

            DisplayColorValues(r, g, b);
        }

        private void DisplayColorValues(float r, float g, float b)
        {
            // RGB
            int R = (int)(r * 255), G = (int)(g * 255), B = (int)(b * 255);

            // HSV
            float h, s, v;
            RgbToHsv(r, g, b, out h, out s, out v);

            // CMY
            int C = 255 - R, M = 255 - G, Y = 255 - B;

            // YUV
            float Yu = 0.299f * r + 0.587f * g + 0.114f * b;
            float U = -0.147f * r - 0.289f * g + 0.436f * b;
            float V = 0.615f * r - 0.515f * g - 0.100f * b;

            // YCbCr
            float Yy = 0.299f * r + 0.587f * g + 0.114f * b;
            float Cb = -0.169f * r - 0.331f * g + 0.500f * b;
            float Cr = 0.500f * r - 0.419f * g - 0.081f * b;

            // LAB
            float L, a, bLab;
            RgbToLab(r, g, b, out L, out a, out bLab);

            // عرض اللون المختار
            var swatch = new Panel
            {
                Size = new Size(30, 30),
                BackColor = Color.FromArgb(R, G, B),
                Location = new Point(10, 10)
            };

            string text =
                $"  RGB    →  ({R}, {G}, {B})\n" +
                $"  HSV    →  ({h:F0}°, {s * 100:F0}%, {v * 100:F0}%)\n" +
                $"  CMY    →  ({C}, {M}, {Y})\n" +
                $"  YUV    →  ({Yu:F2}, {U:F2}, {V:F2})\n" +
                $"  YCbCr  →  ({Yy:F2}, {Cb:F2}, {Cr:F2})\n" +
                $"  LAB    →  ({L:F1}, {a:F1}, {bLab:F1})";

            // تحديث pnlColorSimulate real-time
            pnlColorSimulate.Controls.Clear();
            pnlColorSimulate.Controls.Add(swatch);

            var lbl = new Label
            {
                Text = text,
                Font = new Font("Microsoft Sans Serif", 12),
                ForeColor = Color.Black,
                AutoSize = false,
                Size = new Size(pnlColorSimulate.Width - 60, pnlColorSimulate.Height - 10),
                Location = new Point(50, 5)
                
            };
            pnlColorSimulate.Controls.Add(lbl);
        }


        // تحويل لون بكسل واحد، التوابع الجاهزة بدها صورة كاملة
        //اساسا فكل شي رح يقراه هو هيك، فلنعرض اللون بالنظام الصح منحولو rgb الحاسوب شغال بنظام 
        private void RgbToHsv(float r, float g, float b,
                       out float h, out float s, out float v)
        {
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float diff = max - min;

            v = max;
            s = max == 0 ? 0 : diff / max;

            if (diff == 0) { h = 0; return; }

            if (max == r) h = 60 * ((g - b) / diff % 6);
            else if (max == g) h = 60 * ((b - r) / diff + 2);
            else h = 60 * ((r - g) / diff + 4);

            if (h < 0) h += 360;
        }

        private void RgbToLab(float r, float g, float b,
                               out float L, out float a, out float bOut)
        {
            double R = r > 0.04045 ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            double G = g > 0.04045 ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            double B = b > 0.04045 ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            double x = (R * 0.4124 + G * 0.3576 + B * 0.1805) / 0.95047;
            double y = (R * 0.2126 + G * 0.7152 + B * 0.0722) / 1.00000;
            double z = (R * 0.0193 + G * 0.1192 + B * 0.9505) / 1.08883;

            x = x > 0.008856 ? Math.Pow(x, 1.0 / 3) : 7.787 * x + 16.0 / 116;
            y = y > 0.008856 ? Math.Pow(y, 1.0 / 3) : 7.787 * y + 16.0 / 116;
            z = z > 0.008856 ? Math.Pow(z, 1.0 / 3) : 7.787 * z + 16.0 / 116;

            L = (float)(116 * y - 16);
            a = (float)(500 * (x - y));
            bOut = (float)(200 * (y - z));
        }


        //  رسم المحاور لكل نظام لوني

        private void DrawRGBAxes()
        {
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(1f, 0f, 0f); GL.Vertex3(0, 0, 0); GL.Vertex3(1, 0, 0); // R
            GL.Color3(0f, 1f, 0f); GL.Vertex3(0, 0, 0); GL.Vertex3(0, 1, 0); // G
            GL.Color3(0f, 0f, 1f); GL.Vertex3(0, 0, 0); GL.Vertex3(0, 0, 1); // B
            GL.End();
        }

        private void DrawHSVAxes()
        {
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);

            // V axis — عمودي في مركز المخروط
            GL.Color3(1f, 1f, 1f);
            GL.Vertex3(0.5f, 0f, 0.5f);
            GL.Vertex3(0.5f, 1.2f, 0.5f);

            // S axis — أفقي يمثل نصف القطر
            GL.Color3(0.8f, 0.8f, 0.8f);
            GL.Vertex3(0.5f, 1f, 0.5f);
            GL.Vertex3(1.1f, 1f, 0.5f);

            GL.End();

            // دائرة H في الأعلى
            GL.LineWidth(1f);
            GL.Color3(0.6f, 0.6f, 0.6f);
            GL.Begin(PrimitiveType.LineLoop);
            for (int i = 0; i < 72; i++)
            {
                float angle = i * 2f * (float)Math.PI / 72;
                GL.Vertex3(0.5f + 0.52f * (float)Math.Cos(angle),
                           1f,
                           0.5f + 0.52f * (float)Math.Sin(angle));
            }
            GL.End();
        }

        private void DrawYCbCrAxes()
        {
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);

            // Cb axis — (أفقي (أزرق
            GL.Color3(0.3f, 0.5f, 1f);
            GL.Vertex3(0f, 0.5f, 0.5f);
            GL.Vertex3(1.1f, 0.5f, 0.5f);

            // Cr axis — (عمودي (أحمر
            GL.Color3(1f, 0.3f, 0.3f);
            GL.Vertex3(0.5f, 0f, 0.5f);
            GL.Vertex3(0.5f, 1.1f, 0.5f);

            GL.End();
        }

        private void DrawYUVAxes()
        {
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(0.8f, 0.8f, 0.2f); // U — أصفر
            GL.Vertex3(0f, 0.5f, 0.5f); GL.Vertex3(1.1f, 0.5f, 0.5f);
            GL.Color3(0.2f, 0.8f, 0.8f); // V — سماوي
            GL.Vertex3(0.5f, 0f, 0.5f); GL.Vertex3(0.5f, 1.1f, 0.5f);
            GL.End();
        }

        private void DrawCMYAxes()
        {
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(0f, 1f, 1f); // C — سماوي
            GL.Vertex3(0, 0, 0); GL.Vertex3(1, 0, 0);
            GL.Color3(1f, 0f, 1f); // M — أرجواني
            GL.Vertex3(0, 0, 0); GL.Vertex3(0, 1, 0);
            GL.Color3(1f, 1f, 0f); // Y — أصفر
            GL.Vertex3(0, 0, 0); GL.Vertex3(0, 0, 1);
            GL.End();
        }

        private void DrawLABAxes()
        {
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(1f, 1f, 1f); // L — أبيض عمودي
            GL.Vertex3(0.5f, 0f, 0.5f); GL.Vertex3(0.5f, 1.1f, 0.5f);
            GL.Color3(1f, 0f, 0f);  // a — أحمر/أخضر
            GL.Vertex3(0f, 0.5f, 0.5f); GL.Vertex3(1.1f, 0.5f, 0.5f);
            GL.Color3(1f, 1f, 0f);  // b — أصفر/أزرق
            GL.Vertex3(0.5f, 0.5f, 0f); GL.Vertex3(0.5f, 0.5f, 1.1f);
            GL.End();
        }


        //RGB CUBE SPACE
        private void DrawRGBCubeFaces()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Begin(PrimitiveType.Quads);

            // B=0 (الوجه الأمامي)
            GL.Color3(0f, 0f, 0f); GL.Vertex3(0, 0, 0); // Black
            GL.Color3(1f, 0f, 0f); GL.Vertex3(1, 0, 0); // Red
            GL.Color3(1f, 1f, 0f); GL.Vertex3(1, 1, 0); // Yellow
            GL.Color3(0f, 1f, 0f); GL.Vertex3(0, 1, 0); // Green

            // B=1 (الوجه الخلفي)
            GL.Color3(0f, 0f, 1f); GL.Vertex3(0, 0, 1); // Blue
            GL.Color3(1f, 0f, 1f); GL.Vertex3(1, 0, 1); // Magenta
            GL.Color3(1f, 1f, 1f); GL.Vertex3(1, 1, 1); // White
            GL.Color3(0f, 1f, 1f); GL.Vertex3(0, 1, 1); // Cyan

            // R=0 (الوجه الأيسر)
            GL.Color3(0f, 0f, 0f); GL.Vertex3(0, 0, 0); // Black
            GL.Color3(0f, 1f, 0f); GL.Vertex3(0, 1, 0); // Green
            GL.Color3(0f, 1f, 1f); GL.Vertex3(0, 1, 1); // Cyan
            GL.Color3(0f, 0f, 1f); GL.Vertex3(0, 0, 1); // Blue

            // R=1 (الوجه الأيمن)
            GL.Color3(1f, 0f, 0f); GL.Vertex3(1, 0, 0); // Red
            GL.Color3(1f, 1f, 0f); GL.Vertex3(1, 1, 0); // Yellow
            GL.Color3(1f, 1f, 1f); GL.Vertex3(1, 1, 1); // White
            GL.Color3(1f, 0f, 1f); GL.Vertex3(1, 0, 1); // Magenta

            // G=0 (الوجه السفلي)
            GL.Color3(0f, 0f, 0f); GL.Vertex3(0, 0, 0); // Black
            GL.Color3(1f, 0f, 0f); GL.Vertex3(1, 0, 0); // Red
            GL.Color3(1f, 0f, 1f); GL.Vertex3(1, 0, 1); // Magenta
            GL.Color3(0f, 0f, 1f); GL.Vertex3(0, 0, 1); // Blue

            // G=1 (الوجه العلوي)
            GL.Color3(0f, 1f, 0f); GL.Vertex3(0, 1, 0); // Green
            GL.Color3(1f, 1f, 0f); GL.Vertex3(1, 1, 0); // Yellow
            GL.Color3(1f, 1f, 1f); GL.Vertex3(1, 1, 1); // White
            GL.Color3(0f, 1f, 1f); GL.Vertex3(0, 1, 1); // Cyan

            GL.End();

            // إطار المكعب فوق الألوان
            GL.LineWidth(1.5f);
            GL.Color3(0.2f, 0.2f, 0.2f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0); GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 0, 0); GL.Vertex3(1, 0, 1);
            GL.Vertex3(1, 0, 1); GL.Vertex3(0, 0, 1);
            GL.Vertex3(0, 0, 1); GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 1, 0); GL.Vertex3(1, 1, 0);
            GL.Vertex3(1, 1, 0); GL.Vertex3(1, 1, 1);
            GL.Vertex3(1, 1, 1); GL.Vertex3(0, 1, 1);
            GL.Vertex3(0, 1, 1); GL.Vertex3(0, 1, 0);
            GL.Vertex3(0, 0, 0); GL.Vertex3(0, 1, 0);
            GL.Vertex3(1, 0, 0); GL.Vertex3(1, 1, 0);
            GL.Vertex3(1, 0, 1); GL.Vertex3(1, 1, 1);
            GL.Vertex3(0, 0, 1); GL.Vertex3(0, 1, 1);
            GL.End();
        }


        // HSV CONE SPACE
        private void DrawHSVCone()
        {
            int segments = 72;
            float cx = 0.5f, cz = 0.5f, radius = 0.5f;

            // القرص العلوي (V=1، من أبيض في المركز إلى ألوان في الحافة)
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(1f, 1f, 1f);           // مركز أبيض
            GL.Vertex3(cx, 1f, cz);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * 2f * (float)Math.PI / segments;
                float h = i * 360f / segments;
                Color c = HsvToRgb(h, 1f, 1f);
                GL.Color3(c.R / 255f, c.G / 255f, c.B / 255f);
                GL.Vertex3(cx + radius * (float)Math.Cos(angle), 1f,
                           cz + radius * (float)Math.Sin(angle));
            }
            GL.End();

            // (جوانب المخروط (من أسود في القاع إلى ألوان في الأعلى
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(0f, 0f, 0f);           // قمة سوداء
            GL.Vertex3(cx, 0f, cz);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * 2f * (float)Math.PI / segments;
                float h = i * 360f / segments;
                Color c = HsvToRgb(h, 1f, 1f);
                GL.Color3(c.R / 255f, c.G / 255f, c.B / 255f);
                GL.Vertex3(cx + radius * (float)Math.Cos(angle), 1f,
                           cz + radius * (float)Math.Sin(angle));
            }
            GL.End();
        }


        //عند العرض على الشاشة rgb عم نحول ل 
        private Color HsvToRgb(float h, float s, float v)
        {
            float r, g, b;
            if (s == 0) { r = g = b = v; }
            else
            {
                float hh = h / 60f;
                int i = (int)hh;
                float f = hh - i;
                float p = v * (1 - s);
                float q = v * (1 - s * f);
                float t = v * (1 - s * (1 - f));
                switch (i % 6)
                {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    default: r = v; g = p; b = q; break;
                }
            }
            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }


        //YCbCr 2d SPACE
        private void DrawYCbCrPlane()
        {
            int grid = 64;
            float step = 1f / grid;
            float Y = 0.5f; // luma ثابت

            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < grid; i++)
                for (int j = 0; j < grid; j++)
                {
                    float cb = i * step - 0.5f;
                    float cr = j * step - 0.5f;
                    Color c = YCbCrToRgb(Y, cb, cr);
                    GL.Color3(c.R / 255f, c.G / 255f, c.B / 255f);

                    GL.Vertex3(i * step, j * step, 0.5f);
                    GL.Vertex3((i + 1) * step, j * step, 0.5f);
                    GL.Vertex3((i + 1) * step, (j + 1) * step, 0.5f);
                    GL.Vertex3(i * step, (j + 1) * step, 0.5f);
                }
            GL.End();
        }

        private Color YCbCrToRgb(float Y, float Cb, float Cr)
        {
            int r = Clamp((int)((Y + 1.402f * Cr) * 255));
            int g = Clamp((int)((Y - 0.344f * Cb - 0.714f * Cr) * 255));
            int b = Clamp((int)((Y + 1.772f * Cb) * 255));
            return Color.FromArgb(r, g, b);
        }

        private int Clamp(int v) => Math.Max(0, Math.Min(255, v));


        //YUV 2d SPACE
        private void DrawYUVPlane()
        {
            int grid = 64;
            float step = 1f / grid;
            float Y = 0.5f;

            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < grid; i++)
                for (int j = 0; j < grid; j++)
                {
                    float U = i * step - 0.5f;
                    float V = j * step - 0.5f;
                    Color c = YuvToRgb(Y, U, V);
                    GL.Color3(c.R / 255f, c.G / 255f, c.B / 255f);
                    GL.Vertex3(i * step, j * step, 0.5f);
                    GL.Vertex3((i + 1) * step, j * step, 0.5f);
                    GL.Vertex3((i + 1) * step, (j + 1) * step, 0.5f);
                    GL.Vertex3(i * step, (j + 1) * step, 0.5f);
                }
            GL.End();
        }

        private Color YuvToRgb(float Y, float U, float V)
        {
            int r = Clamp((int)((Y + 1.13983f * V) * 255));
            int g = Clamp((int)((Y - 0.39465f * U - 0.58060f * V) * 255));
            int b = Clamp((int)((Y + 2.03211f * U) * 255));
            return Color.FromArgb(r, g, b);
        }


        // CMY CUBE SPACE
        private void DrawCMYCubeFaces()
        {
            GL.Begin(PrimitiveType.Quads);

            // Y=0
            GL.Color3(1f, 1f, 1f); GL.Vertex3(0, 0, 0); // White
            GL.Color3(0f, 1f, 1f); GL.Vertex3(1, 0, 0); // Cyan
            GL.Color3(0f, 0f, 1f); GL.Vertex3(1, 1, 0); // Blue
            GL.Color3(1f, 0f, 1f); GL.Vertex3(0, 1, 0); // Magenta

            // Y=1
            GL.Color3(1f, 1f, 0f); GL.Vertex3(0, 0, 1); // Yellow
            GL.Color3(0f, 1f, 0f); GL.Vertex3(1, 0, 1); // Green
            GL.Color3(0f, 0f, 0f); GL.Vertex3(1, 1, 1); // Black
            GL.Color3(1f, 0f, 0f); GL.Vertex3(0, 1, 1); // Red

            // C=0
            GL.Color3(1f, 1f, 1f); GL.Vertex3(0, 0, 0); // White
            GL.Color3(1f, 0f, 1f); GL.Vertex3(0, 1, 0); // Magenta
            GL.Color3(1f, 0f, 0f); GL.Vertex3(0, 1, 1); // Red
            GL.Color3(1f, 1f, 0f); GL.Vertex3(0, 0, 1); // Yellow

            // C=1
            GL.Color3(0f, 1f, 1f); GL.Vertex3(1, 0, 0); // Cyan
            GL.Color3(0f, 0f, 1f); GL.Vertex3(1, 1, 0); // Blue
            GL.Color3(0f, 0f, 0f); GL.Vertex3(1, 1, 1); // Black
            GL.Color3(0f, 1f, 0f); GL.Vertex3(1, 0, 1); // Green

            // M=0
            GL.Color3(1f, 1f, 1f); GL.Vertex3(0, 0, 0); // White
            GL.Color3(0f, 1f, 1f); GL.Vertex3(1, 0, 0); // Cyan
            GL.Color3(0f, 1f, 0f); GL.Vertex3(1, 0, 1); // Green
            GL.Color3(1f, 1f, 0f); GL.Vertex3(0, 0, 1); // Yellow

            // M=1
            GL.Color3(1f, 0f, 1f); GL.Vertex3(0, 1, 0); // Magenta
            GL.Color3(0f, 0f, 1f); GL.Vertex3(1, 1, 0); // Blue
            GL.Color3(0f, 0f, 0f); GL.Vertex3(1, 1, 1); // Black
            GL.Color3(1f, 0f, 0f); GL.Vertex3(0, 1, 1); // Red

            GL.End();
        }


        // LAB SPHERE SPACE
        private void DrawLABSphere()
        {
            int lat = 40, lon = 40;
            float radius = 0.5f;
            float cx = 0.5f, cy = 0.5f, cz = 0.5f;

            for (int i = 0; i < lat; i++)
            {
                float theta1 = i * (float)Math.PI / lat;
                float theta2 = (i + 1) * (float)Math.PI / lat;

                GL.Begin(PrimitiveType.TriangleStrip);
                for (int j = 0; j <= lon; j++)
                {
                    float phi = j * 2f * (float)Math.PI / lon;

                    for (int pass = 0; pass < 2; pass++)
                    {
                        float theta = pass == 0 ? theta1 : theta2;
                        float x = (float)(Math.Sin(theta) * Math.Cos(phi));
                        float y = (float)Math.Cos(theta);
                        float z = (float)(Math.Sin(theta) * Math.Sin(phi));

                        float L = (y + 1f) * 50f;
                        float a = x * 100f;
                        float b = z * 100f;

                        Color c = LabToRgb(L, a, b);
                        GL.Color3(c.R / 255f, c.G / 255f, c.B / 255f);
                        GL.Vertex3(cx + radius * x, cy + radius * y, cz + radius * z);
                    }
                }
                GL.End();
            }
        }

        private Color LabToRgb(float L, float a, float b)
        {
            float fy = (L + 16f) / 116f;
            float fx = a / 500f + fy;
            float fz = fy - b / 200f;

            float x = fx > 0.2069f ? fx * fx * fx : (fx - 16f / 116f) / 7.787f;
            float y = L > 8f ? (float)Math.Pow((L + 16) / 116.0, 3) : L / 903.3f;
            float z = fz > 0.2069f ? fz * fz * fz : (fz - 16f / 116f) / 7.787f;

            x *= 0.95047f; y *= 1.00000f; z *= 1.08883f;

            float r = 3.2406f * x - 1.5372f * y - 0.4986f * z;
            float g = -0.9689f * x + 1.8758f * y + 0.0415f * z;
            float bv = 0.0557f * x - 0.2040f * y + 1.0570f * z;

            r = r > 0.0031308f ? 1.055f * (float)Math.Pow(r, 1 / 2.4) - 0.055f : 12.92f * r;
            g = g > 0.0031308f ? 1.055f * (float)Math.Pow(g, 1 / 2.4) - 0.055f : 12.92f * g;
            bv = bv > 0.0031308f ? 1.055f * (float)Math.Pow(bv, 1 / 2.4) - 0.055f : 12.92f * bv;

            return Color.FromArgb(Clamp((int)(r * 255)), Clamp((int)(g * 255)), Clamp((int)(bv * 255)));
        }


        




        // BUTTONS

        private void btnRGB_Click(object sender, EventArgs e)
        {
            _currentSpace = "RGB";
            _spaceLoaded = true;
            _glControl.Invalidate();
        }

        private void btnHSV_Click(object sender, EventArgs e)
        {
            _currentSpace = "HSV";
            _spaceLoaded = true;
            _rotX = 20f; _rotY = -30f; // زاوية مناسبة للمخروط
            _glControl.Invalidate();
        }

        private void btnYCbCr_Click(object sender, EventArgs e)
        {
            _currentSpace = "YCBCR";
            _spaceLoaded = true;
            _rotX = 0f; _rotY = 0f; // نظرة مباشرة للمستوي
            _glControl.Invalidate();
        }

        private void btnYUV_Click(object sender, EventArgs e)
        {
            _currentSpace = "YUV";
            _spaceLoaded = true;
            _rotX = 0f; _rotY = 0f;
            _glControl.Invalidate();
        }

        private void btnCMY_Click(object sender, EventArgs e)
        {
            _currentSpace = "CMY";
            _spaceLoaded = true;
            _rotX = 25f; _rotY = -45f;
            _glControl.Invalidate();
        }

        private void btnLAB_Click(object sender, EventArgs e)
        {
            _currentSpace = "LAB";
            _spaceLoaded = true;
            _rotX = 20f; _rotY = -30f;
            _glControl.Invalidate();
        }

        private void ColorSpaceForm_Load(object sender, EventArgs e)
        {

        }

        
    }
}