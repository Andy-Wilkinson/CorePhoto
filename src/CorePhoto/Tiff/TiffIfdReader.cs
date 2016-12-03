using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Numerics;

namespace CorePhoto.Tiff
{
    public static class TiffIfdReader
    {
        // Baseline TIFF fields

        public static Task<string> ReadArtistAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadStringAsync(ifd, TiffTags.Artist, stream, byteOrder);

        public static Task<uint[]> ReadBitsPerSampleAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.BitsPerSample);

            if (entry == null)
                return Task.FromResult(new uint[] { 1 });
            else
                return entry.Value.ReadIntegerArrayAsync(stream, byteOrder);
        }

        public static uint? GetCellLength(this TiffIfd ifd, ByteOrder byteOrder) => GetInteger(ifd, TiffTags.CellLength, byteOrder);

        public static uint? GetCellWidth(this TiffIfd ifd, ByteOrder byteOrder) => GetInteger(ifd, TiffTags.CellWidth, byteOrder);

        public static Task<uint[]> ReadColorMapAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadIntegerArrayAsync(ifd, TiffTags.ColorMap, stream, byteOrder);

        public static TiffCompression GetCompression(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.Compression);
            return entry == null ? TiffCompression.None : (TiffCompression)entry.Value.GetInteger(byteOrder);
        }

        public static Task<string> ReadCopyrightAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadStringAsync(ifd, TiffTags.Copyright, stream, byteOrder);

        public async static Task<DateTimeOffset?> ReadDateTimeAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.DateTime);

            if (entry == null)
                return null;
            else
            {
                var str = await entry.Value.ReadStringAsync(stream, byteOrder);
                return DateTimeOffset.ParseExact(str, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            }
        }

        public async static Task<TiffExtraSamples[]> ReadExtraSamplesAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.ExtraSamples);

            if (entry == null)
                return new TiffExtraSamples[0];
            else
            {
                var values = await entry.Value.ReadIntegerArrayAsync(stream, byteOrder);
                var result = new TiffExtraSamples[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    result[i] = (TiffExtraSamples)values[i];
                }

                return result;
            }
        }

        public static TiffFillOrder GetFillOrder(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.FillOrder);
            return entry == null ? TiffFillOrder.MostSignificantBitFirst : (TiffFillOrder)entry.Value.GetInteger(byteOrder);
        }

        public static Task<uint[]> ReadFreeByteCountsAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadIntegerArrayAsync(ifd, TiffTags.FreeByteCounts, stream, byteOrder);

        public static Task<uint[]> ReadFreeOffsetsAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadIntegerArrayAsync(ifd, TiffTags.FreeOffsets, stream, byteOrder);

        public static Task<uint[]> ReadGrayResponseCurveAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadIntegerArrayAsync(ifd, TiffTags.GrayResponseCurve, stream, byteOrder);

        public static double GetGrayResponseUnit(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.GrayResponseUnit);

            if (entry == null)
                return 0.01;
            else
            {
                var value = entry.Value.GetInteger(byteOrder);
                return Math.Pow(10, -value);
            }
        }
        public static Task<string> ReadHostComputerAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadStringAsync(ifd, TiffTags.HostComputer, stream, byteOrder);

        public static Task<string> ReadImageDescriptionAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadStringAsync(ifd, TiffTags.ImageDescription, stream, byteOrder);

        public static uint? GetImageLength(this TiffIfd ifd, ByteOrder byteOrder) => GetInteger(ifd, TiffTags.ImageLength, byteOrder);

        public static uint? GetImageWidth(this TiffIfd ifd, ByteOrder byteOrder) => GetInteger(ifd, TiffTags.ImageWidth, byteOrder);

        public static Task<string> ReadMakeAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadStringAsync(ifd, TiffTags.Make, stream, byteOrder);

        public static Task<uint[]> ReadMaxSampleValueAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadIntegerArrayAsync(ifd, TiffTags.MaxSampleValue, stream, byteOrder);

        public static Task<uint[]> ReadMinSampleValueAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadIntegerArrayAsync(ifd, TiffTags.MinSampleValue, stream, byteOrder);

        public static Task<string> ReadModelAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadStringAsync(ifd, TiffTags.Model, stream, byteOrder);

        public static TiffNewSubfileType GetNewSubfileType(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.NewSubfileType);
            return entry == null ? TiffNewSubfileType.FullImage : (TiffNewSubfileType)entry.Value.GetInteger(byteOrder);
        }

        public static TiffOrientation GetOrientation(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.Orientation);
            return entry == null ? TiffOrientation.TopLeft : (TiffOrientation)entry.Value.GetInteger(byteOrder);
        }

        public static TiffPhotometricInterpretation? GetPhotometricInterpretation(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.PhotometricInterpretation);
            return entry == null ? null : (TiffPhotometricInterpretation?)entry.Value.GetInteger(byteOrder);
        }

        public static TiffPlanarConfiguration GetPlanarConfiguration(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.PlanarConfiguration);
            return entry == null ? TiffPlanarConfiguration.Chunky : (TiffPlanarConfiguration)entry.Value.GetInteger(byteOrder);
        }

        public static TiffResolutionUnit GetResolutionUnit(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.ResolutionUnit);
            return entry == null ? TiffResolutionUnit.Inch : (TiffResolutionUnit)entry.Value.GetInteger(byteOrder);
        }

        public static uint GetRowsPerStrip(this TiffIfd ifd, ByteOrder byteOrder) => GetInteger(ifd, TiffTags.RowsPerStrip, byteOrder) ?? UInt32.MaxValue;

        public static uint GetSamplesPerPixel(this TiffIfd ifd, ByteOrder byteOrder) => GetInteger(ifd, TiffTags.SamplesPerPixel, byteOrder) ?? 1;

        public static Task<string> ReadSoftwareAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadStringAsync(ifd, TiffTags.Software, stream, byteOrder);

        public static Task<uint[]> ReadStripByteCountsAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadIntegerArrayAsync(ifd, TiffTags.StripByteCounts, stream, byteOrder);

        public static Task<uint[]> ReadStripOffsetsAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadIntegerArrayAsync(ifd, TiffTags.StripOffsets, stream, byteOrder);

        public static TiffSubfileType? GetSubfileType(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.SubfileType);
            return entry == null ? null : (TiffSubfileType?)entry.Value.GetInteger(byteOrder);
        }

        public static TiffThreshholding GetThreshholding(this TiffIfd ifd, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.Threshholding);
            return entry == null ? TiffThreshholding.None : (TiffThreshholding)entry.Value.GetInteger(byteOrder);
        }

        public static Task<Rational?> ReadXResolutionAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadRationalAsync(ifd, TiffTags.XResolution, stream, byteOrder);

        public static Task<Rational?> ReadYResolutionAsync(this TiffIfd ifd, Stream stream, ByteOrder byteOrder) => ReadRationalAsync(ifd, TiffTags.YResolution, stream, byteOrder);

        // Helper functions

        private static uint? GetInteger(TiffIfd ifd, ushort tag, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, tag);
            return entry?.GetInteger(byteOrder);
        }

        private static Task<uint[]> ReadIntegerArrayAsync(TiffIfd ifd, ushort tag, Stream stream, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, tag);

            if (entry == null)
                return Task.FromResult<uint[]>(null);
            else
                return entry.Value.ReadIntegerArrayAsync(stream, byteOrder);
        }

        private async static Task<Rational?> ReadRationalAsync(TiffIfd ifd, ushort tag, Stream stream, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, tag);

            if (entry == null)
                return null;
            else
                return await entry.Value.ReadRationalAsync(stream, byteOrder);
        }

        private static Task<string> ReadStringAsync(TiffIfd ifd, ushort tag, Stream stream, ByteOrder byteOrder)
        {
            var entry = TiffReader.GetTiffIfdEntry(ifd, tag);

            if (entry == null)
                return Task.FromResult<string>(null);
            else
                return entry.Value.ReadStringAsync(stream, byteOrder);
        }
    }
}