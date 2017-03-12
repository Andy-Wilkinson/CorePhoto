using CorePhoto.IO;
using CorePhoto.Tiff;
using CorePhoto.Tests.Helpers;
using Xunit;
using System.Threading.Tasks;

namespace CorePhoto.Tests.Tiff
{
    public class TiffReaderTests
    {
        public static object[][] ByteOrderValues = new[] { new object[] { ByteOrder.LittleEndian }, new object[] { ByteOrder.BigEndian } };

        [Fact]
        public async Task ReadHeaderAsync_ReadsCorrectly_LittleEndian()
        {
            var stream = new StreamBuilder(ByteOrder.LittleEndian)
                                     .WriteBytes(0x49, 0x49)
                                     .WriteInt16(42)
                                     .WriteUInt32(123456)
                                     .ToStream();

            var header = await TiffReader.ReadHeaderAsync(stream);

            Assert.Equal(ByteOrder.LittleEndian, header.ByteOrder);
            Assert.Equal(new TiffIfdReference(123456), header.FirstIfdReference);
        }

        [Fact]
        public async Task ReadHeaderAsync_ReadsCorrectly_BigEndian()
        {
            var stream = new StreamBuilder(ByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0x4D)
                                     .WriteInt16(42)
                                     .WriteUInt32(123456)
                                     .ToStream();

            var header = await TiffReader.ReadHeaderAsync(stream);

            Assert.Equal(ByteOrder.BigEndian, header.ByteOrder);
            Assert.Equal(new TiffIfdReference(123456), header.FirstIfdReference);
        }

        [Fact]
        public async Task ReadHeaderAsync_ThrowsException_IfFirstByteOrderMarkerIsUnknown()
        {
            var stream = new StreamBuilder(ByteOrder.BigEndian)
                                     .WriteBytes(0xAB, 0x4D)
                                     .WriteInt16(42)
                                     .WriteUInt32(123456)
                                     .ToStream();

            var e = await Assert.ThrowsAsync<ImageFormatException>(() => TiffReader.ReadHeaderAsync(stream));

            Assert.Equal("The TIFF byte order markers are invalid.", e.Message);
        }

        [Fact]
        public async Task ReadHeaderAsync_ThrowsException_IfSecondByteOrderMarkerIsUnknown()
        {
            var stream = new StreamBuilder(ByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0xAB)
                                     .WriteInt16(42)
                                     .WriteUInt32(123456)
                                     .ToStream();

            var e = await Assert.ThrowsAsync<ImageFormatException>(() => TiffReader.ReadHeaderAsync(stream));

            Assert.Equal("The TIFF byte order markers are invalid.", e.Message);
        }

