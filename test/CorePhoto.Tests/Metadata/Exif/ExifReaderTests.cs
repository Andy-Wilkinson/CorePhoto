using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Metadata.Exif;
using CorePhoto.Tests.Helpers;
using CorePhoto.Tiff;
using Xunit;

namespace CorePhoto.Tests.Metadata.Exif
{
    public class ExifReaderTests
    {
        public static object[][] ByteOrderValues = new[] { new object[] { ByteOrder.LittleEndian }, new object[] { ByteOrder.BigEndian } };

        [Theory]
        [InlineDataAttribute(TiffType.Long, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.Long, ByteOrder.BigEndian)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.LittleEndian)]
        [InlineDataAttribute(TiffType.Ifd, ByteOrder.BigEndian)]
        public async Task ReadExifIfdAsync_ReadsCorrectly(TiffType type, ByteOrder byteOrder)
        {
            var stream = new StreamBuilder(byteOrder)
                                    .WritePadding(20)
                                    .WriteInt16(3)
                                    .WriteTiffIfdEntry(2, TiffType.Ascii, 20, new byte[] { 1, 2, 3, 4 })
                                    .WriteTiffIfdEntry(4, TiffType.Short, 40, new byte[] { 2, 3, 4, 5 })
                                    .WriteTiffIfdEntry(6, TiffType.Double, 60, new byte[] { 3, 4, 5, 6 })
                                    .WriteUInt32(123456)
                                    .ToStream();

            var tiffIfd = new TiffIfd
            {
                Entries = new[]
                {
                    new TiffIfdEntry { Tag = 10, Type = TiffType.Ascii, Count = 10},
                    TiffHelper.GenerateTiffIfdEntry(TiffTags.ExifIFD, type, 20u, byteOrder),
                    new TiffIfdEntry { Tag = 20, Type = TiffType.Ascii, Count = 10}
                }
            };

            var exifIfd = (await ExifReader.ReadExifIfdAsync(tiffIfd, stream, byteOrder)).Value;

            Assert.Equal(3, exifIfd.Entries.Length);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 2, Type = TiffType.Ascii, Count = 20, Value = new byte[] { 1, 2, 3, 4 } }, exifIfd.Entries[0]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 4, Type = TiffType.Short, Count = 40, Value = new byte[] { 2, 3, 4, 5 } }, exifIfd.Entries[1]);
            AssertTiff.Equal(new TiffIfdEntry { Tag = 6, Type = TiffType.Double, Count = 60, Value = new byte[] { 3, 4, 5, 6 } }, exifIfd.Entries[2]);
            Assert.Equal(123456u, exifIfd.NextIfdOffset);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public async Task ReadExifIfdAsync_ReturnsNullIfNoExifIfdExists(ByteOrder byteOrder)
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

            var exifIfd = await ExifReader.ReadExifIfdAsync(tiffIfd, stream, byteOrder);

            Assert.Null(exifIfd);
        }
    }
}