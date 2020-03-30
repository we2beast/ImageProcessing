using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageProcessing
{
    class MainClass
    {
        private static Bitmap _srcBmp;
        private static Bitmap _destBmp;
        private static readonly int[] Histogram = new int[256];
        private static int _thr;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a path.");
                return;
            }

            string pathToImage = args[0];
            if (!File.Exists(pathToImage))
            {
                Console.WriteLine("Please enter a correct path to image.");
                return;
            }

            Bitmap imageBitmap = ConvertToBitmap(pathToImage);

            if (IsGrayBitmap(imageBitmap))
                _srcBmp = imageBitmap;
            else
            {
                _srcBmp = ConvertToGrayBitmap(imageBitmap);
                imageBitmap.Dispose();
            }

            _destBmp = CreateGrayBitmap(_srcBmp.Width, _srcBmp.Height);
            GetHistGram(_srcBmp, Histogram);
            Update();
        }

        private static Bitmap ConvertToBitmap(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = File.Open(fileName, FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);
            }

            return bitmap;
        }

        private static Bitmap CreateGrayBitmap(int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette pal = bmp.Palette;
            for (int y = 0; y < pal.Entries.Length; y++) pal.Entries[y] = Color.FromArgb(255, y, y, y);
            bmp.Palette = pal;
            return bmp;
        }

        private static bool IsGrayBitmap(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format8bppIndexed) return false;
            if (bmp.Palette.Entries.Length != 256) return false;
            for (int y = 0; y < bmp.Palette.Entries.Length; y++)
                if (bmp.Palette.Entries[y] != Color.FromArgb(255, y, y, y))
                    return false;
            return true;
        }
        
        private static Bitmap ConvertToGrayBitmap(Bitmap src)
        {
            unsafe
            {
                Bitmap dest = CreateGrayBitmap(src.Width, src.Height);
                BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                BitmapData destData = dest.LockBits(new Rectangle(0, 0, dest.Width, dest.Height), ImageLockMode.ReadWrite, dest.PixelFormat);
                int width = srcData.Width, height = srcData.Height;
                int srcStride = srcData.Stride, destStride = destData.Stride;
                for (int y = 0; y < height; y++)
                {
                    var srcP = (byte*) srcData.Scan0 + y * srcStride;
                    var destP = (byte*) destData.Scan0 + y * destStride;
                    for (int x = 0; x < width; x++)
                    {
                        *destP = (byte) ((*srcP + (*(srcP + 1) << 1) + *(srcP + 3)) >> 2);
                        srcP += 3;
                        destP++;
                    }
                }

                src.UnlockBits(srcData);
                dest.UnlockBits(destData);
                return dest;
            }
        }

        private static void GetHistGram(Bitmap src, int[] histogram)
        {
            unsafe
            {
                BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadWrite,
                    src.PixelFormat);
                int width = srcData.Width, height = srcData.Height, srcStride = srcData.Stride;
                byte* srcP;
                for (int y = 0; y < 256; y++) histogram[y] = 0;
                for (int y = 0; y < height; y++)
                {
                    srcP = (byte*) srcData.Scan0 + y * srcStride;
                    for (int x = 0; x < width; x++, srcP++) histogram[*srcP]++;
                }

                src.UnlockBits(srcData);
            }
        }

        private static void Update()
        {
            _thr = Threshold.GetYenThreshold(Histogram);
            Console.WriteLine(Threshold.GetMeanThreshold(Histogram));
            Console.WriteLine(Threshold.GetOTSUThreshold(Histogram));
            Console.WriteLine(Threshold.GetYenThreshold(Histogram));

            DoBinarization(_srcBmp, _destBmp, _thr);

            _srcBmp.Save("complete.png", ImageFormat.Png);
            _destBmp.Save("complete2.png", ImageFormat.Png);
        }

        private static void DoBinarization(Bitmap src, Bitmap dest, int threshold)
        {
            unsafe
            {
                BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadWrite,
                    src.PixelFormat);
                BitmapData destData = dest.LockBits(new Rectangle(0, 0, dest.Width, dest.Height),
                    ImageLockMode.ReadWrite, dest.PixelFormat);
                int width = srcData.Width, height = srcData.Height;
                int srcStride = srcData.Stride, destStride = destData.Stride;
                byte* SrcP, DestP;
                for (int y = 0; y < height; y++)
                {
                    SrcP = (byte*) srcData.Scan0 + y * srcStride;
                    DestP = (byte*) destData.Scan0 + y * destStride;
                    for (int x = 0; x < width; x++, SrcP++, DestP++)
                        *DestP = *SrcP > threshold ? byte.MaxValue : byte.MinValue;
                }

                src.UnlockBits(srcData);
                dest.UnlockBits(destData);
            }
        }
    }
}