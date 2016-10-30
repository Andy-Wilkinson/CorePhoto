using CorePhoto.IO;
using CorePhoto.Tests.Helpers;
using CorePhoto.Tiff;
using Xunit;

namespace CorePhoto.Tests.Tiff
{
    public class TiffIfdReaderTests
    {
        [Theory]
        [InlineData(ByteOrder.LittleEndian, TiffType.Undefined, null, TiffCompression.None)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 1, TiffCompression.None)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 32773, TiffCompression.PackBits)]
        [InlineData(ByteOrder.LittleEndian, TiffType.Short, 99, (TiffCompression)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 1, TiffCompression.None)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 99, (TiffCompression)99)]
        [InlineData(ByteOrder.BigEndian, TiffType.Short, 32773, TiffCompression.PackBits)]
        public void GetCompression_ReturnsValue(ByteOrder byteOrder, TiffType type, int? data, TiffCompression expectedResult)
        {
            var ifd = TiffHelper.GenerateTiffIfd(TiffTags.Compression, type, data, byteOrder);

            var result = ifd.GetCompression(byteOrder);

            Assert.Equal(expectedResult, result);
        }
    }
}