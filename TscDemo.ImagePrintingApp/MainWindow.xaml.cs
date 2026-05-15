using Microsoft.Win32;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows;

namespace TscDemo.ImagePrintingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitPrinter()
        {
            TscLibWrapper.OpenPort("usb");

            TscLibWrapper.SendCommand("DIRECTION 1");
            TscLibWrapper.SendCommand("SET PEEL OFF");
            TscLibWrapper.SendCommand("SET CUTTER OFF");
            TscLibWrapper.SendCommand("SET CUTTER OFF");
            TscLibWrapper.SendCommand("SET PARTIAL_CUTTER OFF");
            TscLibWrapper.SendCommand("SET TEAR ON");
            //TscLibWrapper.SendCommand("CODEPAGE UTF8");
            TscLibWrapper.SendCommand("SIZE 58 mm,40 mm");
            TscLibWrapper.SendCommand("CLS");
        }

        private void Print()
        {
            TscLibWrapper.PrintLabel("1", "1");
            TscLibWrapper.ClosePort();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string testFileName = "TEST_PNG.png";

            string testFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), testFileName);

            byte[] bytes = File.ReadAllBytes(testFilePath);
            string hexString = BitConverter.ToString(bytes).Replace("-", "");
            string base64String = Convert.ToBase64String(bytes);

            // DOWNLOAD F - for save to flash memory
            string downloadCmd = $"DOWNLOAD \"{testFileName}\",{new System.IO.FileInfo(testFilePath).Length},{base64String}";

            string putCmd = $"PUTPNG 50,50,\"{testFileName}\"";

            InitPrinter();

            TscLibWrapper.SendCommand(downloadCmd);
            TscLibWrapper.SendCommand(putCmd);

            TscLibWrapper.PrintLabel("1", "1");

            TscLibWrapper.SendCommand($"KILL \"{testFileName}\"");

            TscLibWrapper.ClosePort();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string testFileName = "TEST_PNG.png";

            string testFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), testFileName);

            byte[] bytes = File.ReadAllBytes(testFilePath);
            string hexString = BitConverter.ToString(bytes).Replace("-", " ");
            string base64String = Convert.ToBase64String(bytes);

            //int bitmapBytes = 
            string hexInput = "0000000000000000";

            var asciiBytes = Encoding.ASCII.GetBytes(hexInput);
            var asciiHexString = Encoding.ASCII.GetString(asciiBytes);

            byte[] hexBytes = Convert.FromHexString(asciiHexString);

            // DOWNLOAD F - for save to flash memory
            string bitmapCmd = $"BITMAP 50,50,1,1,0,\x30\x30\x30\x30";

            InitPrinter();

            TscLibWrapper.SendCommand(bitmapCmd);

            Print();
        }

        public byte[] HexStringToByteArray(string hex)
        {
            // Удаляем возможный префикс "0x", если он есть
            if (hex.StartsWith("0x")) hex = hex.Substring(2);

            // Удаляем пробелы или запятые, если они есть
            hex = hex.Replace(" ", "").Replace(",", "");

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string bitmapCmd = $"BITMAP 50,40,2,2,0,\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00";
            byte[] ascii = Encoding.UTF8.GetBytes(bitmapCmd);
            int lenght = ascii.Length;

            InitPrinter();
            TscLibWrapper.SendBinaryData(ascii, lenght);
            Print();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            string putCmd = $"PUTPNG 1,1,\"TEST.PNG\"";

            InitPrinter();
            TscLibWrapper.SendCommand(putCmd);
            Print();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                //Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    var label = ImageHelper.GetLabel(fileBytes, 203);

                    /// Создаем заголовок команды и переводим ее в битовый формат
                    byte[] cmdHeaderBytes = Encoding.ASCII.GetBytes($"BITMAP 1,1,{label.ByteWidth},{label.Height},0,");

                    var binaryCommand = new byte[cmdHeaderBytes.Length + label.Data.Length];


                    Buffer.BlockCopy(cmdHeaderBytes, 0, binaryCommand, 0, cmdHeaderBytes.Length);
                    Buffer.BlockCopy(label.Data, 0, binaryCommand, cmdHeaderBytes.Length, label.Data.Length);

                    InitPrinter();
                    TscLibWrapper.SendBinaryData(binaryCommand, binaryCommand.Length);
                    Print();
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}");
                }
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            /// Моделируем матриуц BMP-файла. Принтер печатает false-значения (0)
            /// и пропускает true (1)
            var bmpBits = new BitArray([
                true, true, true, true, false, false, false, false]);

            /// Переводим биты в байты методом группировки.
            int numBmpBytes = (bmpBits.Length + 7) / 8;
            byte[] bmpBytes = new byte[numBmpBytes];
            bmpBits.CopyTo(bmpBytes, 0);

            /// Выполняем зеркалирование битов, чтобы сохранить исходную матрицу
            for (int i = 0; i < bmpBytes.Length; i++)
            {
                bmpBytes[i] = ImageHelper.ReverseBits(bmpBytes[i]);
            }

            /// Создаем заголовок команды и переводим ее в битовый формат
            byte[] cmdHeaderBytes = Encoding.ASCII.GetBytes("BITMAP 50,40,2,1,0,");

            var binaryCommand = new byte[cmdHeaderBytes.Length + bmpBytes.Length];

            Buffer.BlockCopy(cmdHeaderBytes, 0, binaryCommand, 0, cmdHeaderBytes.Length);
            Buffer.BlockCopy(bmpBytes, 0, binaryCommand, cmdHeaderBytes.Length, bmpBytes.Length);

            InitPrinter();
            TscLibWrapper.SendBinaryData(binaryCommand, binaryCommand.Length);
            Print();
        }
    }
}