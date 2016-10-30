using System.IO;
using System.Threading.Tasks;
using CorePhoto.IO;
using CorePhoto.Tiff;

namespace CorePhoto.Metadata.Exif
{
    public static class ExifReader
    {
        public static async Task<TiffIfd?> ReadExifIfdAsync(TiffIfd ifd, Stream stream, ByteOrder byteOrder)
        {
            var exifEntry = TiffReader.GetTiffIfdEntry(ifd, TiffTags.ExifIFD);

            if (exifEntry == null)
                return null;

            uint offset = TiffReader.GetIfdOffset(exifEntry.Value, byteOrder);
            return await TiffReader.ReadIfdAsync(stream, byteOrder, offset);
        }
    }
}