namespace TscDemo.ImagePrintingApp
{
    internal class Label
    {
        public string Bitmap { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public int ByteWidth => (Width + 7) / 8;
    }
}
