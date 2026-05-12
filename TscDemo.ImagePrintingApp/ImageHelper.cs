using ImageMagick;
using System.IO;

namespace TscDemo.ImagePrintingApp
{
    internal class ImageHelper
    {
        MagickImage GetImageFromPdf(int density)
        {
            string fileName = "test.pdf";

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileName);

            byte[] pdfBytes = File.ReadAllBytes(filePath);

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

            return (MagickImage)images[0];
        }

        public byte[] ConvertPdfToPng()
        {
            var label = GetImageFromPdf(203);

            label.Format = MagickFormat.Png;

            return label.ToByteArray();
        }

        public byte[] ConvertPdfToBitmap()
        {
            var label = GetImageFromPdf(203);

            label.Format = MagickFormat.Bmp;

            //label.Quantize(new QuantizeSettings { Colors = 2 });
            label.Depth = 1;

            byte[] bmpBytes = label.ToByteArray();

            uint widthInBytes = (label.Width + 7) / 8;
            uint height = label.Height;
            byte[] tsplData = new byte[widthInBytes * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < label.Width; x++)
                {
                    var pixel = label.GetPixels().GetPixel(x, y);
                    var color = pixel.ToColor();

                    if (color == MagickColors.Black)
                    {
                        // 3. Set the corresponding bit in the byte array
                        // TSPL usually treats 1 as black, 0 as white (or vice versa depending on mode)
                        //int byteIndex = (y * widthInBytes) + (x / 8);
                        int bitIndex = 7 - (x % 8); // Most Significant Bit first
                        //tsplData[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }
            }

            return tsplData;
        }
    }
}
