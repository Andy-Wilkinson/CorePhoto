using System;
using System.IO;
using CorePhotoInfo.Reporting;
using CorePhotoInfo.Tiff;

namespace CorePhotoInfo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string fileName = args[0];

            FileInfo fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("The specified file does not exist.");
                return;
            }

            DirectoryInfo outputDirectory = new DirectoryInfo(Path.Combine(fileInfo.Directory.FullName, "CorePhotoInfo_" + fileInfo.Name));
            CreateOrCleanDirectory(outputDirectory);

            IReportWriter report = new ConsoleReportWriter();

            using (FileStream stream = File.OpenRead(fileName))
            {
                TiffDump.WriteTiffInfo(stream, report, outputDirectory);
            }
        }

        private static void CreateOrCleanDirectory(DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                directory.Create();
            }
            else
            {
                foreach (FileInfo subFile in directory.GetFiles())
                {
                    subFile.Delete();
                }
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    subDirectory.Delete(true);
                }
            }
        }
    }
}
