using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using CorePhotoInfo.Reporting;
using System.Collections.Generic;
using System;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Formats.Tiff;

namespace CorePhotoInfo.Tiff
{
    public class TiffDump
    {
        private Stream _stream;
        private TiffDecoderCore _tiffDecoder;
        private IReportWriter _report;
        private DirectoryInfo _outputDirectory;
        private Dictionary<int, string> _tagDictionary = new Dictionary<int, string>();
        private Dictionary<int, string> _exifTagDictionary = new Dictionary<int, string>();

        private TiffDump(Stream stream, IReportWriter reportWriter, DirectoryInfo outputDirectory)
        {
            _stream = stream;
            _report = reportWriter;
            _outputDirectory = outputDirectory;

            _tiffDecoder = new TiffDecoderCore(stream, false, null, null);
        }

        private void WriteTiffInfo()
        {
            uint firstIfdOffset = _tiffDecoder.ReadHeader();

            _report.WriteSubheader("TiffHeader");
            _report.WriteLine("Byte order       : {0}", _tiffDecoder.IsLittleEndian ? "Little Endian" : "Big Endian");

            WriteTiffIfdInfo(firstIfdOffset, "IFD ", 0, _tagDictionary);
        }

        private void WriteTiffIfdInfo(uint offset, string ifdPrefix, int? ifdId, Dictionary<int, string> tagDictionary)
        {
            TiffIfd ifd = _tiffDecoder.ReadIfd(offset);
            _report.WriteSubheader($"{ifdPrefix}{ifdId} (Offset = {offset})");

            // Write the IFD dump

            WriteTiffIfdEntries(ifd, tagDictionary);

            // Decode the image

            DecodeImage(ifd, $"{ifdPrefix}{ifdId}");

            // Write the EXIF IFD

            // var exifIfdReference = ExifReader.GetExifIfdReference(ifd, byteOrder);

            // if (exifIfdReference != null)
            // {
            //     var exifIfd = await TiffReader.ReadIfdAsync(exifIfdReference.Value, _stream, byteOrder);
            //     await WriteTiffIfdInfoAsync(exifIfd, byteOrder, $"{ifdPrefix}{ifdId} (EXIF)", null, _exifTagDictionary);
            // }

            // Write the sub-IFDs

            // var subIfdReferences = await TiffReader.ReadSubIfdReferencesAsync(ifd, _stream, byteOrder);

            // for (int i = 0; i < subIfdReferences.Length; i++)
            // {
            //     TiffIfd subIfd = await TiffReader.ReadIfdAsync(subIfdReferences[i], _stream, byteOrder);
            //     await WriteTiffIfdInfoAsync(subIfd, byteOrder, $"{ifdPrefix}{ifdId}-", i, _tagDictionary);
            // }

            // Write the next IFD

            if (ifd.NextIfdOffset != 0)
                WriteTiffIfdInfo(ifd.NextIfdOffset, ifdPrefix, ifdId + 1, _tagDictionary);
        }

        private void WriteTiffIfdEntries(TiffIfd ifd, Dictionary<int, string> tagDictionary)
        {
            foreach (TiffIfdEntry entry in ifd.Entries)
            {
                WriteTiffIfdEntryInfo(entry, tagDictionary);
            }
        }

        private void WriteTiffIfdEntryInfo(TiffIfdEntry entry, Dictionary<int, string> tagDictionary)
        {
            var tagStr = ConvertTagToString(tagDictionary, entry.Tag);
            var typeStr = entry.Count == 1 ? $"{entry.Type}" : $"{entry.Type}[{entry.Count}]";
            object value = GetTiffIfdEntryValue(entry);

            if (value is Array)
                value = ConvertArrayToString((Array)value);

            _report.WriteLine($"{tagStr} = {value}");
        }

