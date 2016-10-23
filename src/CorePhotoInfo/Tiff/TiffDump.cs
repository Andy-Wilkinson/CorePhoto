using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        private async Task WriteTiffInfoAsync()
        {
            TiffHeader header = await TiffReader.ReadHeaderAsync(_stream);

            _report.WriteSubheader("TiffHeader");
            _report.WriteLine("Byte order       : {0}", header.ByteOrder);

            TiffIfd? ifd = await TiffReader.ReadFirstIfdAsync(header, _stream, header.ByteOrder);
            int ifdId = 0;

            while (ifd != null)
            {
                _report.WriteSubheader("IFD {0}", ifdId);
                await WriteTiffIfdInfoAsync(ifd.Value, header.ByteOrder);

                ifd = await TiffReader.ReadNextIfdAsync(ifd.Value, _stream, header.ByteOrder);
                ifdId++;
            }
        }

        private async Task WriteTiffIfdInfoAsync(TiffIfd ifd, ByteOrder byteOrder)
        {
            foreach (TiffIfdEntry entry in ifd.Entries)
            {
                await WriteTiffIfdEntryInfoAsync(entry, byteOrder);
            }
        }

        private async Task WriteTiffIfdEntryInfoAsync(TiffIfdEntry entry, ByteOrder byteOrder)
        {
            var typeStr = entry.Count == 1 ? $"{entry.Type}" : $"{entry.Type}[{entry.Count}]";
            string value = await GetTiffIfdEntryDataAsync(entry, byteOrder);

            _report.WriteLine($"{entry.Tag} ({typeStr}) = {value}");
        }

        private async Task<string> GetTiffIfdEntryDataAsync(TiffIfdEntry entry, ByteOrder byteOrder)
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
                        var array = await TiffReader.ReadIntegerArrayAsync(entry, _stream, byteOrder);
                        return ConvertArrayToString(array);
                    }
                case TiffType.SByte:
                case TiffType.SShort:
                case TiffType.SLong:
                    if (entry.Count == 1)
                        return TiffReader.GetSignedInteger(entry, byteOrder).ToString();
                    else
                    {
                        var array = await TiffReader.ReadSignedIntegerArrayAsync(entry, _stream, byteOrder);
                        return ConvertArrayToString(array);
                    }
                case TiffType.Ascii:
                    return "\"" + await TiffReader.ReadStringAsync(entry, _stream, byteOrder) + "\"";
                case TiffType.Rational:
                    if (entry.Count == 1)
                        return (await TiffReader.ReadRationalAsync(entry, _stream, byteOrder)).ToString();
                    else
                    {
                        var array = await TiffReader.ReadRationalArrayAsync(entry, _stream, byteOrder);
                        return ConvertArrayToString(array);
                    }
                case TiffType.SRational:
                    if (entry.Count == 1)
                        return (await TiffReader.ReadSignedRationalAsync(entry, _stream, byteOrder)).ToString();
                    else
                    {
                        var array = await TiffReader.ReadSignedRationalArrayAsync(entry, _stream, byteOrder);
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
            instance.WriteTiffInfoAsync().Wait();
        }
    }
}