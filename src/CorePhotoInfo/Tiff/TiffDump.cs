using System.IO;
using CorePhoto.IO;
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

            TiffIfd? ifd = TiffReader.ReadFirstIfd(header, _stream, header.ByteOrder);
            int ifdId = 0;

            while (ifd != null)
            {
                _report.WriteSubheader("IFD {0}", ifdId);
                WriteTiffIfdInfo(ifd.Value, header.ByteOrder);

                ifd = TiffReader.ReadNextIfd(ifd.Value, _stream, header.ByteOrder);
                ifdId++;
            }
        }

        private void WriteTiffIfdInfo(TiffIfd ifd, ByteOrder byteOrder)
        {
            foreach (TiffIfdEntry entry in ifd.Entries)
            {
                WriteTiffIfdEntryInfo(entry, byteOrder);
            }
        }

        private void WriteTiffIfdEntryInfo(TiffIfdEntry entry, ByteOrder byteOrder)
        {
            var typeStr = entry.Count == 1 ? $"{entry.Type}" : $"{entry.Type}[{entry.Count}]";
            var data = TiffReader.ReadData(entry, _stream, byteOrder);

            _report.WriteLine($"{entry.Tag} ({typeStr}) = [{data[0]}, {data[1]}, {data[2]}, {data[3]}]");
        }

        public static void WriteTiffInfo(Stream stream, IReportWriter reportWriter)
        {
            TiffDump instance = new TiffDump(stream, reportWriter);
            instance.WriteTiffInfo();
        }
    }
}