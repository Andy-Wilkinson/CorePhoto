using CorePhoto.IO;
using CorePhoto.Tiff;
using CorePhoto.Tests.Helpers;
using Xunit;
using System;

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
        public void ReadIfd_ReadsCorrectlyWithOffset(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteInt32(123456)
                                    .ToStream();

            var ifd = TiffReader.ReadIfd(stream, byteOrder, 20);

            Assert.Equal(3, ifd.Entries.Length);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)2, Type = TiffType.Ascii, Count = 20, Value = new byte[] { 1, 2, 3, 4 } }, ifd.Entries[0]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)4, Type = TiffType.Short, Count = 40, Value = new byte[] { 2, 3, 4, 5 } }, ifd.Entries[1]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)6, Type = TiffType.Double, Count = 60, Value = new byte[] { 3, 4, 5, 6 } }, ifd.Entries[2]);
            Assert.Equal(123456, ifd.NextIfdOffset);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadIfdEntry_ReadsCorrectly(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WriteInt16(167)
                                    .WriteInt16(5)
                                    .WriteInt32(123456)
                                    .WriteBytes(new byte[] { 3, 4, 5, 6 })
                                    .ToStream();

            var ifdEntry = TiffReader.ReadIfdEntry(stream, byteOrder);

            Assert.Equal((TiffTag)167, ifdEntry.Tag);
            Assert.Equal(TiffType.Rational, ifdEntry.Type);
            Assert.Equal(123456, ifdEntry.Count);
            Assert.Equal(new byte[] { 3, 4, 5, 6 }, ifdEntry.Value);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadFirstIfd_ReadsCorrectly(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteInt32(123456)
                                    .ToStream();

            var header = new TiffHeader { FirstIfdOffset = 20 };
            var ifd = TiffReader.ReadFirstIfd(header, stream, byteOrder);

            Assert.Equal(3, ifd.Entries.Length);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)2, Type = TiffType.Ascii, Count = 20, Value = new byte[] { 1, 2, 3, 4 } }, ifd.Entries[0]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)4, Type = TiffType.Short, Count = 40, Value = new byte[] { 2, 3, 4, 5 } }, ifd.Entries[1]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)6, Type = TiffType.Double, Count = 60, Value = new byte[] { 3, 4, 5, 6 } }, ifd.Entries[2]);
            Assert.Equal(123456, ifd.NextIfdOffset);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadNextIfd_ReadsCorrectly(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteInt32(123456)
                                    .ToStream();

            var previousIfd = new TiffIfd { NextIfdOffset = 20 };
            var ifd = TiffReader.ReadNextIfd(previousIfd, stream, byteOrder).Value;

            Assert.Equal(3, ifd.Entries.Length);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)2, Type = TiffType.Ascii, Count = 20, Value = new byte[] { 1, 2, 3, 4 } }, ifd.Entries[0]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)4, Type = TiffType.Short, Count = 40, Value = new byte[] { 2, 3, 4, 5 } }, ifd.Entries[1]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = (TiffTag)6, Type = TiffType.Double, Count = 60, Value = new byte[] { 3, 4, 5, 6 } }, ifd.Entries[2]);
            Assert.Equal(123456, ifd.NextIfdOffset);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadNextIfd_ReturnsNullIfLastIfd(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteInt32(123456)
                                    .ToStream();

            var previousIfd = new TiffIfd { NextIfdOffset = 00 };
            var ifd = TiffReader.ReadNextIfd(previousIfd, stream, byteOrder);

            Assert.Null(ifd);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 3, new byte[] { }, new byte[] { 1, 2, 3, 4 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, 3, new byte[] { }, new byte[] { 1, 2, 3, 4 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 4, new byte[] { }, new byte[] { 1, 2, 3, 4 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, 4, new byte[] { }, new byte[] { 1, 2, 3, 4 })]
        [InlineDataAttribute(ByteOrder.LittleEndian, new byte[] { 4, 0, 0, 0 }, 5, new byte[] { 0, 0, 0, 0, 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3, 4, 5 })]
        [InlineDataAttribute(ByteOrder.BigEndian, new byte[] { 0, 0, 0, 4 }, 5, new byte[] { 0, 0, 0, 0, 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3, 4, 5 })]
        public void ReadData_ReturnsExpectedData_Byte(ByteOrder byteOrder, byte[] ifdValue, int count, byte[] data, byte[] expectedValue)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WriteBytes(data)
                                    .ToStream();

            var entry = new TiffIfdEntry { Type = TiffType.Byte, Count = count, Value = ifdValue };
            var value = TiffReader.ReadData(entry, stream, byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void SizeOfHeader_AlwaysReturnsEightBytes()
        {
            var header = new TiffHeader();

            var size = TiffReader.SizeOfHeader(header);

            Assert.Equal(8, size);
        }

        [Fact]
        public void SizeOfIfdEntry_AlwaysReturnsTwelveBytes()
        {
            var ifdEntry = new TiffIfdEntry();

            var size = TiffReader.SizeOfIfdEntry(ifdEntry);

            Assert.Equal(12, size);
        }

        [Theory]
        [InlineDataAttribute(1, 18)]
        [InlineDataAttribute(2, 30)]
        [InlineDataAttribute(3, 42)]
        public void SizeOfIfd_ReturnsCorrectSize(int entryCount, int expectedSize)
        {
            var ifd = new TiffIfd { Entries = new TiffIfdEntry[entryCount] };

            var size = TiffReader.SizeOfIfd(ifd);

            Assert.Equal(expectedSize, size);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte, 1)]
        [InlineDataAttribute(TiffType.Ascii, 1)]
        [InlineDataAttribute(TiffType.Short, 2)]
        [InlineDataAttribute(TiffType.Long, 4)]
        [InlineDataAttribute(TiffType.Rational, 8)]
        [InlineDataAttribute(TiffType.SByte, 1)]
        [InlineDataAttribute(TiffType.Undefined, 1)]
        [InlineDataAttribute(TiffType.SShort, 2)]
        [InlineDataAttribute(TiffType.SLong, 4)]
        [InlineDataAttribute(TiffType.SRational, 8)]
        [InlineDataAttribute(TiffType.Float, 4)]
        [InlineDataAttribute(TiffType.Double, 8)]
        [InlineDataAttribute((TiffType)999, 0)]

        public void SizeOfDataType_ReturnsCorrectSize(TiffType type, int expectedSize)
        {
            var size = TiffReader.SizeOfDataType(type);

            Assert.Equal(expectedSize, size);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte, 1, 1)]
        [InlineDataAttribute(TiffType.Ascii, 1, 1)]
        [InlineDataAttribute(TiffType.Short, 1, 2)]
        [InlineDataAttribute(TiffType.Long, 1, 4)]
        [InlineDataAttribute(TiffType.Rational, 1, 8)]
        [InlineDataAttribute(TiffType.SByte, 1, 1)]
        [InlineDataAttribute(TiffType.Undefined, 1, 1)]
        [InlineDataAttribute(TiffType.SShort, 1, 2)]
        [InlineDataAttribute(TiffType.SLong, 1, 4)]
        [InlineDataAttribute(TiffType.SRational, 1, 8)]
        [InlineDataAttribute(TiffType.Float, 1, 4)]
        [InlineDataAttribute(TiffType.Double, 1, 8)]
        [InlineDataAttribute((TiffType)999, 1, 0)]

        public void SizeOfData_SingleItem_ReturnsCorrectSize(TiffType type, int count, int expectedSize)
        {
            var entry = new TiffIfdEntry { Type = type, Count = count };

            var size = TiffReader.SizeOfData(entry);

            Assert.Equal(expectedSize, size);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte, 15, 15)]
        [InlineDataAttribute(TiffType.Ascii, 20, 20)]
        [InlineDataAttribute(TiffType.Short, 18, 36)]
        [InlineDataAttribute(TiffType.Long, 4, 16)]
        [InlineDataAttribute(TiffType.Rational, 9, 72)]
        [InlineDataAttribute(TiffType.SByte, 5, 5)]
        [InlineDataAttribute(TiffType.Undefined, 136, 136)]
        [InlineDataAttribute(TiffType.SShort, 12, 24)]
        [InlineDataAttribute(TiffType.SLong, 15, 60)]
        [InlineDataAttribute(TiffType.SRational, 10, 80)]
        [InlineDataAttribute(TiffType.Float, 2, 8)]
        [InlineDataAttribute(TiffType.Double, 2, 16)]
        [InlineDataAttribute((TiffType)999, 1050, 0)]

        public void SizeOfData_Array_ReturnsCorrectSize(TiffType type, int count, int expectedSize)
        {
            var entry = new TiffIfdEntry { Type = type, Count = count };

            var size = TiffReader.SizeOfData(entry);

            Assert.Equal(expectedSize, size);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 1)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.LittleEndian, new byte[] { 255, 2, 3, 4 }, 255)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, 1)]
        [InlineDataAttribute(TiffType.Byte, ByteOrder.BigEndian, new byte[] { 255, 2, 3, 4 }, 255)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 0, 0, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 1, 0, 2, 3 }, 1)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, 256)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 2, 1, 2, 3 }, 258)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.LittleEndian, new byte[] { 255, 255, 2, 3 }, UInt16.MaxValue)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 0, 0, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, 1)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 1, 0, 2, 3 }, 256)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 1, 2, 2, 3 }, 258)]
        [InlineDataAttribute(TiffType.Short, ByteOrder.BigEndian, new byte[] { 255, 255, 2, 3 }, UInt16.MaxValue)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0 }, 1)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 1, 0, 0 }, 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 1, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 1 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 67305985)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255 }, UInt32.MaxValue)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1 }, 1)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 0, 1, 0 }, 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 0, 1, 0, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 1, 0, 0, 0 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 4, 3, 2, 1 }, 67305985)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255 }, UInt32.MaxValue)]
        public void GetInteger_ReturnsValue(TiffType type, ByteOrder byteOrder, byte[] data, uint expectedValue)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1, Value = data };

            var value = TiffReader.GetInteger(entry, byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.SByte)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SShort)]
        [InlineDataAttribute(TiffType.SLong)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute((TiffType)99)]
        public void GetInteger_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1 };

            var e = Assert.Throws<ImageFormatException>(() => TiffReader.GetInteger(entry, ByteOrder.LittleEndian));

            Assert.Equal($"A value of type '{type}' cannot be converted to an unsigned integer.", e.Message);
        }

        [Theory]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 1)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.LittleEndian, new byte[] { 255, 2, 3, 4 }, -1)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.BigEndian, new byte[] { 1, 2, 3, 4 }, 1)]
        [InlineDataAttribute(TiffType.SByte, ByteOrder.BigEndian, new byte[] { 255, 2, 3, 4 }, -1)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 0, 0, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 1, 0, 2, 3 }, 1)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 0, 1, 2, 3 }, 256)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 2, 1, 2, 3 }, 258)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.LittleEndian, new byte[] { 255, 255, 2, 3 }, -1)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 0, 0, 2, 3 }, 0)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 0, 1, 2, 3 }, 1)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 1, 0, 2, 3 }, 256)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 1, 2, 2, 3 }, 258)]
        [InlineDataAttribute(TiffType.SShort, ByteOrder.BigEndian, new byte[] { 255, 255, 2, 3 }, -1)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 1, 0, 0, 0 }, 1)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 0, 1, 0, 0 }, 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 0, 0, 1, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 0, 0, 0, 1 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 1, 2, 3, 4 }, 67305985)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.LittleEndian, new byte[] { 255, 255, 255, 255 }, -1)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 0 }, 0)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 0, 0, 0, 1 }, 1)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 0, 0, 1, 0 }, 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 0, 1, 0, 0 }, 256 * 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 1, 0, 0, 0 }, 256 * 256 * 256)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 4, 3, 2, 1 }, 67305985)]
        [InlineDataAttribute(TiffType.SLong, ByteOrder.BigEndian, new byte[] { 255, 255, 255, 255 }, -1)]
        public void GetSignedInteger_ReturnsValue(TiffType type, ByteOrder byteOrder, byte[] data, int expectedValue)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1, Value = data };

            var value = TiffReader.GetSignedInteger(entry, byteOrder);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineDataAttribute(TiffType.Byte)]
        [InlineDataAttribute(TiffType.Ascii)]
        [InlineDataAttribute(TiffType.Short)]
        [InlineDataAttribute(TiffType.Long)]
        [InlineDataAttribute(TiffType.Rational)]
        [InlineDataAttribute(TiffType.Undefined)]
        [InlineDataAttribute(TiffType.SRational)]
        [InlineDataAttribute(TiffType.Float)]
        [InlineDataAttribute(TiffType.Double)]
        [InlineDataAttribute((TiffType)99)]
        public void GetSignedInteger_ThrowsExceptionIfInvalidType(TiffType type)
        {
            var entry = new TiffIfdEntry { Type = type, Count = 1 };

            var e = Assert.Throws<ImageFormatException>(() => TiffReader.GetSignedInteger(entry, ByteOrder.LittleEndian));

            Assert.Equal($"A value of type '{type}' cannot be converted to a signed integer.", e.Message);
        }
    }
}
