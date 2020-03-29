using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageProcessing
{
    class MainClass
    {
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
            Bitmap completeImage = new ImageProcessing().Processing(imageBitmap);
            
            completeImage.Save("complete.png", ImageFormat.Png);
        }
        
        private static Bitmap ConvertToBitmap(string fileName)
        {
            Bitmap bitmap;
            using(Stream bmpStream = File.Open(fileName, FileMode.Open ))
            {
                Image image = Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);

            }
            return bitmap;
        }
    }
}
