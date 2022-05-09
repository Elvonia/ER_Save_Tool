using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ER_Save_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static SL2 save;
        private static Regulation regulation;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_LoadClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.ShowDialog();

            if (open.FileName == "") return;

            try
            {
                save = new SL2(open.FileName);
            }
            catch
            {
                return;
            }

            string checksum = ByteArrayToString(save.Files[10].Checksum);
            saveControl.txtChecksumMD5.Text = checksum;
            saveControl.txtSteamID.Text = "" + save.Files[10].SteamID;
            saveControl.txtSteamID.IsEnabled = true;
        }

        private void MenuItem_RegulationClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.ShowDialog();

            if (open.FileName == "") return;

            try
            {
                regulation = new Regulation(open.FileName);
                saveControl.txtVersion.Text = regulation.Binder4.Version;
                saveControl.txtVersion.IsEnabled = true;
            }
            catch
            {
                return;
            }
        }

        private void MenuItem_ValidateClick(object sender, RoutedEventArgs e)
        {
            string valid = "";
            for (int i = 0; i < 12; i++)
            {
                if (save == null) { return; }

                var s = save.Files[i];
                string str = "";

                if (SL2.ValidateChecksum(s))
                {
                    str = String.Format("File {0} - Valid \r\n", i);
                }
                else
                {
                    str = String.Format("File {0} - Invalid \r\n", i);
                }
                    
                valid += str;
            }
            if (valid != "")
            {
                MessageBox.Show(valid);
            }
        }

        private void MenuItem_SaveClick(object sender, RoutedEventArgs e)
        {
            if (save == null){ return; }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.ShowDialog();

            if (saveDialog.FileName == "") return;

            int offset;
            long steamID = long.Parse(saveControl.txtSteamID.Text);
            string version = saveControl.txtVersion.Text;
            byte[] oldSteamID = BitConverter.GetBytes(save.Files[10].SteamID);
            byte[] newSteamID = BitConverter.GetBytes(steamID);
            bool overwrite = (bool)saveControl.cbVersion.IsChecked;

            if (overwrite)
            {
                if (save.PatchRegulation(save, regulation, version, overwrite))
                {
                    SoulsFormats.SFUtil.EncryptERRegulation(regulation.Path, regulation.Binder4);
                    MessageBox.Show("Regulation.bin overwritten successfully");
                }
                    
            }
            else
            {
                if (save.PatchRegulation(save, regulation, version, overwrite))
                    MessageBox.Show("Save regulation patched successfully");
            }
            

            for (int i = 0; i < 10; i++)
            {
                if (save.Active[i])
                {
                    offset = KPM.indexOf(save.Files[i].Data, oldSteamID);

                    if (offset != -1)
                        Buffer.BlockCopy(newSteamID, 0, save.Files[i].Data, offset, 8);
                }
            }

            save.Files[10].SteamID = steamID;
            bool saved = save.Save(save, saveDialog.FileName);

            if (saved)
                MessageBox.Show("SL2 saved successfully");
            else
                MessageBox.Show("Error occurred during save");

            saveControl.txtChecksumMD5.Text = ByteArrayToString(save.Files[10].Checksum);
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
