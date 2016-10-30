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
        public void GetExifIfdReference_ReturnsCorrectReference(TiffType type, ByteOrder byteOrder)
        {
            var tiffIfd = new TiffIfd
            {
                Entries = new[]
                {
                    new TiffIfdEntry { Tag = 10, Type = TiffType.Ascii, Count = 10},
                    TiffHelper.GenerateTiffIfdEntry(TiffTags.ExifIFD, type, 20u, byteOrder),
                    new TiffIfdEntry { Tag = 20, Type = TiffType.Ascii, Count = 10}
                }
            };

            var exifIfdReference = ExifReader.GetExifIfdReference(tiffIfd, byteOrder);

            Assert.Equal(new TiffIfdReference(20), exifIfdReference);
        }

        [Theory]
        [MemberDataAttribute(nameof(ByteOrderValues))]
        public void ReadExifIfdReference_ReturnsNullIfNoExifIfdExists(ByteOrder byteOrder)
        {
            var tiffIfd = new TiffIfd
            {
                Entries = new[]
                {
                    new TiffIfdEntry { Tag = 10, Type = TiffType.Ascii, Count = 10},
                    new TiffIfdEntry { Tag = 15, Type = TiffType.Ascii, Count = 10},
                    new TiffIfdEntry { Tag = 20, Type = TiffType.Ascii, Count = 10}
                }
            };

            var exifIfdReference = ExifReader.GetExifIfdReference(tiffIfd, byteOrder);

            Assert.Null(exifIfdReference);
        }
    }
}