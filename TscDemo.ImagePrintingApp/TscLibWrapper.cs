using System.Runtime.InteropServices;

namespace TscDemo.ImagePrintingApp
{
    /// <summary>
    /// Необходимо обернуть все методы библиотеки. При частичной реализации начинает сбоить
    /// (например, не печатает текстовые блоки)
    /// </summary>
    internal static class TscLibWrapper
    {
        [DllImport("TSCLIB.dll", EntryPoint = "about", CharSet = CharSet.Unicode)]
        public static extern int About();

        [DllImport("TSCLIB.dll", EntryPoint = "openport", CharSet = CharSet.Unicode)]
        public static extern int OpenPort(string printername);

        [DllImport("TSCLIB.dll", EntryPoint = "clearbuffer", CharSet = CharSet.Unicode)]
        public static extern int ClearBuffer();

        [DllImport("TSCLIB.dll", EntryPoint = "closeport", CharSet = CharSet.Unicode)]
        public static extern int ClosePort();

        [DllImport("TSCLIB.dll", EntryPoint = "sendcommand")]
        public static extern int SendCommand(string printercommand);

        /// <summary>
        /// Статус принтера. 0 = idle, 1 = head open, 16 = pause, following <ESC>!? command of TSPL manual
        /// </summary>
        [DllImport("TSCLIB.dll", EntryPoint = "usbportqueryprinter", CharSet = CharSet.Unicode)]
        public static extern byte UsbPortQueryPrinter();

        [DllImport("TSCLIB.dll", EntryPoint = "usbportqueryprinter", CharSet = CharSet.Unicode)]
        public static extern byte UsbPrinterName();

        [DllImport("TSCLIB.dll", EntryPoint = "downloadpcx")]
        public static extern int DownloadPcx(string filename, string image_name);

        [DllImport("TSCLIB.dll", EntryPoint = "barcode")]
        public static extern int Barcode(string x, string y, string type,
        string height, string readable, string rotation,
        string narrow, string wide, string code);

        [DllImport("TSCLIB.dll", EntryPoint = "windowsfont")]
        public static extern int WindowsFont(int x, int y, int fontheight,
            int rotation, int fontstyle, int fontunderline,
            string szFaceName, string content);

        [DllImport("TSCLIB.dll", EntryPoint = "windowsfontUnicode")]
        public static extern int WindowsFontUnicode(int x, int y, int fontheight,
             int rotation, int fontstyle, int fontunderline,
             string szFaceName, byte[] content);

        [DllImport("TSCLIB.dll", EntryPoint = "sendBinaryData")]
        public static extern int SendBinaryData(byte[] content, int length);

        [DllImport("TSCLIB.dll", EntryPoint = "printerfont")]
        public static extern int PrinterFont(string x, string y, string fonttype,
            string rotation, string xmul, string ymul,
            string text);

        [DllImport("TSCLIB.dll", EntryPoint = "printlabel")]
        public static extern int PrintLabel(string set, string copy);
    }
}
