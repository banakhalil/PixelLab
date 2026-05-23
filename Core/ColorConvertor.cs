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
        public static unsafe Bitmap MatToBitmap(Mat mat)
        {
            if (mat == null || mat.IsEmpty)
                throw new ArgumentNullException(nameof(mat), "المصفوفة Mat فارغة ولا يمكن تحويلها.");

            // يدعم التحويل المباشر لـ 3 قنوات (BGR) أو قناة واحدة (Grayscale)
            PixelFormat format = mat.NumberOfChannels == 1 ? PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(mat.Width, mat.Height, format);

            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);

            // نسخ البيانات مباشرة من الـ Pointer لتفادي الحشو وبأعلى سرعة
            long imageSize = (long)mat.Height * mat.Step;
            Buffer.MemoryCopy((void*)mat.DataPointer, (void*)bmpData.Scan0, imageSize, imageSize);

            bmp.UnlockBits(bmpData);

            // إذا كانت الصورة رمادية، يجب إعداد لوحة الألوان (Palette)
            if (format == PixelFormat.Format8bppIndexed)
            {
                ColorPalette palette = bmp.Palette;
                for (int i = 0; i < 256; i++) palette.Entries[i] = Color.FromArgb(i, i, i);
                bmp.Palette = palette;
            }

            return bmp;
        }

        public static unsafe Mat BitmapToMat(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), "الصورة Bitmap فارغة.");

            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            // إنشاء مات مستمرة بأبعاد بكسل صافية
            Mat mat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 3);

            int stride = Math.Abs(bmpData.Stride);
            int matStride = bitmap.Width * 3;

            // نسخ السطور سطراً بسطر لضمان التخلص من الـ Padding (الحشوة)
            for (int y = 0; y < bitmap.Height; y++)
            {
                IntPtr srcRow = IntPtr.Add(bmpData.Scan0, y * stride);
                IntPtr destRow = IntPtr.Add(mat.DataPointer, y * matStride);
                Buffer.MemoryCopy((void*)srcRow, (void*)destRow, matStride, matStride);
            }

            bitmap.UnlockBits(bmpData);
            return mat;
        }

        public static Bitmap ConvertBetweenAnySpaces(Bitmap srcBitmap, string fromSystem, string toSystem,
            int ch1Val, int ch2Val, int ch3Val, int ch4Val,
            bool ch1Active, bool ch2Active, bool ch3Active, bool ch4Active)
        {
            if (srcBitmap == null) throw new ArgumentNullException(nameof(srcBitmap));

            Mat srcImage = BitmapToMat(srcBitmap);
            Mat rgbBridge = new Mat();

            switch (fromSystem.ToUpper())
            {
                case "RGB":
                    rgbBridge = srcImage.Clone();
                    break;
                case "CMY":
                    using (Bitmap cmyBmp = MatToBitmap(srcImage))
                    using (Bitmap rgbBmp = ConvertCMYToRGB(cmyBmp))
                        rgbBridge = BitmapToMat(rgbBmp);
                    break;
                case "CMYK":
                    using (Bitmap rgbFromCmyk = ConvertCMYKToRGB(srcBitmap))
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
                    srcImage.Dispose(); rgbBridge.Dispose();
                    throw new ArgumentException($"النظام المصدر {fromSystem} غير مدعوم حالياً.");
            }

            Mat finalResult = new Mat();
            switch (toSystem.ToUpper())
            {
                case "RGB":
                    finalResult = rgbBridge.Clone();
                    break;
                case "CMY":
                    using (Bitmap finalRgbBmp = MatToBitmap(rgbBridge))
                    using (Bitmap finalCmyBmp = ConvertRGBToCMY(finalRgbBmp))
                        finalResult = BitmapToMat(finalCmyBmp);
                    break;
                case "CMYK":
                    using (Bitmap finalRgb = MatToBitmap(rgbBridge))
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
                    srcImage.Dispose(); rgbBridge.Dispose(); finalResult.Dispose();
                    throw new ArgumentException($"النظام المستهدف {toSystem} غير مدعوم حالياً.");
            }

            Mat processedMat = ProcessChannelsAdvanced(finalResult, toSystem,
                ch1Val, ch2Val, ch3Val, ch4Val,
                ch1Active, ch2Active, ch3Active, ch4Active);

            Bitmap resultBitmap;
            if (toSystem.ToUpper() == "CMYK")
            {
                resultBitmap = ConvertCMYKMatToBitmap(processedMat);
            }
            else
            {
                resultBitmap = MatToBitmap(processedMat);
            }

            // تنظيف صارم ومثالي للذاكرة لمنع تسريب الـ RAM
            srcImage.Dispose();
            rgbBridge.Dispose();
            finalResult.Dispose();
            processedMat.Dispose();

            return resultBitmap;
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
                    Mat k = cmykChannels[3]; // Key

                    ApplyModification(c, ch1Val, ch1Active);
                    ApplyModification(m, ch2Val, ch2Active);
                    ApplyModification(y, ch3Val, ch3Active);
                    ApplyModification(k, ch4Val, ch4Active);

                    Mat resultCmyk = new Mat();
                    using (VectorOfMat merged = new VectorOfMat(c, m, y, k))
                    {
                        CvInvoke.Merge(merged, resultCmyk);
                    }

                    c.Dispose(); m.Dispose(); y.Dispose(); k.Dispose();
                    cmykChannels.Dispose();
                    return resultCmyk;
                }
                cmykChannels.Dispose();
            }

            // لـ 3 قنوات: تذكر أن الترتيب الافتراضي في OpenCV هو BGR
            VectorOfMat channels = new VectorOfMat();
            CvInvoke.Split(srcMat, channels);

            Mat bCh = channels[0];
            Mat gCh = channels[1];
            Mat rCh = channels[2];

            Mat targetCh1 = null, targetCh2 = null, targetCh3 = null;

            switch (system.ToUpper())
            {
                case "RGB":
                    targetCh1 = rCh; // Channel 1: Red
                    targetCh2 = gCh; // Channel 2: Green
                    targetCh3 = bCh; // Channel 3: Blue
                    break;
                case "CMY":
                    targetCh1 = rCh; // C
                    targetCh2 = gCh; // M
                    targetCh3 = bCh; // Y
                    break;
                case "HSV":
                case "YUV":
                case "LAB":
                    targetCh1 = bCh; // أول قناة بالنظام (H أو Y أو L)
                    targetCh2 = gCh; // ثاني قناة (S أو U أو A)
                    targetCh3 = rCh; // ثالث قناة (V أو V أو B)
                    break;
                case "YCBCR":
                    targetCh1 = bCh; // Y
                    targetCh2 = rCh; // Cb (تعديل للترتيب الصحيح)
                    targetCh3 = gCh; // Cr
                    break;
                default:
                    targetCh1 = bCh; targetCh2 = gCh; targetCh3 = rCh;
                    break;
            }

            ApplyModification(targetCh1, ch1Val, ch1Active);
            ApplyModification(targetCh2, ch2Val, ch2Active);
            ApplyModification(targetCh3, ch3Val, ch3Active);

            Mat resultMat = new Mat();
            using (VectorOfMat mergedChannels = new VectorOfMat(bCh, gCh, rCh))
            {
                CvInvoke.Merge(mergedChannels, resultMat);
            }

            bCh.Dispose(); gCh.Dispose(); rCh.Dispose();
            channels.Dispose();
            return resultMat;
        }

        private static void ApplyModification(Mat channel, int value, bool isActive)
        {
            if (channel == null) return;

            if (!isActive)
                channel.SetTo(new MCvScalar(0));
            // تجنب استدعاء AddScalar غير الموجود في بعض نسخ المكتبة واستخدام كائن ScalarArray العام
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

        private static Mat ConvertRGBToCMYKMat(Bitmap rgbBitmap)
        {
            int w = rgbBitmap.Width;
            int h = rgbBitmap.Height;
            Mat cmykMat = new Mat(h, w, DepthType.Cv8U, 4);

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
            using (Mat tempCmyk = ConvertRGBToCMYKMat(cmykBitmap))
            {
                return ConvertCMYKMatToBitmap(tempCmyk);
            }
        }
    }
}