        private object GetTiffIfdEntryValue(TiffIfdEntry entry)
        {
            switch (entry.Tag)
            {
                // Use named enums if known

                case TiffTags.Compression:
                    return (TiffCompression)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.ExtraSamples:
                    return (TiffExtraSamples)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.FillOrder:
                    return (TiffFillOrder)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.NewSubfileType:
                    return (TiffNewSubfileType)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.Orientation:
                    return (TiffOrientation)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.PhotometricInterpretation:
                    return (TiffPhotometricInterpretation)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.PlanarConfiguration:
                    return (TiffPlanarConfiguration)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.ResolutionUnit:
                    return (TiffResolutionUnit)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.SubfileType:
                    return (TiffSubfileType)_tiffDecoder.ReadUnsignedInteger(ref entry);
                case TiffTags.Threshholding:
                    return (TiffThreshholding)_tiffDecoder.ReadUnsignedInteger(ref entry);

                // Other fields

                default:
                    return GetTiffIfdEntryData(entry);
            }
        }

        private string GetTiffIfdEntryData(TiffIfdEntry entry)
        {
            switch (entry.Type)
            {
                case TiffType.Byte:
                case TiffType.Short:
                case TiffType.Long:
                    if (entry.Count == 1)
                        return _tiffDecoder.ReadUnsignedInteger(ref entry).ToString();
                    else
                    {
                        var array = _tiffDecoder.ReadUnsignedIntegerArray(ref entry);
                        return ConvertArrayToString(array);
                    }
                case TiffType.SByte:
                case TiffType.SShort:
                case TiffType.SLong:
                    if (entry.Count == 1)
                        return _tiffDecoder.ReadSignedInteger(ref entry).ToString();
                    else
                    {
                        var array = _tiffDecoder.ReadSignedIntegerArray(ref entry);
                        return ConvertArrayToString(array);
                    }
                case TiffType.Ascii:
                    return "\"" + _tiffDecoder.ReadString(ref entry) + "\"";
                case TiffType.Rational:
                    if (entry.Count == 1)
                        return _tiffDecoder.ReadUnsignedRational(ref entry).ToString();
                    else
                    {
                        var array = _tiffDecoder.ReadUnsignedRationalArray(ref entry);
                        return ConvertArrayToString(array);
                    }
                case TiffType.SRational:
                    if (entry.Count == 1)
                        return _tiffDecoder.ReadSignedRational(ref entry).ToString();
                    else
                    {
                        var array = _tiffDecoder.ReadSignedRationalArray(ref entry);
                        return ConvertArrayToString(array);
                    }
                case TiffType.Float:
                    if (entry.Count == 1)
                        return _tiffDecoder.ReadFloat(ref entry).ToString();
                    else
                    {
                        var array = _tiffDecoder.ReadFloatArray(ref entry);
                        return ConvertArrayToString(array);
                    }
                case TiffType.Double:
                    if (entry.Count == 1)
                        return _tiffDecoder.ReadDouble(ref entry).ToString();
                    else
                    {
                        var array = _tiffDecoder.ReadDoubleArray(ref entry);
                        return ConvertArrayToString(array);
                    }
                case TiffType.Undefined:
                    return "Undefined";
                default:
                    return $"Unknown Type ({(int)entry.Type})";
            }
        }

        private void DecodeImage(TiffIfd ifd, string imageName)
        {
            try
            {
                var image = _tiffDecoder.DecodeImage<Rgba32>(ifd);

                var filename = Path.Combine(_outputDirectory.FullName, imageName + ".png");

                using (FileStream outputStream = File.OpenWrite(filename))
                {
                    image.Save(outputStream);
                }

                _report.WriteImage(new FileInfo(filename));
            }
            catch (Exception e)
            {
                _report.WriteError(e.Message);
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

        private string ConvertArrayToString(Array array)
        {
            var maxArraySize = 10;
            var itemsToDisplay = Math.Min(array.Length, maxArraySize);
            var truncatedArray = new string[itemsToDisplay];

            for (int i = 0; i < itemsToDisplay; i++)
            {
                truncatedArray[i] = array.GetValue(i).ToString();
            }

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

        public static void WriteTiffInfo(Stream stream, IReportWriter reportWriter, DirectoryInfo outputDirectory)
        {
            TiffDump instance = new TiffDump(stream, reportWriter, outputDirectory);

            instance.PopulateTagDictionary<TiffTags>(instance._tagDictionary);
            // instance.PopulateTagDictionary<DngTags>(instance._tagDictionary);
            // instance.PopulateTagDictionary<ExifTags>(instance._exifTagDictionary);

            instance.WriteTiffInfo();
        }
    }
}