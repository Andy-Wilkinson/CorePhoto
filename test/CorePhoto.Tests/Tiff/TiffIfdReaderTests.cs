using System;
using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Numerics;
using CorePhoto.Tests.Helpers;
using CorePhoto.Tiff;
using Xunit;

namespace CorePhoto.Tests.Tiff
{
    public class TiffIfdReaderTests
    {
        public static object[][] SampleAsciiValues = new[] { new object[] { ByteOrder.LittleEndian, TiffType.Ascii, "\0", "" },
                                                             new object[] { ByteOrder.LittleEndian, TiffType.Ascii, "Test Text\0", "Test Text" },
                                                             new object[] { ByteOrder.LittleEndian, TiffType.Ascii, "First\0Second\0", "First\0Second" },
                                                             new object[] { ByteOrder.LittleEndian, TiffType.Ascii, null, null },
                                                             new object[] { ByteOrder.BigEndian, TiffType.Ascii, "\0", "" },
                                                             new object[] { ByteOrder.BigEndian, TiffType.Ascii, "Test Text\0", "Test Text" },
                                                             new object[] { ByteOrder.BigEndian, TiffType.Ascii, "First\0Second\0", "First\0Second" },
                                                             new object[] { ByteOrder.BigEndian, TiffType.Ascii, null, null }};

