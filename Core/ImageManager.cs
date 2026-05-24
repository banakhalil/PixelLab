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

        //public int NumberOfChannels
        //{
        //    get
        //    {
        //        if (CurrentImage == null) return 0;

        //        switch (CurrentImage.PixelFormat)
        //        {
        //            case PixelFormat.Format24bppRgb:
        //            case PixelFormat.Format32bppRgb:
        //                return 3;
        //            case PixelFormat.Format32bppArgb:
        //            case PixelFormat.Format32bppPArgb:
        //                return 4;
        //            case PixelFormat.Format8bppIndexed:
        //                return 1;
        //            default:
        //                return 3;
        //        }
        //    }
        //}

        public void Reset()
        {
            CurrentImage = new Bitmap(OriginalImage);
            CurrentColorSystem = "RGB";
        }
    }
}
