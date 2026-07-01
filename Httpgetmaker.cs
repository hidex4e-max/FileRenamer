using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer
{
    internal class Httpgetmaker
    {
        public string gethttp(string url)
        {
            string text = "";
            WebClient webClient = new WebClient();
            try
            {
                webClient.Encoding = Encoding.UTF8;
                string text2 = webClient.DownloadString(url);
                text += text2;
            }
            catch (WebException ex)
            {
                text += ex.Message;
            }
            return text;
        }

        public async Task<string> gethttpAsync(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                try
                {
                    return await webClient.DownloadStringTaskAsync(url);
                }
                catch (WebException ex)
                {
                    return ex.Message;
                }
            }
        }

        public string GetBetweenStrings(string str1, string str2, string orgStr)
        {
            int length = str1.Length;
            int num = orgStr.IndexOf(str1);
            string text = "";
            try
            {
                text = orgStr.Remove(0, num + length);
                int startIndex = text.IndexOf(str2);
                return text.Remove(startIndex);
            }
            catch (Exception)
            {
                return "取得不可";
            }
        }
    }
}