        public static object[][] SampleIntegerValues = new[] { new object[] { ByteOrder.LittleEndian, TiffType.Undefined, (uint?)null },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Byte, (uint?)42 },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Short, (uint?)3482 },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Long, (uint?)96326 },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Byte, (uint?)42 },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Short, (uint?)3482 },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Long, (uint?)96326 }};

        public static object[][] SampleIntegerArrayValues = new[] { new object[] { ByteOrder.LittleEndian, TiffType.Undefined, null },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Byte, new uint[] { 42 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Byte, new uint[] { 41, 96 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Byte, new uint[] { 35, 91, 17 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Byte, new uint[] { 18, 91, 17, 24 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Byte, new uint[] { 28, 12, 63, 85, 13 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Short, new uint[] { 3482 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Short, new uint[] { 1452, 2852 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Short, new uint[] { 3452, 7934, 1853 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Long, new uint[] { 96326 } },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Long, new uint[] { 26894, 68395 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Byte, new uint[] { 42 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Byte, new uint[] { 41, 96 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Byte, new uint[] { 35, 91, 17 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Byte, new uint[] { 18, 91, 17, 24 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Byte, new uint[] { 28, 12, 63, 85, 13 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Short, new uint[] { 3482 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Short, new uint[] { 1452, 2852 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Short, new uint[] { 3452, 7934, 1853 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Long, new uint[] { 96326 } },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Long, new uint[] { 26894, 68395 } }};

        public static object[][] SampleRationalValues = new[] { new object[] { ByteOrder.LittleEndian, TiffType.Undefined, (Rational?)null },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Rational, (Rational?)new Rational(1, 3) },
                                                               new object[] { ByteOrder.LittleEndian, TiffType.Rational, (Rational?)new Rational(10, 21) },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Rational, (Rational?)new Rational(1, 3) },
                                                               new object[] { ByteOrder.BigEndian, TiffType.Rational, (Rational?)new Rational(10, 21) }};

        public static object[][] SampleDateTimeValues = new[] { new object[] { ByteOrder.LittleEndian, TiffType.Ascii, "2012:05:18 11:46:08\0", new DateTimeOffset(2012, 5, 18, 11, 46, 8, TimeSpan.Zero) },
                                                             new object[] { ByteOrder.LittleEndian, TiffType.Ascii, null, null },
                                                             new object[] { ByteOrder.BigEndian, TiffType.Ascii, "2012:05:18 11:46:08\0", new DateTimeOffset(2012, 5, 18, 11, 46, 8, TimeSpan.Zero) },
                                                             new object[] { ByteOrder.BigEndian, TiffType.Ascii, null, null }};

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadArtistAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.Artist, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadArtistAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadBitsPerSampleAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.BitsPerSample, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadBitsPerSampleAsync(stream, byteOrder);

            if (value == null)
                value = new uint[] { 1 }; // Default value

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerValues))]
        public void GetCellLength_ReturnsValue(ByteOrder byteOrder, TiffType type, uint? value)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.CellLength, type, value, byteOrder).Ifd;

            var result = ifd.GetCellLength(byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerValues))]
        public void GetCellWidth_ReturnsValue(ByteOrder byteOrder, TiffType type, uint? value)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.CellWidth, type, value, byteOrder).Ifd;

            var result = ifd.GetCellWidth(byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadColorMapAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.ColorMap, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadColorMapAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, TiffCompression.None)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffCompression.None)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 32773, TiffCompression.PackBits)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffCompression)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffCompression.None)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 32773, TiffCompression.PackBits)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffCompression)99)]
        public void GetCompression_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffCompression expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.Compression, type, data, byteOrder).Ifd;

            var result = ifd.GetCompression(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadCopyrightAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.Copyright, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadCopyrightAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleDateTimeValues))]
        public async Task ReadDateTimeAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, DateTimeOffset? expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.DateTime, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadDateTimeAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, new TiffExtraSamples[0])]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, new uint[] { 0 }, new[] { TiffExtraSamples.Unspecified })]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, new uint[] { 1, 2 }, new[] { TiffExtraSamples.AssociatedAlpha, TiffExtraSamples.UnassociatedAlpha })]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, new uint[] { 99 }, new[] { (TiffExtraSamples)99 })]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, new uint[] { 0 }, new[] { TiffExtraSamples.Unspecified })]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, new uint[] { 1, 2 }, new[] { TiffExtraSamples.AssociatedAlpha, TiffExtraSamples.UnassociatedAlpha })]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, new uint[] { 99 }, new[] { (TiffExtraSamples)99 })]
        public async Task ReadExtraSamplesAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] data, TiffExtraSamples[] expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.ExtraSamples, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadExtraSamplesAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, TiffFillOrder.MostSignificantBitFirst)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffFillOrder.MostSignificantBitFirst)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 2, TiffFillOrder.LeastSignificantBitFirst)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffFillOrder)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffFillOrder.MostSignificantBitFirst)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 2, TiffFillOrder.LeastSignificantBitFirst)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffFillOrder)99)]
        public void GetFillOrder_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffFillOrder expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.FillOrder, type, data, byteOrder).Ifd;

            var result = ifd.GetFillOrder(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadFreeByteCountsAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.FreeByteCounts, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadFreeByteCountsAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadFreeOffsetsAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.FreeOffsets, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadFreeOffsetsAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadGrayResponseCurveAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.GrayResponseCurve, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadGrayResponseCurveAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, 0.01)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, 0.1)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 2, 0.01)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 3, 0.001)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 4, 0.0001)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 5, 0.00001)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, 0.1)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 2, 0.01)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 3, 0.001)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 4, 0.0001)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 5, 0.00001)]
        public void GetGrayResponseUnit_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, double expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.GrayResponseUnit, type, data, byteOrder).Ifd;

            var result = ifd.GetGrayResponseUnit(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadHostComputerAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.HostComputer, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadHostComputerAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadImageDescriptionAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.ImageDescription, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadImageDescriptionAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerValues))]
        public void GetImageLength_ReturnsValue(ByteOrder byteOrder, TiffType type, uint? value)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.ImageLength, type, value, byteOrder).Ifd;

            var result = ifd.GetImageLength(byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerValues))]
        public void GetImageWidth_ReturnsValue(ByteOrder byteOrder, TiffType type, uint? value)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.ImageWidth, type, value, byteOrder).Ifd;

            var result = ifd.GetImageWidth(byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadMakeAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.Make, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadMakeAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadMaxSampleValueAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.MaxSampleValue, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadMaxSampleValueAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadMinSampleValueAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.MinSampleValue, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadMinSampleValueAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadModelAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.Model, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadModelAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, TiffNewSubfileType.FullImage)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 0, TiffNewSubfileType.FullImage)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 2, TiffNewSubfileType.SinglePage)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 3, TiffNewSubfileType.SinglePage | TiffNewSubfileType.Preview)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffNewSubfileType)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 0, TiffNewSubfileType.FullImage)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 2, TiffNewSubfileType.SinglePage)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 3, TiffNewSubfileType.SinglePage | TiffNewSubfileType.Preview)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffNewSubfileType)99)]
        public void GetNewSubfileType_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffNewSubfileType expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.NewSubfileType, type, data, byteOrder).Ifd;

            var result = ifd.GetNewSubfileType(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, TiffOrientation.TopLeft)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffOrientation.TopLeft)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 5, TiffOrientation.LeftTop)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffOrientation)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffOrientation.TopLeft)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 5, TiffOrientation.LeftTop)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffOrientation)99)]
        public void GetOrientation_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffOrientation expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.Orientation, type, data, byteOrder).Ifd;

            var result = ifd.GetOrientation(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, null)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffPhotometricInterpretation.BlackIsZero)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 34892, TiffPhotometricInterpretation.LinearRaw)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffPhotometricInterpretation)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffPhotometricInterpretation.BlackIsZero)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 34892, TiffPhotometricInterpretation.LinearRaw)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffPhotometricInterpretation)99)]
        public void GetPhotometricInterpretation_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffPhotometricInterpretation? expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.PhotometricInterpretation, type, data, byteOrder).Ifd;

            var result = ifd.GetPhotometricInterpretation(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, TiffPlanarConfiguration.Chunky)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffPlanarConfiguration.Chunky)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 2, TiffPlanarConfiguration.Planar)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffPlanarConfiguration)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffPlanarConfiguration.Chunky)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 2, TiffPlanarConfiguration.Planar)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffPlanarConfiguration)99)]
        public void GetPlanarConfiguraion_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffPlanarConfiguration expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.PlanarConfiguration, type, data, byteOrder).Ifd;

            var result = ifd.GetPlanarConfiguration(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, TiffResolutionUnit.Inch)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffResolutionUnit.None)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 2, TiffResolutionUnit.Inch)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffResolutionUnit)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffResolutionUnit.None)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 2, TiffResolutionUnit.Inch)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffResolutionUnit)99)]
        public void GetResolutionUnit_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffResolutionUnit expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.ResolutionUnit, type, data, byteOrder).Ifd;

            var result = ifd.GetResolutionUnit(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerValues))]
        public void GetRowsPerStrip_ReturnsValue(ByteOrder byteOrder, TiffType type, uint? value)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.RowsPerStrip, type, value, byteOrder).Ifd;

            var result = ifd.GetRowsPerStrip(byteOrder);

            if (value == null)
                value = UInt32.MaxValue; // Default value

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerValues))]
        public void GetSamplesPerPixel_ReturnsValue(ByteOrder byteOrder, TiffType type, uint? value)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.SamplesPerPixel, type, value, byteOrder).Ifd;

            var result = ifd.GetSamplesPerPixel(byteOrder);

            if (value == null)
                value = 1; // Default value

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadSoftwareAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.Software, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadSoftwareAsync(stream, byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadStripByteCountsAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.StripByteCounts, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadStripByteCountsAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerArrayValues))]
        public async Task ReadStripOffsetsAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, uint[] value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.StripOffsets, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadStripOffsetsAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, null)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffSubfileType.FullImage)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 2, TiffSubfileType.Preview)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffSubfileType)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffSubfileType.FullImage)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 2, TiffSubfileType.Preview)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffSubfileType)99)]
        public void GetSubfileType_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffSubfileType? expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.SubfileType, type, data, byteOrder).Ifd;

            var result = ifd.GetSubfileType(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, TiffThreshholding.None)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffThreshholding.None)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 2, TiffThreshholding.Ordered)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffThreshholding)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffThreshholding.None)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 2, TiffThreshholding.Ordered)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffThreshholding)99)]
        public void GetThreshholding_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffThreshholding expectedResult)
        {
            var ifd = TiffIfdBuilder.GenerateIfd(TiffTags.Threshholding, type, data, byteOrder).Ifd;

            var result = ifd.GetThreshholding(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleRationalValues))]
        public async Task ReadXResolutionAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, Rational? value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.XResolution, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadXResolutionAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleRationalValues))]
        public async Task ReadYResolutionAsync_ReturnsValue(ByteOrder byteOrder, TiffType type, Rational? value)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.YResolution, type, value, byteOrder);

            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadYResolutionAsync(stream, byteOrder);

            Assert.Equal(value, result);
        }
    }
}