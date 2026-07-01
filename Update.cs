using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace FileRenamer
{
    internal class Update
    {
        public async void updatecheck()
        {
            try
            {
                string apiUrl = "https://api.github.com/repos/hidex4e-max/FileRenamer/releases/latest";
                WebClient webClient = new WebClient();
                webClient.Headers.Add("User-Agent", "FileRenamer");
                string json = await webClient.DownloadStringTaskAsync(apiUrl);

                string tag = GetJsonValue(json, "tag_name");
                string version = tag?.TrimStart('v');
                string downloadUrl = GetJsonValue(json, "browser_download_url");

                if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(downloadUrl))
                {
                    MessageBox.Show("バージョン情報を取得できませんでした", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }

                if (Application.ProductVersion != version)
                {
                    if (MessageBox.Show("新しいバージョン (" + version + ") があります\nアップデートしますか？", "アップデート", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        doupdate(downloadUrl);
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
            try
            {
                File.Move(fileNameWithoutExtension + ".exe", fileNameWithoutExtension + ".old");
            }
            catch { }
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("User-Agent", "FileRenamer");
                webClient.DownloadFile(address, fileNameWithoutExtension + ".exe");
                Process.Start(location, "/up " + Process.GetCurrentProcess().Id);
                Application.Exit();
            }
            catch
            {
                try { File.Delete(fileNameWithoutExtension + ".exe"); } catch { }
                try { File.Move(fileNameWithoutExtension + ".old", fileNameWithoutExtension + ".exe"); } catch { }
                if (MessageBox.Show("アップデートに失敗しました。\nリリースページにアクセスしますか？", "エラー", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) == DialogResult.Yes)
                {
                    Process.Start("https://github.com/hidex4e-max/FileRenamer/releases");
                }
            }
        }

        private string GetJsonValue(string json, string key)
        {
            string search = "\"" + key + "\":\"";
            int idx = json.IndexOf(search);
            if (idx == -1)
            {
                search = "\"" + key + "\": \"";
                idx = json.IndexOf(search);
                if (idx == -1) return null;
            }
            idx += search.Length;
            int end = json.IndexOf("\"", idx);
            if (end == -1) return null;
            return json.Substring(idx, end - idx);
        }
    }
}
