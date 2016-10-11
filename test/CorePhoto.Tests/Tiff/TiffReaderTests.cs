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
            var stream = new StreamBuilder(StreamBuilderByteOrder.LittleEndian)
                                     .WriteBytes(0x49, 0x49)
                                     .WriteInt16(42)
                                     .WriteInt32(12345)
                                     .ToStream();

            var header = TiffReader.ReadHeader(stream);

            Assert.Equal(ByteOrder.LittleEndian, header.byteOrder);
            Assert.Equal(42, header.magicNumber);
            Assert.Equal(12345, header.firstIfdOffset);
        }

        [Fact]
        public void ReadHeader_ReadsCorrectly_BigEndian()
        {
            var stream = new StreamBuilder(StreamBuilderByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0x4D)
                                     .WriteInt16(42)
                                     .WriteInt32(12345)
                                     .ToStream();

            var header = TiffReader.ReadHeader(stream);

            Assert.Equal(ByteOrder.BigEndian, header.byteOrder);
            Assert.Equal(42, header.magicNumber);
            Assert.Equal(12345, header.firstIfdOffset);
        }
    }
}
