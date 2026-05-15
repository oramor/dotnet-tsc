namespace TscDemo.ImagePrintingApp
{
    internal record BitmapElement(byte[] Data, int Width, int Height)
    {
        public int ByteWidth => (Width + 7) / 8;
    }
}
