using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using CorePhotoInfo.Reporting;
using System.Collections.Generic;
using System;
using ImageSharp;
using ImageSharp.Formats;

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

            _tiffDecoder = new TiffDecoderCore(stream, false, null);
        }

        private void WriteTiffInfo()
        {
            uint firstIfdOffset = _tiffDecoder.ReadHeader();

            _report.WriteSubheader("TiffHeader");
            _report.WriteLine("Byte order       : {0}", _tiffDecoder.IsLittleEndian ? "Little Endian" : "Big Endian");

            TiffIfd ifd = _tiffDecoder.ReadIfd(firstIfdOffset);
            WriteTiffIfdInfo(ifd, "IFD ", 0, _tagDictionary);
        }

        private void WriteTiffIfdInfo(TiffIfd ifd, string ifdPrefix, int? ifdId, Dictionary<int, string> tagDictionary)
        {
            _report.WriteSubheader($"{ifdPrefix}{ifdId}");

            // Write the IFD dump

            WriteTiffIfdEntries(ifd, tagDictionary);

            // Decode the image

            // await DecodeImage(ifd, byteOrder, $"{ifdPrefix}{ifdId}");

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
            {
                TiffIfd nextIfd = _tiffDecoder.ReadIfd(ifd.NextIfdOffset);
                WriteTiffIfdInfo(nextIfd, ifdPrefix, ifdId + 1, _tagDictionary);
            }
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
        //         // Baseline TIFF fields

        //         case TiffTags.Artist:
        //             return await ifd.ReadArtistAsync(_stream, byteOrder);
        //         case TiffTags.BitsPerSample:
        //             return await ifd.ReadBitsPerSampleAsync(_stream, byteOrder);
        //         case TiffTags.CellLength:
        //             return ifd.GetCellLength(byteOrder);
        //         case TiffTags.CellWidth:
        //             return ifd.GetCellWidth(byteOrder);
        //         case TiffTags.ColorMap:
        //             return await ifd.ReadColorMapAsync(_stream, byteOrder);
        //         case TiffTags.Compression:
        //             return ifd.GetCompression(byteOrder);
        //         case TiffTags.Copyright:
        //             return await ifd.ReadCopyrightAsync(_stream, byteOrder);
        //         case TiffTags.DateTime:
        //             return await ifd.ReadDateTimeAsync(_stream, byteOrder);
        //         case TiffTags.ExtraSamples:
        //             return await ifd.ReadExtraSamplesAsync(_stream, byteOrder);
        //         case TiffTags.FillOrder:
        //             return ifd.GetFillOrder(byteOrder);
        //         case TiffTags.FreeByteCounts:
        //             return await ifd.ReadFreeByteCountsAsync(_stream, byteOrder);
        //         case TiffTags.FreeOffsets:
        //             return await ifd.ReadFreeOffsetsAsync(_stream, byteOrder);
        //         case TiffTags.GrayResponseCurve:
        //             return await ifd.ReadGrayResponseCurveAsync(_stream, byteOrder);
        //         case TiffTags.GrayResponseUnit:
        //             return ifd.GetGrayResponseUnit(byteOrder);
        //         case TiffTags.HostComputer:
        //             return await ifd.ReadHostComputerAsync(_stream, byteOrder);
        //         case TiffTags.ImageDescription:
        //             return await ifd.ReadImageDescriptionAsync(_stream, byteOrder);
        //         case TiffTags.ImageLength:
        //             return ifd.GetImageLength(byteOrder);
        //         case TiffTags.ImageWidth:
        //             return ifd.GetImageWidth(byteOrder);
        //         case TiffTags.Make:
        //             return await ifd.ReadMakeAsync(_stream, byteOrder);
        //         case TiffTags.MaxSampleValue:
        //             return await ifd.ReadMaxSampleValueAsync(_stream, byteOrder);
        //         case TiffTags.MinSampleValue:
        //             return await ifd.ReadMinSampleValueAsync(_stream, byteOrder);
        //         case TiffTags.Model:
        //             return await ifd.ReadModelAsync(_stream, byteOrder);
        //         case TiffTags.NewSubfileType:
        //             return ifd.GetNewSubfileType(byteOrder);
        //         case TiffTags.Orientation:
        //             return ifd.GetOrientation(byteOrder);
        //         case TiffTags.PhotometricInterpretation:
        //             return ifd.GetPhotometricInterpretation(byteOrder);
        //         case TiffTags.PlanarConfiguration:
        //             return ifd.GetPlanarConfiguration(byteOrder);
        //         case TiffTags.ResolutionUnit:
        //             return ifd.GetResolutionUnit(byteOrder);
        //         case TiffTags.RowsPerStrip:
        //             return ifd.GetRowsPerStrip(byteOrder);
        //         case TiffTags.SamplesPerPixel:
        //             return ifd.GetSamplesPerPixel(byteOrder);
        //         case TiffTags.Software:
        //             return await ifd.ReadSoftwareAsync(_stream, byteOrder);
        //         case TiffTags.StripByteCounts:
        //             return await ifd.ReadStripByteCountsAsync(_stream, byteOrder);
        //         case TiffTags.StripOffsets:
        //             return await ifd.ReadStripOffsetsAsync(_stream, byteOrder);
        //         case TiffTags.SubfileType:
        //             return ifd.GetSubfileType(byteOrder);
        //         case TiffTags.Threshholding:
        //             return ifd.GetThreshholding(byteOrder);
        //         case TiffTags.XResolution:
        //             return await ifd.ReadXResolutionAsync(_stream, byteOrder);
        //         case TiffTags.YResolution:
        //             return await ifd.ReadYResolutionAsync(_stream, byteOrder);

                // Unknown fields

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
                // case TiffType.Rational:
                //     if (entry.Count == 1)
                //         return (await entry.ReadRationalAsync(_stream, byteOrder)).ToString();
                //     else
                //     {
                //         var array = await entry.ReadRationalArrayAsync(_stream, byteOrder);
                //         return ConvertArrayToString(array);
                //     }
                // case TiffType.SRational:
                //     if (entry.Count == 1)
                //         return (await entry.ReadSignedRationalAsync(_stream, byteOrder)).ToString();
                //     else
                //     {
                //         var array = await entry.ReadSignedRationalArrayAsync(_stream, byteOrder);
                //         return ConvertArrayToString(array);
                //     }
                // case TiffType.Float:
                //     if (entry.Count == 1)
                //         return (await entry.ReadFloatAsync(_stream, byteOrder)).ToString();
                //     else
                //     {
                //         var array = await entry.ReadFloatArrayAsync(_stream, byteOrder);
                //         return ConvertArrayToString(array);
                //     }
                // case TiffType.Double:
                //     if (entry.Count == 1)
                //         return (await entry.ReadDoubleAsync(_stream, byteOrder)).ToString();
                //     else
                //     {
                //         var array = await entry.ReadDoubleArrayAsync(_stream, byteOrder);
                //         return ConvertArrayToString(array);
                //     }
                case TiffType.Undefined:
                    return "Undefined";
                default:
                    return $"Unknown Type ({(int)entry.Type})";
            }
        }

        // private async Task DecodeImage(TiffIfd ifd, ByteOrder byteOrder, string imageName)
        // {
        //     var compression = ifd.GetCompression(byteOrder);
        //     var photometricInterpretation = ifd.GetPhotometricInterpretation(byteOrder);
        //     var imageWidth = ifd.GetImageWidth(byteOrder);
        //     var imageLength = ifd.GetImageLength(byteOrder);

        //     if (!TiffDecompressor.SupportsCompression(compression))
        //     {
        //         _report.WriteError($"Image compression format {compression} is not supported.");
        //         return;
        //     }
        //     else if (photometricInterpretation == null)
        //     {
        //         _report.WriteError($"Photometric interpretation is missing.");
        //         return;
        //     }
        //     else if (!TiffImageReader.SupportsPhotometricInterpretation(photometricInterpretation.Value))
        //     {
        //         _report.WriteError($"Photometric interpretation {photometricInterpretation} is not supported.");
        //         return;
        //     }
        //     else
        //     {
        //         var stripOffsets = await ifd.ReadStripOffsetsAsync(_stream, byteOrder);
        //         var stripByteCounts = await ifd.ReadStripByteCountsAsync(_stream, byteOrder);
        //         var rowsPerStrip = (int)ifd.GetRowsPerStrip(byteOrder);
        //         var width = (int)imageWidth.Value;
        //         var height = (int)imageLength.Value;
        //         var bytesPerRow = width * 3;
        //         var samplesPerPixel = (int)ifd.GetSamplesPerPixel(byteOrder);
        //         var imageDecoder = await TiffImageReader.GetImageDecoderAsync(ifd, _stream, byteOrder);

        //         if (stripOffsets != null && stripByteCounts != null)
        //         {
        //             var image = new Image<Rgb888>(width, height);

        //             for (int stripIndex = 0; stripIndex < stripOffsets.Length; stripIndex++)
        //             {
        //                 var sizeOfStrip = (int)rowsPerStrip * bytesPerRow;

        //                 _stream.Seek(stripOffsets[stripIndex], SeekOrigin.Begin);
        //                 var stripLength = (int)stripByteCounts[stripIndex];
        //                 var data = await TiffDecompressor.DecompressStreamAsync(_stream, compression, stripLength, sizeOfStrip);

        //                 var stripHeight = stripIndex < stripOffsets.Length - 1 || height % rowsPerStrip == 0 ? (int)rowsPerStrip : height % rowsPerStrip;

        //                 using (var pixels = image.Lock())
        //                 {
        //                     imageDecoder(data, pixels, new Rectangle(0, stripIndex * rowsPerStrip, width, stripHeight));
        //                 }
        //             }

        //             var filename = Path.Combine(_outputDirectory.FullName, imageName + ".png");

        //             using (FileStream outputStream = File.OpenWrite(filename))
        //             {
        //                 image.To<Color>().Save(outputStream);
        //             }

        //             _report.WriteImage(new FileInfo(filename));
        //         }
        //     }
        // }

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