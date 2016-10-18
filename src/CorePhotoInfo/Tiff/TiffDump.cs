using System.IO;
using System.Linq;
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
            string value = GetTiffIfdEntryData(entry, byteOrder);

            _report.WriteLine($"{entry.Tag} ({typeStr}) = {value}");
        }

        private string GetTiffIfdEntryData(TiffIfdEntry entry, ByteOrder byteOrder)
        {
            switch (entry.Type)
            {
                case TiffType.Byte:
                case TiffType.Short:
                case TiffType.Long:
                    if (entry.Count == 1)
                        return TiffReader.GetInteger(entry, byteOrder).ToString();
                    else
                    {
                        uint[] array = TiffReader.ReadIntegerArray(entry, _stream, byteOrder);
                        return ConvertArrayToString(array);
                    }
                case TiffType.SByte:
                case TiffType.SShort:
                case TiffType.SLong:
                    if (entry.Count == 1)
                        return TiffReader.GetSignedInteger(entry, byteOrder).ToString();
                    else
                    {
                        int[] array = TiffReader.ReadSignedIntegerArray(entry, _stream, byteOrder);
                        return ConvertArrayToString(array);
                    }
                default:
                    return "Unknown Type";
            }
        }

        private string ConvertArrayToString<T>(T[] array)
        {
            var maxArraySize = 10;
            var truncatedArray = array.Take(maxArraySize).Select(i => i.ToString());
            var arrayString = string.Join(", ", truncatedArray);
            var continuationString = array.Length > maxArraySize ? ", ..." : "";

            return $"[{arrayString}{continuationString}]";
        }

        public static void WriteTiffInfo(Stream stream, IReportWriter reportWriter)
        {
            TiffDump instance = new TiffDump(stream, reportWriter);
            instance.WriteTiffInfo();
        }
    }
}