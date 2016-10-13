using System.IO;
using CorePhotoInfo.Reporting;
using CorePhotoInfo.Tiff;

namespace CorePhotoInfo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IReportWriter report = new ConsoleReportWriter();
            string fileName = args[0];

            using (FileStream stream = File.OpenRead(fileName))
            {
                TiffDump.WriteTiffInfo(stream, report);
            }
        }
    }
}
