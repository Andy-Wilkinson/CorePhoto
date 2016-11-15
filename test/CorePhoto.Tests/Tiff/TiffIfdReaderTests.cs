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

        [Theory]
        [MemberData(nameof(SampleAsciiValues))]
        public async Task ReadArtist_ReturnsValue(ByteOrder byteOrder, TiffType type, string data, string expectedResult)
        {
            var ifdStreamTuple = TiffHelper.GenerateTiffIfdWithStream(TiffTags.Artist, type, data, byteOrder);
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
        public void GetCompression_ReturnsValue(ByteOrder byteOrder, TiffType type, object data, TiffCompression expectedResult)
        {
            var ifd = TiffHelper.GenerateTiffIfd(TiffTags.Compression, type, data, byteOrder);

            var result = ifd.GetCompression(byteOrder);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerValues))]
        public void GetImageLength_ReturnsValue(ByteOrder byteOrder, TiffType type, uint? value)
        {
            var ifd = TiffHelper.GenerateTiffIfd(TiffTags.ImageLength, type, value, byteOrder);

            var result = ifd.GetImageLength(byteOrder);

            Assert.Equal(value, result);
        }

        [Theory]
        [MemberData(nameof(SampleIntegerValues))]
        public void GetImageWidth_ReturnsValue(ByteOrder byteOrder, TiffType type, uint? value)
        {
            var ifd = TiffHelper.GenerateTiffIfd(TiffTags.ImageWidth, type, value, byteOrder);

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
        public void GetNewSubfileType_ReturnsValue(ByteOrder byteOrder, TiffType type, object data, TiffNewSubfileType expectedResult)
        {
            var ifd = TiffHelper.GenerateTiffIfd(TiffTags.NewSubfileType, type, data, byteOrder);

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
        public void GetPhotometricInterpretation_ReturnsValue(ByteOrder byteOrder, TiffType type, object data, TiffPhotometricInterpretation? expectedResult)
        {
            var ifd = TiffHelper.GenerateTiffIfd(TiffTags.PhotometricInterpretation, type, data, byteOrder);

            var result = ifd.GetPhotometricInterpretation(byteOrder);

            Assert.Equal(expectedResult, result);
        }
    }
}