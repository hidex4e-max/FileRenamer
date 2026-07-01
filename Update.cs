using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.FileIO;

namespace FileRenamer
{
    internal class Update
    {
        public void updatecheck()
        {
            string address = "https://calmtempo.com/softupdate/FileRenamer/verson.txt";
            WebClient webClient = new WebClient();
            try
            {
                Stream stream = webClient.OpenRead(address);
                StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("Shift_JIS"));
                string text = streamReader.ReadLine();
                string address2 = streamReader.ReadLine();
                streamReader.Close();
                stream.Close();
                if (Application.ProductVersion != text)
                {
                    if (MessageBox.Show("新しいバージョンがあります\nアップデートしますか？", "アップデート", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        doupdate(address2);
                    }
                }
                else
                {
                    MessageBox.Show("アップデートはありません", "アップデート", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch
            {
                MessageBox.Show("アップデートサーバーが見つかりません。\nしばらくしてから再実行してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public void doupdate(string address)
        {
            string location = Assembly.GetExecutingAssembly().Location;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            File.Delete(fileNameWithoutExtension + ".old");
            File.Move(fileNameWithoutExtension + ".exe", fileNameWithoutExtension + ".old");
            try
            {
                new Network().DownloadFile(address, fileNameWithoutExtension + ".exe", "", "", showUI: true, 6000, overwrite: true, UICancelOption.ThrowException);
                Process.Start(location, "/up " + Process.GetCurrentProcess().Id);
                Application.Exit();
            }
            catch
            {
                File.Delete(fileNameWithoutExtension + ".exe");
                File.Move(fileNameWithoutExtension + ".old", fileNameWithoutExtension + ".exe");
                if (MessageBox.Show("アップデートに失敗しました。\nHPにアクセスしますか？。", "エラー", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) == DialogResult.Yes)
                {
                    Process.Start("http://calmtempo.webcrow.jp/");
                }
            }
        }
    }
}
