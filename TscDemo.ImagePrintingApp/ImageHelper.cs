using ImageMagick;
using System.Collections;

namespace TscDemo.ImagePrintingApp
{
    internal class ImageHelper
    {
        static byte[] GetBmpBytes(byte[] pdfBytes, int density)
        {
            using MagickImageCollection images = [];

            // High density is essential for PDF to Image conversion
            var readSettings = new MagickReadSettings
            {
                Density = new Density(density),

                /// Таким образом получаемый PNG не будет иметь прозрачности,
                /// а PDF файл станет меньше
                UseMonochrome = true
            };

            images.Read(pdfBytes, readSettings);

            var label = (MagickImage)images[0];
            label.Format = MagickFormat.Bmp;
            //label.Quantize(new QuantizeSettings { Colors = 2 });
            label.Depth = 1;

            return label.ToByteArray();
        }

        //public static byte[] ConvertPdfToPng(byte[] pdfBytes)
        //{
        //    var label = GetImageFromPdf(pdfBytes, 203);

        //    label.Format = MagickFormat.Png;

        //    return label.ToByteArray();
        //}

        public static string ConvertPdfToBitmapHexString(byte[] pdfBytes, int density)
        {
            using MagickImageCollection images = [];

            // High density is essential for PDF to Image conversion
            var readSettings = new MagickReadSettings
            {
                Density = new Density(density),

                /// Таким образом получаемый PNG не будет иметь прозрачности,
                /// а PDF файл станет меньше
                UseMonochrome = true
            };

            images.Read(pdfBytes, readSettings);

            var label = (MagickImage)images[0];
            label.Format = MagickFormat.Bmp;
            //label.Quantize(new QuantizeSettings { Colors = 2 });
            label.Depth = 1;

            //var guid = Guid.NewGuid();
            //string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            //string bmpPath = Path.Combine(folder, $"{guid}.bmp");
            //label.Write(bmpPath);

            var width = (int)label.Width;
            var height = (int)label.Height;
            //int widthInBytes = (width + 7) / 8;

            var dots = new BitArray(width * height);
            var pixels = label.GetPixels();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < label.Width; x++)
                {
                    var pixel = pixels.GetPixel(x, y);
                    var color = pixel.ToColor();

                    //int byteIndex = (y * widthInBytes) + (x / 8);
                    int bitIndex = y * width + x; // 7 - (x % 8); // Most Significant Bit first

                    // Fill bit array (1 bit = 1 dot). False - black, true - white.
                    if (color.ToString() == "#FFFFFFFF")
                    {
                        dots[bitIndex] = true;
                    }
                    else
                    {
                        // NOTH
                    }
                }
            }

            // Convert bits to byte array
            int numBytes = (dots.Length + 7) / 8;
            byte[] bytes = new byte[numBytes];
            dots.CopyTo(bytes, 0);

            // Перед первым hex-кодом отсутствует дефис
            //return "\\x" + BitConverter.ToString(bytes).Replace("-", "\\x");
            return BitConverter.ToString(bytes);
        }
    }
}
