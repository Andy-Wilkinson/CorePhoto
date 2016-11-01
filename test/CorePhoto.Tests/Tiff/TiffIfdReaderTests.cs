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
                                                             new object[] { ByteOrder.LittleEndian, TiffType.Ascii, "First\0Second\0", "First\0Second" },
                                                             new object[] { ByteOrder.BigEndian, TiffType.Ascii, null, null }};

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
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffCompression)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 32773, TiffCompression.PackBits)]
        public void GetCompression_ReturnsValue(ByteOrder byteOrder, TiffType type, object data, TiffCompression expectedResult)
        {
            var ifd = TiffHelper.GenerateTiffIfd(TiffTags.Compression, type, data, byteOrder);

            var result = ifd.GetCompression(byteOrder);

            Assert.Equal(expectedResult, result);
        }
    }
}