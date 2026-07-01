using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace FileRenamer
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            if (Environment.CommandLine.IndexOf("/up", StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
                try
                {
                    Process.GetProcessById(Convert.ToInt32(Environment.GetCommandLineArgs()[2])).WaitForExit();
                }
                catch (Exception)
                {
                }
                File.Delete(fileNameWithoutExtension + ".old");
                MessageBox.Show("アップデートされました。", "お知らせ", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(defaultValue: false);
                Application.Run(new Form1());
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(defaultValue: false);
                Application.Run(new Form1());
            }
        }
    }
}
