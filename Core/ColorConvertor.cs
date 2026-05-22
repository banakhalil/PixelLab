using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace PixelLab.Core
{
    public static class ColorConvertor
    {
        public static Bitmap MatToBitmap(Mat mat)
        {
            if (mat == null || mat.IsEmpty)
                throw new ArgumentNullException(nameof(mat), "المصفوفة Mat فارغة ولا يمكن تحويلها.");

            using (Image<Bgr, byte> img = mat.ToImage<Bgr, byte>())
            {
                Bitmap bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                System.Runtime.InteropServices.Marshal.Copy(img.Bytes, 0, bmpData.Scan0, img.Bytes.Length);

                bmp.UnlockBits(bmpData);
                return bmp;
            }
        }

        public static Mat BitmapToMat(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), "الصورة Bitmap فارغة.");

            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            Mat mat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 3);

            int bytesCount = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytesCount];

            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytesCount);
            mat.SetTo(rgbValues);

            bitmap.UnlockBits(bmpData);
            return mat;
        }

        public static Bitmap ConvertBetweenAnySpaces(Bitmap srcBitmap, string fromSystem, string toSystem)
        {
            if (fromSystem.Equals(toSystem, StringComparison.OrdinalIgnoreCase))
            {
                return new Bitmap(srcBitmap);
            }

            Mat rgbBridge = new Mat();
            Mat srcImage = BitmapToMat(srcBitmap);

            switch (fromSystem.ToUpper())
            {
                case "RGB":
                    rgbBridge = srcImage;
                    break;
                case "CMY":
                    Bitmap cmyBmp = MatToBitmap(srcImage);
                    Bitmap rgbBmp = ConvertCMYToRGB(cmyBmp);
                    rgbBridge = BitmapToMat(rgbBmp);
                    break;
                case "HSV":
                    CvInvoke.CvtColor(srcImage, rgbBridge, ColorConversion.Hsv2Bgr);
                    break;
                case "YCBCR":
                    CvInvoke.CvtColor(srcImage, rgbBridge, ColorConversion.YCrCb2Bgr);
                    break;
                case "YUV":
                    CvInvoke.CvtColor(srcImage, rgbBridge, ColorConversion.Yuv2Bgr);
                    break;
                case "LAB":
                    CvInvoke.CvtColor(srcImage, rgbBridge, ColorConversion.Lab2Bgr);
                    break;
                default:
                    throw new ArgumentException($"النظام المصدر {fromSystem} غير مدعوم حالياً.");
            }

            Mat finalResult = new Mat();

            switch (toSystem.ToUpper())
            {
                case "RGB":
                    finalResult = rgbBridge;
                    break;
                case "CMY":
                    Bitmap finalRgbBmp = MatToBitmap(rgbBridge);
                    Bitmap finalCmyBmp = ConvertRGBToCMY(finalRgbBmp);
                    finalResult = BitmapToMat(finalCmyBmp);
                    break;
                case "HSV":
                    CvInvoke.CvtColor(rgbBridge, finalResult, ColorConversion.Bgr2Hsv);
                    break;
                case "YCBCR":
                    CvInvoke.CvtColor(rgbBridge, finalResult, ColorConversion.Bgr2YCrCb);
                    break;
                case "YUV":
                    CvInvoke.CvtColor(rgbBridge, finalResult, ColorConversion.Bgr2Yuv);
                    break;
                case "LAB":
                    CvInvoke.CvtColor(rgbBridge, finalResult, ColorConversion.Bgr2Lab);
                    break;
                default:
                    throw new ArgumentException($"النظام المستهدف {toSystem} غير مدعوم حالياً.");
            }

            return MatToBitmap(finalResult);
        }

        public static Bitmap ConvertToCMY(Bitmap rgbBitmap)
        {
            return ConvertRGBToCMY(rgbBitmap);
        }

        public static Bitmap ConvertRGBToCMY(Bitmap rgbBitmap)
        {
            Bitmap cmyBitmap = new Bitmap(rgbBitmap.Width, rgbBitmap.Height, PixelFormat.Format24bppRgb);
            BitmapData srcData = rgbBitmap.LockBits(new Rectangle(0, 0, rgbBitmap.Width, rgbBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData destData = cmyBitmap.LockBits(new Rectangle(0, 0, cmyBitmap.Width, cmyBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(srcData.Stride) * rgbBitmap.Height;
            byte[] rgbValues = new byte[bytes];
            byte[] cmyValues = new byte[bytes];

            Marshal.Copy(srcData.Scan0, rgbValues, 0, bytes);
            for (int i = 0; i < rgbValues.Length; i++)
            {
                cmyValues[i] = (byte)(255 - rgbValues[i]); 
            }
            Marshal.Copy(cmyValues, 0, destData.Scan0, bytes);

            rgbBitmap.UnlockBits(srcData);
            cmyBitmap.UnlockBits(destData);

            return cmyBitmap;
        }

        public static Bitmap ConvertCMYToRGB(Bitmap cmyBitmap)
        {
            return ConvertRGBToCMY(cmyBitmap); // 
        }

        public static Bitmap ConvertToHSV(Bitmap rgbBitmap)
        {
            Mat rgbImage = BitmapToMat(rgbBitmap);
            Mat hsvImage = new Mat();
            CvInvoke.CvtColor(rgbImage, hsvImage, ColorConversion.Bgr2Hsv);
            return MatToBitmap(hsvImage);
        }

        public static Bitmap ConvertHSVToRGB(Bitmap hsvBitmap)
        {
            Mat hsvImage = BitmapToMat(hsvBitmap);
            Mat rgbImage = new Mat();
            CvInvoke.CvtColor(hsvImage, rgbImage, ColorConversion.Hsv2Bgr);
            return MatToBitmap(rgbImage);
        }

        public static Bitmap ConvertToYCbCr(Bitmap rgbBitmap)
        {
            Mat rgbImage = BitmapToMat(rgbBitmap);
            Mat ycbcrImage = new Mat();
            CvInvoke.CvtColor(rgbImage, ycbcrImage, ColorConversion.Bgr2YCrCb);
            return MatToBitmap(ycbcrImage);
        }

        public static Bitmap ConvertYCbCrToRGB(Bitmap ycbcrBitmap)
        {
            Mat ycbcrImage = BitmapToMat(ycbcrBitmap);
            Mat rgbImage = new Mat();
            CvInvoke.CvtColor(ycbcrImage, rgbImage, ColorConversion.YCrCb2Bgr);
            return MatToBitmap(rgbImage);
        }

        public static Bitmap ConvertToYUV(Bitmap rgbBitmap)
        {
            Mat rgbImage = BitmapToMat(rgbBitmap);
            Mat yuvImage = new Mat();
            CvInvoke.CvtColor(rgbImage, yuvImage, ColorConversion.Bgr2Yuv);
            return MatToBitmap(yuvImage);
        }

        public static Bitmap ConvertYUVToRGB(Bitmap yuvBitmap)
        {
            Mat yuvImage = BitmapToMat(yuvBitmap);
            Mat rgbImage = new Mat();
            CvInvoke.CvtColor(yuvImage, rgbImage, ColorConversion.Yuv2Bgr);
            return MatToBitmap(rgbImage);
        }

        public static Bitmap ConvertToLAB(Bitmap rgbBitmap)
        {
            Mat rgbImage = BitmapToMat(rgbBitmap);
            Mat labImage = new Mat();
            CvInvoke.CvtColor(rgbImage, labImage, ColorConversion.Bgr2Lab);
            return MatToBitmap(labImage);
        }

        public static Bitmap ConvertLABToRGB(Bitmap labBitmap)
        {
            Mat labImage = BitmapToMat(labBitmap);
            Mat rgbImage = new Mat();
            CvInvoke.CvtColor(labImage, rgbImage, ColorConversion.Lab2Bgr);
            return MatToBitmap(rgbImage);
        }
    }
}