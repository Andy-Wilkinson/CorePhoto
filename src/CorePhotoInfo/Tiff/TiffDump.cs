using System.IO;
using CorePhoto.Tiff;
using CorePhotoInfo.Reporting;

namespace CorePhotoInfo.Tiff
{
    public class TiffDump
    {
        private Stream _stream;
        private IReportWriter _report;

        private TiffDump(Stream stream, IReportWriter reportWriter)
        {
            _stream = stream;
            _report = reportWriter;
        }

        private void WriteTiffInfo()
        {
            TiffHeader header = TiffReader.ReadHeader(_stream);

            _report.WriteSubheader("TiffHeader");
            _report.WriteLine("Byte order       : {0}", header.ByteOrder);
            _report.WriteLine("First IFD Offset : {0}", header.FirstIfdOffset);
        }

        public static void WriteTiffInfo(Stream stream, IReportWriter reportWriter)
        {
            TiffDump instance = new TiffDump(stream, reportWriter);
            instance.WriteTiffInfo();
        }
    }
}