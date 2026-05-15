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

        //public static Label GetLabelFromBitmap(byte[] bitmapBytes, int density)
        //{
        //    using MagickImageCollection images = [];

        //    // High density is essential for PDF to Image conversion
        //    var readSettings = new MagickReadSettings
        //    {
        //        Density = new Density(density),

        //        /// Таким образом получаемый PNG не будет иметь прозрачности,
        //        /// а PDF файл станет меньше
        //        UseMonochrome = true
        //    };

        //    images.Read(bitmapBytes, readSettings);

        //    var label = (MagickImage)images[0];
        //    label.Format = MagickFormat.Bmp;
        //    //label.Quantize(new QuantizeSettings { Colors = 2 });
        //    label.Depth = 1;
        //}

        public static byte ReverseBits(byte b)
        {
            return (byte)((b * 0x0202020202 & 0x010884422010) % 1023);
            // Или более понятный способ через цикл:
            /*
            byte reversed = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((b & (1 << i)) != 0)
                    reversed |= (byte)(1 << (7 - i));
            }
            return reversed;
            */
        }

        public static BitmapElement GetLabel(byte[] imgBytes, int density)
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

            images.Read(imgBytes, readSettings);

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

            /// Через остаток от деления мы вычисляем кол-во белых точек, которые необходимо
            /// добавить к каждой строке, чтобы ширина матрицы была четной числу битов в байте,
            /// то есть восьми
            int offset = width % 8;
            int specialWidth = width - offset + 8;

            /// Ширина битовой матрицы будет больше реального изображения на величину оффсета,
            /// поскольку команда BITMAP в TSPL2 понимает только четные значения. Здесь также
            /// следует обратить внимание, что битовая коллеция по дефолту заполняется true
            /// значениями, т.к. они не пропечатываются.
            var dots = new BitArray(specialWidth * height, true);
            var pixels = label.GetPixels();

            for (int y = 0; y < height; y++)
            {
                /// Цикл проходит по реальной ширине изображения
                for (int x = 0; x < specialWidth; x++)
                {
                    int bitIndex = y * specialWidth + x;

                    /// Пропускаем, если вышил за пределы оригинального изображения
                    /// (пиксели останутся белыми)
                    if (x > width - 1)
                    {
                        dots[bitIndex] = true;
                        continue;
                    }

                    var pixel = pixels.GetPixel(x, y);
                    var color = pixel.ToColor();

                    // Fill bit array (1 bit = 1 dot). False - black, true - white.
                    if (color.ToString() != "#FFFFFFFF")
                    {
                        dots[bitIndex] = false;
                    }
                }
            }

            /// Ширина изображения уже приведена к четности 8, поэтому выражение numBytes = (dots.Length + 7) / 8 избыточно
            byte[] bytes = new byte[dots.Length / 8];
            dots.CopyTo(bytes, 0);

            /// Выполняем инверсию битов в каждом байте, чтобы их порядок соответствовал точкам изображения
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = ReverseBits(bytes[i]);
            }

            return new BitmapElement(bytes, specialWidth, height);
        }
    }
}
