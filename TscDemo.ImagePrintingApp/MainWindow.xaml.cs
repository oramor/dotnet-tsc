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

        /// <summary>
        /// Демонстрация отправки файла на принтер и печать файла, загруженного в память.
        /// НЕ доработано. Нужно отправлять данные в бинарном виде, чтобы они корректно
        /// записались в память. По аналогии с BITMAP
        /// </summary>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                //Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            string filePath = openFileDialog.FileName;
            string fileName = openFileDialog.SafeFileName;

            byte[] bytes = File.ReadAllBytes(filePath);
            string hexString = BitConverter.ToString(bytes).Replace("-", "");
            string base64String = Convert.ToBase64String(bytes);

            // DOWNLOAD F - for save to flash memory
            string downloadCmd = $"DOWNLOAD \"{fileName}\",{new System.IO.FileInfo(filePath).Length},{base64String}";
            string putCmd = $"PUTPNG 50,50,\"{fileName}\"";

            InitPrinter();

            TscLibWrapper.SendCommand(downloadCmd);
            TscLibWrapper.SendCommand(putCmd);
            TscLibWrapper.PrintLabel("1", "1");
            TscLibWrapper.SendCommand($"KILL \"{fileName}\"");
            TscLibWrapper.ClosePort();
        }

        /// <summary>
        /// Тест конвертации файла в Bitmap и отправки данных на принтер в бинарном виде.
        /// </summary>
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
    }
}