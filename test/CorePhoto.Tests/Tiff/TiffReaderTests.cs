using CorePhoto.IO;
using CorePhoto.Tiff;
using CorePhoto.Tests.Helpers;
using Xunit;

namespace CorePhoto.Tests.Tiff
{
    public class TiffReaderTests
    {
        public static object[][] ByteOrderValues = new[] { new object[] { ByteOrder.LittleEndian }, new object[] { ByteOrder.BigEndian } };

        [Fact]
        public void ReadHeader_ReadsCorrectly_LittleEndian()
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian)
                                     .WriteBytes(0x49, 0x49)
                                     .WriteInt16(42)
                                     .WriteInt32(123456)
                                     .ToStream();

            var header = TiffReader.ReadHeader(stream);

            Assert.Equal(ByteOrder.LittleEndian, header.ByteOrder);
            Assert.Equal(123456, header.FirstIfdOffset);
        }

        [Fact]
        public void ReadHeader_ReadsCorrectly_BigEndian()
        {
            var stream = new StreamBuilder(ByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0x4D)
                                     .WriteInt16(42)
                                     .WriteInt32(123456)
                                     .ToStream();

            var header = TiffReader.ReadHeader(stream);

            Assert.Equal(ByteOrder.BigEndian, header.ByteOrder);
            Assert.Equal(123456, header.FirstIfdOffset);
        }

        [Fact]
        public void ReadHeader_ThrowsException_IfFirstByteOrderMarkerIsUnknown()
        {
            var stream = new StreamBuilder(ByteOrder.BigEndian)
                                     .WriteBytes(0xAB, 0x4D)
                                     .WriteInt16(42)
                                     .WriteInt32(123456)
                                     .ToStream();

            var e = Assert.Throws<ImageFormatException>(() => TiffReader.ReadHeader(stream));

            Assert.Equal("The TIFF byte order markers are invalid.", e.Message);
        }

        [Fact]
        public void ReadHeader_ThrowsException_IfSecondByteOrderMarkerIsUnknown()
        {
            var stream = new StreamBuilder(ByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0xAB)
                                     .WriteInt16(42)
                                     .WriteInt32(123456)
                                     .ToStream();

            var e = Assert.Throws<ImageFormatException>(() => TiffReader.ReadHeader(stream));

            Assert.Equal("The TIFF byte order markers are invalid.", e.Message);
        }

        [Fact]
        public void ReadHeader_ThrowsException_MagicNumberIsIncorrect()
        {
            var stream = new StreamBuilder(ByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0x4D)
                                     .WriteInt16(123)
                                     .WriteInt32(123456)
                                     .ToStream();

            var e = Assert.Throws<ImageFormatException>(() => TiffReader.ReadHeader(stream));

            Assert.Equal("The TIFF header does not contain the expected magic number.", e.Message);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadIfd(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, 200)
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, 400)
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, 600)
                                    .WriteInt32(123456)
                                    .ToStream();

            var ifd = TiffReader.ReadIfd(stream, byteOrder);

            Assert.Equal(3, ifd.Entries.Length);
            Assert.Equal(new TiffIfdEntry { Tag = 2, Type = TiffType.Ascii, Count = 20, Value = 200 }, ifd.Entries[0]);
            Assert.Equal(new TiffIfdEntry { Tag = 4, Type = TiffType.Short, Count = 40, Value = 400 }, ifd.Entries[1]);
            Assert.Equal(new TiffIfdEntry { Tag = 6, Type = TiffType.Double, Count = 60, Value = 600 }, ifd.Entries[2]);
            Assert.Equal(123456, ifd.NextIfdOffset);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadIfdEntry(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WriteInt16(167)
                                    .WriteInt16(5)
                                    .WriteInt32(123456)
                                    .WriteInt32(234567)
                                    .ToStream();

            var ifdEntry = TiffReader.ReadIfdEntry(stream, byteOrder);

            Assert.Equal(167, ifdEntry.Tag);
            Assert.Equal(TiffType.Rational, ifdEntry.Type);
            Assert.Equal(123456, ifdEntry.Count);
            Assert.Equal(234567, ifdEntry.Value);
        }
    }
}
