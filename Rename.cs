using System;
using System.Collections;
using System.IO;

namespace FileRenamer
{
    internal class Rename
    {
        public string makeriname(string title, string dai, string wa, bool stou, bool symbol, int stcount, bool usesub, bool useshobo, bool usefile, ArrayList shobotitle, ArrayList filetitle, int count, int filecount)
        {
            string text = null;
            int num = stcount + count;
            int alnum = stcount + filecount;
            string text2 = null;
            text2 = mknumber(num, alnum);
            text += title;
            text = ((!stou) ? (text + " ") : (text + "_"));
            text += dai;
            text += text2;
            text += wa;
            if (symbol)
            {
                text += "「";
            }
            if (usesub)
            {
                if (useshobo)
                {
                    try
                    {
                        text += shobotitle[count - 1].ToString();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        text += "-";
                    }
                }
                else
                {
                    try
                    {
                        text += filetitle[count - 1].ToString();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        text += "-";
                    }
                }
            }
            if (symbol)
            {
                text += "」";
            }
            return ValidFileName(text);
        }

        public string mknumber(int num, int alnum)
        {
            string text = null;
            if (alnum >= 1000)
            {
                if (num < 10) return "000" + num;
                if (num < 100) return "00" + num;
                if (num < 1000) return "0" + num;
                return num.ToString();
            }
            if (alnum >= 100)
            {
                if (num < 10) return "00" + num;
                if (num < 100) return "0" + num;
                return num.ToString();
            }
            if (num < 10) return "0" + num;
            return num.ToString();
        }

        public string renamefile(string src, string dest)
        {
            string extension = Path.GetExtension(src);
            dest += extension;
            FileInfo fileInfo = new FileInfo(src);
            string directoryName = Path.GetDirectoryName(fileInfo.FullName);
            string text = null;
            try
            {
                text = Path.Combine(directoryName, dest);
            }
            catch
            {
            }
            if (File.Exists(text))
            {
                dest = "-dummy-" + dest;
                text = Path.Combine(directoryName, dest);
            }
            try
            {
                File.Move(fileInfo.FullName, text);
                return text;
            }
            catch
            {
                return null;
            }
        }

        public string ValidFileName(string s)
        {
            string text = s;
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (char oldChar in invalidFileNameChars)
            {
                text = text.Replace(oldChar, '_');
            }
            return text;
        }
    }
}