        [Fact]
        public async Task ReadHeaderAsync_ThrowsException_MagicNumberIsIncorrect()
        {
            var stream = new StreamBuilder(ByteOrder.BigEndian)
                                     .WriteBytes(0x4D, 0x4D)
                                     .WriteInt16(123)
                                     .WriteUInt32(123456)
                                     .ToStream();

            var e = await Assert.ThrowsAsync<ImageFormatException>(() => TiffReader.ReadHeaderAsync(stream));

            Assert.Equal("The TIFF header does not contain the expected magic number.", e.Message);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public async Task ReadIfdAsync_ReadsCorrectlyWithReference(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteUInt32(123456)
                                    .ToStream();

            var ifdReference = new TiffIfdReference(20);
            var ifd = await TiffReader.ReadIfdAsync(ifdReference, stream, byteOrder);

            Assert.Equal(3, ifd.Entries.Length);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 2, Type = TiffType.Ascii, Count = 20, Value = new byte[] { 1, 2, 3, 4 } }, ifd.Entries[0]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 4, Type = TiffType.Short, Count = 40, Value = new byte[] { 2, 3, 4, 5 } }, ifd.Entries[1]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 6, Type = TiffType.Double, Count = 60, Value = new byte[] { 3, 4, 5, 6 } }, ifd.Entries[2]);
            Assert.Equal(new TiffIfdReference(123456), ifd.NextIfdReference);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ParseIfdEntry_ReadsCorrectly(ByteOrder byteOrder)
        {
            var bytes = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(167)
                                    .WriteInt16(5)
                                    .WriteUInt32(123456)
                                    .WriteBytes(new byte[] { 3, 4, 5, 6 })
                                    .ToBytes();

            var ifdEntry = TiffReader.ParseIfdEntry(bytes, 20, byteOrder);

            Assert.Equal(167, ifdEntry.Tag);
            Assert.Equal(TiffType.Rational, ifdEntry.Type);
            Assert.Equal(123456, ifdEntry.Count);
            Assert.Equal(new byte[] { 3, 4, 5, 6 }, ifdEntry.Value);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ParseIfdEntry_CanCompareToTiffTagEnumeration(ByteOrder byteOrder)
        {
            var bytes = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(274)
                                    .WriteInt16(5)
                                    .WriteUInt32(123456)
                                    .WriteBytes(new byte[] { 3, 4, 5, 6 })
                                    .ToBytes();

            var ifdEntry = TiffReader.ParseIfdEntry(bytes, 20, byteOrder);

            Assert.True(ifdEntry.Tag == TiffTags.Orientation);

        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ParseIfdEntry_CanSwitchWithTiffTagEnumeration(ByteOrder byteOrder)
        {
            var bytes = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(274)
                                    .WriteInt16(5)
                                    .WriteUInt32(123456)
                                    .WriteBytes(new byte[] { 3, 4, 5, 6 })
                                    .ToBytes();

            var ifdEntry = TiffReader.ParseIfdEntry(bytes, 20, byteOrder);

            bool matched = false;
            switch (ifdEntry.Tag)
            {
                case TiffTags.Orientation:
                    matched = true;
                    break;
            }

            Assert.True(matched);

        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public async Task ReadFirstIfdAsync_ReadsCorrectly(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteUInt32(123456)
                                    .ToStream();

            var header = new TiffHeader { FirstIfdReference = new TiffIfdReference(20) };
            var ifd = await TiffReader.ReadFirstIfdAsync(header, stream, byteOrder);

            Assert.Equal(3, ifd.Entries.Length);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 2, Type = TiffType.Ascii, Count = 20, Value = new byte[] { 1, 2, 3, 4 } }, ifd.Entries[0]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 4, Type = TiffType.Short, Count = 40, Value = new byte[] { 2, 3, 4, 5 } }, ifd.Entries[1]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 6, Type = TiffType.Double, Count = 60, Value = new byte[] { 3, 4, 5, 6 } }, ifd.Entries[2]);
            Assert.Equal(new TiffIfdReference(123456), ifd.NextIfdReference);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public async Task ReadNextIfdAsync_ReadsCorrectly(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteUInt32(123456)
                                    .ToStream();

            var previousIfd = new TiffIfd { NextIfdReference = new TiffIfdReference(20) };
            var ifd = (await TiffReader.ReadNextIfdAsync(previousIfd, stream, byteOrder)).Value;

            Assert.Equal(3, ifd.Entries.Length);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 2, Type = TiffType.Ascii, Count = 20, Value = new byte[] { 1, 2, 3, 4 } }, ifd.Entries[0]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 4, Type = TiffType.Short, Count = 40, Value = new byte[] { 2, 3, 4, 5 } }, ifd.Entries[1]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 6, Type = TiffType.Double, Count = 60, Value = new byte[] { 3, 4, 5, 6 } }, ifd.Entries[2]);
            Assert.Equal(new TiffIfdReference(123456), ifd.NextIfdReference);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public async Task ReadNextIfdAsync_ReturnsNullIfLastIfd(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteUInt32(123456)
                                    .ToStream();

            var previousIfd = new TiffIfd { NextIfdReference = null };
            var ifd = await TiffReader.ReadNextIfdAsync(previousIfd, stream, byteOrder);

            Assert.Null(ifd);
        }

        [Theory]
        [InlineDataAttribute(ByteOrder.LittleEndian, TiffType.Long)]
        [InlineDataAttribute(ByteOrder.LittleEndian, TiffType.Ifd)]
        [InlineDataAttribute(ByteOrder.BigEndian, TiffType.Long)]
        [InlineDataAttribute(ByteOrder.BigEndian, TiffType.Ifd)]
        public async Task ReadSubIfdReferencesAsync_ReadsCorrectly(ByteOrder byteOrder, TiffType type)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteUInt32(10)
                                    .WriteUInt32(42)
                                    .WriteUInt32(30)
                                    .ToStream();

            var tiffIfd = new TiffIfd
            {
                Entries = new[]
                {
                    new TiffIfdEntry { Tag = 10, Type = TiffType.Ascii, Count = 10},
                    new TiffIfdEntry { Tag = TiffTags.SubIFDs, Type = type, Count = 3, Value = ByteArrayHelper.ToBytes(20u, byteOrder) },
                    new TiffIfdEntry { Tag = 20, Type = TiffType.Ascii, Count = 10}
                }
            };

            var subIfdReferences = await TiffReader.ReadSubIfdReferencesAsync(tiffIfd, stream, byteOrder);

            Assert.Equal(new[] { new TiffIfdReference(10), new TiffIfdReference(42), new TiffIfdReference(30) }, subIfdReferences);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public async Task ReadSubIfdReferencesAsync_ReadsCorrectlyWithNoSubIfds(ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder).ToStream();

            var tiffIfd = new TiffIfd
            {
                Entries = new[]
                {
                    new TiffIfdEntry { Tag = 10, Type = TiffType.Ascii, Count = 10},
                    new TiffIfdEntry { Tag = 15, Type = TiffType.Ascii, Count = 10},
                    new TiffIfdEntry { Tag = 20, Type = TiffType.Ascii, Count = 10}
                }
            };

            var subIfdReferences = await TiffReader.ReadSubIfdReferencesAsync(tiffIfd, stream, byteOrder);

            Assert.Equal(new TiffIfdReference[] { }, subIfdReferences);
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
        [InlineDataAttribute(TiffType.Ifd, 4)]
        [InlineDataAttribute((TiffType)999, 0)]

        public void SizeOfDataType_ReturnsCorrectSize(TiffType type, int expectedSize)
        {
            var size = TiffReader.SizeOfDataType(type);

            Assert.Equal(expectedSize, size);
        }

        [Fact]
        public void GetIfdEntry_ReturnsEntryByTag()
        {
            var ifd = new TiffIfd
            {
                Entries = new[]
                {
                    new TiffIfdEntry { Tag = 10, Type = TiffType.Long, Count = 5},
                    new TiffIfdEntry { Tag = 15, Type = TiffType.Ascii, Count = 10},
                    new TiffIfdEntry { Tag = 20, Type = TiffType.Byte, Count = 15}
                }
            };

            var entry = TiffReader.GetTiffIfdEntry(ifd, 15).Value;

            Assert.Equal(15, entry.Tag);
            Assert.Equal(TiffType.Ascii, entry.Type);
            Assert.Equal(10, entry.Count);
        }

        [Fact]
        public void GetIfdEntry_ReturnsNullIfTagIsNotPresent()
        {
            var ifd = new TiffIfd
            {
                Entries = new[]
                {
                    new TiffIfdEntry { Tag = 10, Type = TiffType.Long, Count = 5},
                    new TiffIfdEntry { Tag = 15, Type = TiffType.Ascii, Count = 10},
                    new TiffIfdEntry { Tag = 20, Type = TiffType.Byte, Count = 15}
                }
            };

            var entry = TiffReader.GetTiffIfdEntry(ifd, 18);

            Assert.Null(entry);
        }
    }
}
