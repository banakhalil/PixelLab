using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace PixelLab.Core
{
    public class ImageManager
    {
        public Bitmap CurrentImage { get; private set; }
        public Bitmap OriginalImage { get; private set; }
        public string ImagePath { get; private set; }
        public string ImageName => Path.GetFileName(ImagePath);
        public string ImageFormat => Path.GetExtension(ImagePath).ToUpper();
        public long ImageSize => new FileInfo(ImagePath).Length;
        public string CurrentColorSystem { get; set; } = "RGB";

        // الأنواع المسموح بها
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff" };

        public bool LoadImage(string path)
        {
            string ext = Path.GetExtension(path).ToLower();

            if (Array.IndexOf(AllowedExtensions, ext) == -1)
                return false;   // نوع غير مدعوم

            CurrentImage = new Bitmap(path);
            OriginalImage = new Bitmap(path);
            ImagePath = path;
            return true;
        }

        public void Reset()
        {
            CurrentImage = new Bitmap(OriginalImage);
            CurrentColorSystem = "RGB";
        }
    }
}
