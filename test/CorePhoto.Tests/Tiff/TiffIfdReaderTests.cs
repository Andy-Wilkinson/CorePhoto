using System.Threading.Tasks;
using CorePhoto.IO;
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

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadArtist_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffIfdBuilder.GenerateIfd(TiffTags.Artist, type, data, byteOrder);
            var ifd = ifdStreamTuple.Ifd;
            var stream = ifdStreamTuple.Stream;

            var result = await ifd.ReadArtist(stream, byteOrder);

            Assert.Equal(expectedResult, result);
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
    }
}