using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using CorePhoto.IO;
using CorePhoto.Tiff;
using CorePhotoInfo.Reporting;
using System.Collections.Generic;
using CorePhoto.Dng;

namespace CorePhotoInfo.Tiff
{
    public class TiffDump
    {
        private Stream _stream;
        private IReportWriter _report;
        private Dictionary<int, string> _tagDictionary = new Dictionary<int, string>();

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
            await WriteTiffIfdInfoAsync(ifd, header.ByteOrder, "IFD ", 0);
        }

        private async Task WriteTiffIfdInfoAsync(TiffIfd ifd, ByteOrder byteOrder, string ifdPrefix, int? ifdId)
        {
            _report.WriteSubheader($"{ifdPrefix}{ifdId}");

            await WriteTiffIfdEntriesAsync(ifd, byteOrder);

            // Write the EXIF IFD

            var exifIfdEntry = ifd.Entries.FirstOrDefault(e => e.Tag == TiffTags.ExifIFD);

            if (exifIfdEntry.Tag != 0)
            {
                uint[] subIfdOffsets = await TiffReader.ReadIntegerArrayAsync(exifIfdEntry, _stream, byteOrder);

                for (int i = 0; i < subIfdOffsets.Length; i++)
                {
                    TiffIfd subIfd = await TiffReader.ReadIfdAsync(_stream, byteOrder, subIfdOffsets[i]);
                    await WriteTiffIfdInfoAsync(subIfd, byteOrder, $"{ifdPrefix}{ifdId} (EXIF)", null);
                }
            }

            // Write the sub-IFDs

            var subIfdEntry = ifd.Entries.FirstOrDefault(e => e.Tag == TiffTags.SubIFDs);

            if (subIfdEntry.Tag != 0)
            {
                uint[] subIfdOffsets = await TiffReader.ReadIntegerArrayAsync(subIfdEntry, _stream, byteOrder);

                for (int i = 0; i < subIfdOffsets.Length; i++)
                {
                    TiffIfd subIfd = await TiffReader.ReadIfdAsync(_stream, byteOrder, subIfdOffsets[i]);
                    await WriteTiffIfdInfoAsync(subIfd, byteOrder, $"{ifdPrefix}{ifdId}-", i);
                }
            }

            // Write the next IFD

            TiffIfd? nextIfd = await TiffReader.ReadNextIfdAsync(ifd, _stream, byteOrder);
            if (nextIfd != null)
                await WriteTiffIfdInfoAsync(nextIfd.Value, byteOrder, ifdPrefix, ifdId + 1);
        }

        private async Task WriteTiffIfdEntriesAsync(TiffIfd ifd, ByteOrder byteOrder)
        {
            foreach (TiffIfdEntry entry in ifd.Entries)
            {
                await WriteTiffIfdEntryInfoAsync(entry, byteOrder);
            }
        }

        private async Task WriteTiffIfdEntryInfoAsync(TiffIfdEntry entry, ByteOrder byteOrder)
        {
            var tagStr = ConvertTagToString(_tagDictionary, entry.Tag);
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

            instance.WriteTiffInfoAsync().Wait();
        }
    }
}