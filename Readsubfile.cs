using System.Collections;
using System.IO;
using System.Text;

namespace FileRenamer
{
    internal class Readsubfile
    {
        public ArrayList readsubfile(string stfile)
        {
            ArrayList arrayList = new ArrayList();
            StreamReader streamReader = new StreamReader(stfile, Encoding.GetEncoding("SHIFT_JIS"));
            while (!streamReader.EndOfStream)
            {
                string value = streamReader.ReadLine();
                arrayList.Add(value);
            }
            streamReader.Close();
            return arrayList;
        }
    }
}
