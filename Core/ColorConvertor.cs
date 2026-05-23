using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util; 

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
                Bitmap bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);

                BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                Marshal.Copy(img.Bytes, 0, bmpData.Scan0, img.Bytes.Length);

                bmp.UnlockBits(bmpData);
                return bmp;
            }
        }

        public static Mat BitmapToMat(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), "الصورة Bitmap فارغة.");

            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            Mat mat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 3);

            int bytesCount = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytesCount];

            Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytesCount);
            mat.SetTo(rgbValues);

            bitmap.UnlockBits(bmpData);
            return mat;
        }


        public static Bitmap ConvertBetweenAnySpaces(Bitmap srcBitmap, string fromSystem, string toSystem,
            int ch1Val, int ch2Val, int ch3Val, int ch4Val,
            bool ch1Active, bool ch2Active, bool ch3Active, bool ch4Active)
        {
            if (srcBitmap == null) throw new ArgumentNullException(nameof(srcBitmap));

            Mat rgbBridge = new Mat();
            Mat srcImage = BitmapToMat(srcBitmap);

            switch (fromSystem.ToUpper())
            {
                case "RGB":
                    rgbBridge = srcImage.Clone(); //
                    break;
                case "CMY":
                    Bitmap cmyBmp = MatToBitmap(srcImage);
                    Bitmap rgbBmp = ConvertCMYToRGB(cmyBmp);
                    rgbBridge = BitmapToMat(rgbBmp);
                    break;
                case "CMYK":
                    Bitmap rgbFromCmyk = ConvertCMYKToRGB(srcBitmap);
                    rgbBridge = BitmapToMat(rgbFromCmyk);
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
                    finalResult = rgbBridge.Clone();
                    break;
                case "CMY":
                    Bitmap finalRgbBmp = MatToBitmap(rgbBridge);
                    Bitmap finalCmyBmp = ConvertRGBToCMY(finalRgbBmp);
                    finalResult = BitmapToMat(finalCmyBmp);
                    break;
                case "CMYK":
                    Bitmap finalRgb = MatToBitmap(rgbBridge);
                    finalResult = ConvertRGBToCMYKMat(finalRgb);
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

            Mat processedMat = ProcessChannelsAdvanced(finalResult, toSystem,
                ch1Val, ch2Val, ch3Val, ch4Val,
                ch1Active, ch2Active, ch3Active, ch4Active);

            if (toSystem.ToUpper() == "CMYK")
            {
                Bitmap cmykDisplay = ConvertCMYKMatToBitmap(processedMat);

                srcImage.Dispose(); rgbBridge.Dispose(); finalResult.Dispose(); processedMat.Dispose();
                return cmykDisplay;
            }
            else
            {
                Bitmap resultBitmap = MatToBitmap(processedMat);

                // تنظيف الذاكرة
                srcImage.Dispose(); rgbBridge.Dispose(); finalResult.Dispose(); processedMat.Dispose();
                return resultBitmap;
            }
        }

        public static Mat ProcessChannelsAdvanced(Mat srcMat, string system,
            int ch1Val, int ch2Val, int ch3Val, int ch4Val,
            bool ch1Active, bool ch2Active, bool ch3Active, bool ch4Active)
        {
            if (system.ToUpper() == "CMYK")
            {
                VectorOfMat cmykChannels = new VectorOfMat();
                CvInvoke.Split(srcMat, cmykChannels);

                if (cmykChannels.Size == 4)
                {
                    Mat c = cmykChannels[0]; // Cyan
                    Mat m = cmykChannels[1]; // Magenta
                    Mat y = cmykChannels[2]; // Yellow
                    Mat k = cmykChannels[3]; // Key (Black)

                    ApplyModification(c, ch1Val, ch1Active);
                    ApplyModification(m, ch2Val, ch2Active);
                    ApplyModification(y, ch3Val, ch3Active);
                    ApplyModification(k, ch4Val, ch4Active);

                    Mat resultCmyk = new Mat();
                    using (VectorOfMat merged = new VectorOfMat(c, m, y, k))
                    {
                        CvInvoke.Merge(merged, resultCmyk);
                    }
                    cmykChannels.Dispose();
                    return resultCmyk;
                }
                cmykChannels.Dispose();
            }

            // --- باقي الأنظمة اللونية (3 قنوات: RGB, HSV, YUV, LAB, YCbCr, CMY) ---
            VectorOfMat channels = new VectorOfMat();
            CvInvoke.Split(srcMat, channels);

            Mat c1 = channels[0];
            Mat c2 = channels[1];
            Mat c3 = channels[2];

            Mat targetCh1 = null, targetCh2 = null, targetCh3 = null;

            switch (system.ToUpper())
            {
                case "RGB":
                    targetCh1 = c3; // R
                    targetCh2 = c2; // G
                    targetCh3 = c1; // B
                    break;
                case "CMY":
                    targetCh1 = c3; // C
                    targetCh2 = c2; // M
                    targetCh3 = c1; // Y
                    break;
                case "HSV":
                    targetCh1 = c1; // H
                    targetCh2 = c2; // S
                    targetCh3 = c3; // V
                    break;
                case "YUV":
                    targetCh1 = c1; // Y
                    targetCh2 = c2; // U
                    targetCh3 = c3; // V
                    break;
                case "LAB":
                    targetCh1 = c1; // L
                    targetCh2 = c2; // A
                    targetCh3 = c3; // B
                    break;
                case "YCBCR":
                    targetCh1 = c1; // Y
                    targetCh2 = c3; // Cb
                    targetCh3 = c2; // Cr
                    break;
                default:
                    targetCh1 = c1; targetCh2 = c2; targetCh3 = c3;
                    break;
            }

            ApplyModification(targetCh1, ch1Val, ch1Active);
            ApplyModification(targetCh2, ch2Val, ch2Active);
            ApplyModification(targetCh3, ch3Val, ch3Active);

            Mat resultMat = new Mat();
            using (VectorOfMat mergedChannels = new VectorOfMat(c1, c2, c3))
            {
                CvInvoke.Merge(mergedChannels, resultMat);
            }

            channels.Dispose();
            return resultMat;
        }

        private static void ApplyModification(Mat channel, int value, bool isActive)
        {
            if (channel == null) return;

            if (!isActive)
                channel.SetTo(new MCvScalar(0));
            else if (value != 0)
                CvInvoke.Add(channel, new Emgu.CV.ScalarArray(value), channel);
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
            return ConvertRGBToCMY(cmyBitmap);
        }

        private static Mat ConvertRGBToYKMat(Bitmap rgbBitmap) //
        {
            int w = rgbBitmap.Width;
            int h = rgbBitmap.Height;
            Mat cmykMat = new Mat(h, w, DepthType.Cv8U, 4); // 

            BitmapData srcData = rgbBitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int bytes = Math.Abs(srcData.Stride) * h;
            byte[] bgr = new byte[bytes];
            Marshal.Copy(srcData.Scan0, bgr, 0, bytes);
            rgbBitmap.UnlockBits(srcData);

            byte[] cmykData = new byte[w * h * 4];
            int cmykIdx = 0;
            int stride = Math.Abs(srcData.Stride);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int bgrIdx = (y * stride) + (x * 3);
                    double b = bgr[bgrIdx] / 255.0;
                    double g = bgr[bgrIdx + 1] / 255.0;
                    double r = bgr[bgrIdx + 2] / 255.0;

                    double k = 1.0 - Math.Max(r, Math.Max(g, b));
                    double c = (k == 1.0) ? 0 : (1.0 - r - k) / (1.0 - k);
                    double m = (k == 1.0) ? 0 : (1.0 - g - k) / (1.0 - k);
                    double yCol = (k == 1.0) ? 0 : (1.0 - b - k) / (1.0 - k);

                    cmykData[cmykIdx] = (byte)(c * 255);
                    cmykData[cmykIdx + 1] = (byte)(m * 255);
                    cmykData[cmykIdx + 2] = (byte)(yCol * 255);
                    cmykData[cmykIdx + 3] = (byte)(k * 255);
                    cmykIdx += 4;
                }
            }

            cmykMat.SetTo(cmykData);
            return cmykMat;
        }

        private static Bitmap ConvertCMYKMatToBitmap(Mat cmykMat)
        {
            int w = cmykMat.Width;
            int h = cmykMat.Height;
            Bitmap rgbBmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData destData = rgbBmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            byte[] cmykData = new byte[w * h * 4];
            cmykMat.CopyTo(cmykData);

            int stride = Math.Abs(destData.Stride);
            byte[] bgr = new byte[stride * h];

            int cmykIdx = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double c = cmykData[cmykIdx] / 255.0;
                    double m = cmykData[cmykIdx + 1] / 255.0;
                    double yCol = cmykData[cmykIdx + 2] / 255.0;
                    double k = cmykData[cmykIdx + 3] / 255.0;

                    byte r = (byte)(255 * (1.0 - c) * (1.0 - k));
                    byte g = (byte)(255 * (1.0 - m) * (1.0 - k));
                    byte b = (byte)(255 * (1.0 - yCol) * (1.0 - k));

                    int bgrIdx = (y * stride) + (x * 3);
                    bgr[bgrIdx] = b;
                    bgr[bgrIdx + 1] = g;
                    bgr[bgrIdx + 2] = r;

                    cmykIdx += 4;
                }
            }

            Marshal.Copy(bgr, 0, destData.Scan0, bgr.Length);
            rgbBmp.UnlockBits(destData);
            return rgbBmp;
        }

        private static Bitmap ConvertCMYKToRGB(Bitmap cmykBitmap)
        {
            Mat tempCmyk = ConvertRGBToYKMat(cmykBitmap);
            Bitmap rgb = ConvertCMYKMatToBitmap(tempCmyk);
            tempCmyk.Dispose();
            return rgb;
        }

        private static Mat ConvertRGBToCMYKMat(Bitmap rgbBitmap)
        {
            return ConvertRGBToYKMat(rgbBitmap);
        }
    }
}