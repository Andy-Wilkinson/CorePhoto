using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using CorePhoto.IO;
using CorePhoto.Metadata.Exif;
using CorePhoto.Tiff;
using CorePhotoInfo.Reporting;
using System.Collections.Generic;
using CorePhoto.Dng;
using CorePhoto.Metadata.Exif;

namespace CorePhotoInfo.Tiff
{
    public class TiffDump
    {
        private Stream _stream;
        private IReportWriter _report;
        private Dictionary<int, string> _tagDictionary = new Dictionary<int, string>();
        private Dictionary<int, string> _exifTagDictionary = new Dictionary<int, string>();

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

            TiffIfd ifd = await TiffReader.ReadFirstIfdAsync(header, _stream, header.ByteOrder);
            await WriteTiffIfdInfoAsync(ifd, header.ByteOrder, "IFD ", 0, _tagDictionary);
        }

        private async Task WriteTiffIfdInfoAsync(TiffIfd ifd, ByteOrder byteOrder, string ifdPrefix, int? ifdId, Dictionary<int, string> tagDictionary)
        {
            _report.WriteSubheader($"{ifdPrefix}{ifdId}");

            await WriteTiffIfdEntriesAsync(ifd, byteOrder, tagDictionary);

            // Write the EXIF IFD

            var exifIfd = await ExifReader.ReadExifIfdAsync(ifd, _stream, byteOrder);

            if (exifIfd != null)
                await WriteTiffIfdInfoAsync(exifIfd.Value, byteOrder, $"{ifdPrefix}{ifdId} (EXIF)", null, _exifTagDictionary);

            // Write the sub-IFDs

            var subIfdCount = TiffReader.CountSubIfds(ifd);

            for (int i = 0; i < subIfdCount; i++)
            {
                TiffIfd subIfd = await TiffReader.ReadSubIfdAsync(ifd, i, _stream, byteOrder);
                await WriteTiffIfdInfoAsync(subIfd, byteOrder, $"{ifdPrefix}{ifdId}-", i, _tagDictionary);
            }

            // Write the next IFD

            TiffIfd? nextIfd = await TiffReader.ReadNextIfdAsync(ifd, _stream, byteOrder);
            if (nextIfd != null)
                await WriteTiffIfdInfoAsync(nextIfd.Value, byteOrder, ifdPrefix, ifdId + 1, _tagDictionary);
        }

        private async Task WriteTiffIfdEntriesAsync(TiffIfd ifd, ByteOrder byteOrder, Dictionary<int, string> tagDictionary)
        {
            foreach (TiffIfdEntry entry in ifd.Entries)
            {
                await WriteTiffIfdEntryInfoAsync(entry, byteOrder, tagDictionary);
            }
        }

        private async Task WriteTiffIfdEntryInfoAsync(TiffIfdEntry entry, ByteOrder byteOrder, Dictionary<int, string> tagDictionary)
        {
            var tagStr = ConvertTagToString(tagDictionary, entry.Tag);
            var typeStr = entry.Count == 1 ? $"{entry.Type}" : $"{entry.Type}[{entry.Count}]";
            string value = await GetTiffIfdEntryDataAsync(entry, byteOrder);

            _report.WriteLine($"{tagStr} ({typeStr}) = {value}");
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

        private string ConvertTagToString(Dictionary<int, string> dictionary, int tag)
        {
            string str;

            if (dictionary.TryGetValue(tag, out str))
                return str;
            else
                return $"UNKNOWN {tag}";
        }

        private void PopulateTagDictionary<T>(Dictionary<int, string> dictionary)
        {
            TypeInfo type = typeof(T).GetTypeInfo();

            foreach (var field in type.GetFields())
            {
                var name = field.Name;
                var value = (int)field.GetRawConstantValue();

                if (dictionary.ContainsKey(value))
                    _report.WriteError("CorePhotoInfo ERROR - Tag defined multiple times '{0}'", value);
                else
                    dictionary.Add(value, name);
            }
        }

        public static void WriteTiffInfo(Stream stream, IReportWriter reportWriter)
        {
            TiffDump instance = new TiffDump(stream, reportWriter);

            instance.PopulateTagDictionary<TiffTags>(instance._tagDictionary);
            instance.PopulateTagDictionary<DngTags>(instance._tagDictionary);
            instance.PopulateTagDictionary<ExifTags>(instance._exifTagDictionary);

            instance.WriteTiffInfoAsync().Wait();
        }
    }
}