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

            Assert.Equal(ByteOrder.LittleEndian, header.ByteOrder);
            Assert.Equal(12345, header.FirstIfdOffset);
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

            Assert.Equal(ByteOrder.BigEndian, header.ByteOrder);
            Assert.Equal(12345, header.FirstIfdOffset);
        }

        [Fact]
        public void ReadHeader_ThrowsException_IfFirstByteOrderMarkerIsUnknown()
        {
            var stream = new StreamBuilder(StreamBuilderByteOrder.BigEndian)
                                     .WriteBytes(0xAB, 0x4D)
                                     .WriteInt16(42)
                                     .WriteInt32(12345)
                                     .ToStream();

            var e = Assert.Throws<ImageFormatException>(() => TiffReader.ReadHeader(stream));

            Assert.Equal("The TIFF byte order markers are invalid.", e.Message);
        }

        [Fact]
        public void ReadHeader_ThrowsException_IfSecondByteOrderMarkerIsUnknown()
        {
            var stream = new StreamBuilder(StreamBuilderByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0xAB)
                                     .WriteInt16(42)
                                     .WriteInt32(12345)
                                     .ToStream();

            var e = Assert.Throws<ImageFormatException>(() => TiffReader.ReadHeader(stream));

            Assert.Equal("The TIFF byte order markers are invalid.", e.Message);
        }

        [Fact]
        public void ReadHeader_ThrowsException_MagicNumberIsIncorrect()
        {
            var stream = new StreamBuilder(StreamBuilderByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0x4D)
                                     .WriteInt16(123)
                                     .WriteInt32(12345)
                                     .ToStream();

            var e = Assert.Throws<ImageFormatException>(() => TiffReader.ReadHeader(stream));

            Assert.Equal("The TIFF header does not contain the expected magic number.", e.Message);
        }
    }
}
