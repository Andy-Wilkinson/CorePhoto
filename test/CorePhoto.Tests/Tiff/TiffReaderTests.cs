using CorePhoto.IO;
using CorePhoto.Tiff;
using CorePhoto.Tests.Helpers;
using Xunit;

namespace CorePhoto.Tests.Tiff
{
    public class TiffReaderTests
    {
        [Fact]
        public void ReadHeader_ReadsCorrectly_LittleEndian()
        {
            var stream = StreamHelper.CreateStreamLittleEndian()
                                     .WithBytes(0x49, 0x49)
                                     .WithInt16(42)
                                     .WithInt32(12345)
                                     .ToStream();

            var header = TiffReader.ReadHeader(stream);

            Assert.Equal(ByteOrder.LittleEndian, header.byteOrder);
            Assert.Equal(42, header.magicNumber);
            Assert.Equal(12345, header.firstIfdOffset);
        }

        [Fact]
        public void ReadHeader_ReadsCorrectly_BigEndian()
        {
            var stream = StreamHelper.CreateStreamBigEndian()
                                     .WithBytes(0x4D, 0x4D)
                                     .WithInt16(42)
                                     .WithInt32(12345)
                                     .ToStream();

            var header = TiffReader.ReadHeader(stream);

            Assert.Equal(ByteOrder.BigEndian, header.byteOrder);
            Assert.Equal(42, header.magicNumber);
            Assert.Equal(12345, header.firstIfdOffset);
        }
    }
}
